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
        /// </summary>
        public async Task<List<NominationWithScore>> GetRankedNominationsAsync(int cycleId)
        {
            // Load nominations with related data
            var nominations = await _db.Nominations
                .Include(n => n.ManagerScores)
                .Include(n => n.Evaluations)
                    .ThenInclude(e => e.EvaluationScores)
                .Include(n => n.AwardCycle)
                    .ThenInclude(ac => ac.AwardType)
                        .ThenInclude(at => at.Criteria)
                            .ThenInclude(c => c.SubCriteria)
                .Include(n => n.Employee)
                .Where(n => n.CycleId == cycleId)
                .ToListAsync();

            var result = new List<NominationWithScore>();

            foreach (var nom in nominations)
            {
                // Manager score (one per nomination) – we assume manager scores completed.
                var managerDict = ScoringHelper.ToScoreDictionary(nom.ManagerScores);
                var managerScore = ScoringHelper.CalculateWeightedScore(nom, managerDict);

                // Committee evaluations scores
                var committeeScores = new List<double>();
                foreach (var evaluation in nom.Evaluations)
                {
                    var evalDict = ScoringHelper.ToScoreDictionary(evaluation.EvaluationScores);
                    var evalScore = ScoringHelper.CalculateWeightedScore(nom, evalDict);
                    committeeScores.Add(evalScore);
                }

                double averageScore;
                if (committeeScores.Any())
                {
                    averageScore = (managerScore + committeeScores.Sum()) / (committeeScores.Count + 1);
                }
                else
                {
                    averageScore = managerScore; // Fallback if no evaluations
                }

                result.Add(new NominationWithScore
                {
                    Nomination = nom,
                    AverageScore = Math.Round(averageScore, 2)
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