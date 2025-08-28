using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EOM.Web.Data;
using EOM.Web.Models;

namespace EOM.Web.Services
{
    public class CycleRankingService
    {
        private readonly ApplicationDbContext _db;

        public CycleRankingService(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Returns a list of nominations within a cycle ordered by the average of manager + committee scores (descending).
        /// Simplified version that directly calculates averages without complex weighted scoring.
        /// </summary>
        public async Task<List<NominationWithScore>> GetRankedNominationsAsync(int cycleId)
        {
            // Use raw SQL for optimal performance with direct score calculation
            var query = @"
                SELECT 
                    n.NominationId,
                    n.EmployeeId,
                    n.ManagerId,
                    n.CycleId,
                    n.CreatedAt,
                    n.IsWinner,
                    n.WonAt,
                    n.SelectedByCommitteeMemberId,
                    n.Title,
                    n.IsSelfNomination,
                    n.InitiativeDetails,
                    n.AttachmentPath,
                    n.SupportingDocPath,
                    e.FirstName,
                    e.LastName,
                    e.JobTitle,
                    d.Name as DepartmentName,
                    d.Description as DepartmentDescription,
                    m.FirstName as ManagerFirstName,
                    m.LastName as ManagerLastName,
                    COALESCE(
                        (
                            -- Manager score (sum of all sub-criteria scores)
                            SELECT AVG(CAST(ms.Score AS FLOAT))
                            FROM ManagerScores ms
                            WHERE ms.NominationId = n.NominationId AND ms.Score IS NOT NULL
                        ), 0
                    ) as ManagerAvgScore,
                    COALESCE(
                        (
                            -- Committee average (average of all evaluation scores)
                            SELECT AVG(CAST(es.Score AS FLOAT))
                            FROM Evaluations ev
                            INNER JOIN EvaluationScores es ON ev.EvaluationId = es.EvaluationId
                            WHERE ev.NominationId = n.NominationId AND es.Score IS NOT NULL
                        ), 0
                    ) as CommitteeAvgScore
                FROM Nominations n
                INNER JOIN Employees e ON n.EmployeeId = e.EmployeeId
                LEFT JOIN Departments d ON e.DepartmentId = d.DepartmentId
                LEFT JOIN Employees m ON n.ManagerId = m.EmployeeId
                WHERE n.CycleId = {0}";

            var nominations = await _db.Nominations
                .FromSqlRaw(query, cycleId)
                .Include(n => n.Employee)
                    .ThenInclude(e => e.Department)
                .Include(n => n.Manager)
                .Include(n => n.GroupMembers)
                    .ThenInclude(gm => gm.Employee)
                .ToListAsync();

            var result = new List<NominationWithScore>();

            // Calculate simple average for each nomination
            foreach (var nom in nominations)
            {
                // Get the pre-calculated scores from the query
                var managerAvg = await _db.ManagerScores
                    .Where(ms => ms.NominationId == nom.NominationId && ms.Score != null)
                    .Select(ms => (double?)ms.Score)
                    .AverageAsync() ?? 0;

                var committeeAvg = await (from ev in _db.Evaluations
                                         join es in _db.EvaluationScores on ev.EvaluationId equals es.EvaluationId
                                         where ev.NominationId == nom.NominationId && es.Score != null
                                         select (double?)es.Score)
                                         .AverageAsync() ?? 0;

                // Simple average: (manager average + committee average) / 2
                // If no committee scores, use manager score only
                double finalScore;
                if (committeeAvg > 0)
                {
                    finalScore = (managerAvg + committeeAvg) / 2;
                }
                else
                {
                    finalScore = managerAvg;
                }

                result.Add(new NominationWithScore
                {
                    Nomination = nom,
                    AverageScore = Math.Round(finalScore, 2)
                });
            }

            return result.OrderByDescending(x => x.AverageScore).ToList();
        }

        /// <summary>
        /// Fast version using simplified direct score calculation.
        /// Sum all scores and average across manager + all committee members.
        /// </summary>
        public async Task<List<NominationWithScore>> GetRankedNominationsFastAsync(int cycleId)
        {
            // Load nominations with related data
            var nominations = await _db.Nominations
                .Include(n => n.Employee)
                    .ThenInclude(e => e.Department)
                .Include(n => n.Manager)
                .Include(n => n.GroupMembers)
                    .ThenInclude(gm => gm.Employee)
                .Where(n => n.CycleId == cycleId)
                .ToListAsync();

            if (!nominations.Any())
                return new List<NominationWithScore>();

            var nominationIds = nominations.Select(n => n.NominationId).ToList();

            // Get manager total scores (sum of all ManagerScores)
            var managerScores = await _db.ManagerScores
                .Where(ms => nominationIds.Contains(ms.NominationId) && ms.Score != null)
                .GroupBy(ms => ms.NominationId)
                .Select(g => new { 
                    NominationId = g.Key, 
                    TotalScore = g.Sum(ms => (double)ms.Score.Value) 
                })
                .ToDictionaryAsync(x => x.NominationId, x => x.TotalScore);

            // Get each committee member's total score (sum of their EvaluationScores)
            var committeeScores = await (from ev in _db.Evaluations
                                        join es in _db.EvaluationScores on ev.EvaluationId equals es.EvaluationId
                                        where nominationIds.Contains(ev.NominationId) && es.Score != null
                                        group es by new { ev.NominationId, ev.EvaluationId } into g
                                        select new {
                                            NominationId = g.Key.NominationId,
                                            EvaluationId = g.Key.EvaluationId,
                                            TotalScore = g.Sum(es => (double)es.Score.Value)
                                        })
                                        .GroupBy(x => x.NominationId)
                                        .Select(g => new {
                                            NominationId = g.Key,
                                            CommitteeScores = g.Select(x => x.TotalScore).ToList()
                                        })
                                        .ToDictionaryAsync(x => x.NominationId, x => x.CommitteeScores);

            var result = new List<NominationWithScore>();

            foreach (var nom in nominations)
            {
                var allScores = new List<double>();
                
                // Add manager score if exists
                if (managerScores.ContainsKey(nom.NominationId))
                    allScores.Add(managerScores[nom.NominationId]);
                
                // Add all committee member scores
                if (committeeScores.ContainsKey(nom.NominationId))
                    allScores.AddRange(committeeScores[nom.NominationId]);

                // Calculate average: (manager + committee1 + committee2 + ...) / total_evaluators
                double finalScore = allScores.Any() ? allScores.Average() : 0;

                result.Add(new NominationWithScore
                {
                    Nomination = nom,
                    AverageScore = Math.Round(finalScore, 1)
                });
            }

            return result.OrderByDescending(x => x.AverageScore).ToList();
        }
    }

    public class NominationWithScore
    {
        public Nomination Nomination { get; set; }
        public double AverageScore { get; set; }
    }
} 