using EOM.Web.Data;
using EOM.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace EOM.Web.Services
{
    public class EjadahEligibilityService : IEjadahEligibilityService
    {
        private readonly ApplicationDbContext _context;
        private static readonly string[] IneligibleScores = { "POOR", "MODERATE" };

        public EjadahEligibilityService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CanEmployeeBeNominatedAsync(int employeeId)
        {
            var latestScore = await GetLatestEjadahScoreAsync(employeeId);
            
            // If no Ejadah evaluation exists, employee can be nominated
            if (latestScore == null)
                return true;

            // Check if latest score allows nomination
            return !IneligibleScores.Contains(latestScore.Score);
        }

        public async Task<EjadahEmployeeScore?> GetLatestEjadahScoreAsync(int employeeId)
        {
            try
            {
                // Get score without include first
                var score = await _context.EjadahEmployeeScores
                    .Where(es => es.EmployeeId == employeeId)
                    .OrderByDescending(es => es.EjadahCycleId) // Use ID for ordering initially
                    .FirstOrDefaultAsync();
                
                if (score != null)
                {
                    // Get the cycle separately
                    var cycle = await _context.EjadahCycles
                        .FirstOrDefaultAsync(ec => ec.EjadahCycleId == score.EjadahCycleId);
                    
                    score.EjadahCycle = cycle;
                }
                
                return score;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting single Ejadah score for employee {employeeId}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Employee>> GetEligibleEmployeesAsync(long departmentId, int managerId)
        {
            // Get all employees in the department (excluding manager)
            var departmentEmployees = await _context.Employees
                .Where(e => e.DepartmentId == departmentId && e.EmployeeId != managerId)
                .ToListAsync();

            var eligibleEmployees = new List<Employee>();

            foreach (var employee in departmentEmployees)
            {
                var isEligible = await CanEmployeeBeNominatedAsync(employee.EmployeeId);
                if (isEligible)
                {
                    eligibleEmployees.Add(employee);
                }
            }

            return eligibleEmployees;
        }

        public async Task<Dictionary<int, string>> GetIneligibleEmployeesAsync(List<int> employeeIds)
        {
            var ineligibleEmployees = new Dictionary<int, string>();

            foreach (var employeeId in employeeIds)
            {
                var latestScore = await GetLatestEjadahScoreAsync(employeeId);
                
                if (latestScore != null && IneligibleScores.Contains(latestScore.Score))
                {
                    var scoreText = GetScoreArabicText(latestScore.Score);
                    var cycleText = $"{latestScore.EjadahCycle?.Year} - النصف {(latestScore.EjadahCycle?.Half == 1 ? "الأول" : "الثاني")}";
                    ineligibleEmployees.Add(employeeId, $"تقييم أجادة {scoreText} في دورة {cycleText}");
                }
            }

            return ineligibleEmployees;
        }

        public async Task<EjadahCycle?> GetLatestEjadahCycleAsync()
        {
            return await _context.EjadahCycles
                .OrderByDescending(ec => ec.Year)
                .ThenByDescending(ec => ec.Half)
                .FirstOrDefaultAsync();
        }

        private static string GetScoreArabicText(string score)
        {
            return score switch
            {
                "EXCELLENT" => "ممتاز",
                "VERY_GOOD" => "جيد جداً",
                "GOOD" => "جيد",
                "MODERATE" => "متوسط",
                "POOR" => "ضعيف",
                _ => score
            };
        }

        /// <summary>
        /// Bulk check eligibility for multiple employees (more efficient)
        /// </summary>
        public async Task<Dictionary<int, bool>> CheckMultipleEmployeeEligibilityAsync(List<int> employeeIds)
        {
            var eligibilityResults = new Dictionary<int, bool>();

            // Get all latest scores for the employees in one query
            var latestScores = await GetLatestScoresForEmployeesAsync(employeeIds);

            foreach (var employeeId in employeeIds)
            {
                if (latestScores.ContainsKey(employeeId))
                {
                    var score = latestScores[employeeId];
                    eligibilityResults[employeeId] = !IneligibleScores.Contains(score.Score);
                }
                else
                {
                    // No Ejadah score found, employee is eligible
                    eligibilityResults[employeeId] = true;
                }
            }

            return eligibilityResults;
        }

        private async Task<Dictionary<int, EjadahEmployeeScore>> GetLatestScoresForEmployeesAsync(List<int> employeeIds)
        {
            Console.WriteLine($"Getting Ejadah scores for {employeeIds.Count} employees");
            
            try
            {
                // Try without Include first to isolate the issue
                Console.WriteLine("Trying to read EjadahEmployeeScores without Include...");
                var allScoresWithoutInclude = await _context.EjadahEmployeeScores
                    .Where(es => employeeIds.Contains(es.EmployeeId))
                    .ToListAsync();
                
                Console.WriteLine($"Success! Found {allScoresWithoutInclude.Count} Ejadah score records without Include");
                
                // Now try to get the cycles separately
                Console.WriteLine("Getting EjadahCycles separately...");
                var cycleIds = allScoresWithoutInclude.Select(es => es.EjadahCycleId).Distinct().ToList();
                var cycles = await _context.EjadahCycles
                    .Where(ec => cycleIds.Contains(ec.EjadahCycleId))
                    .ToListAsync();
                
                Console.WriteLine($"Found {cycles.Count} Ejadah cycles");
                
                // Manually join the data
                var cyclesDict = cycles.ToDictionary(c => c.EjadahCycleId);
                foreach (var score in allScoresWithoutInclude)
                {
                    if (cyclesDict.TryGetValue(score.EjadahCycleId, out var cycle))
                    {
                        score.EjadahCycle = cycle;
                    }
                }
                
                // Group by employee and get latest score for each
                var latestScores = allScoresWithoutInclude
                    .GroupBy(es => es.EmployeeId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.OrderByDescending(es => es.EjadahCycle?.Year ?? 0)
                               .ThenByDescending(es => es.EjadahCycle?.Half ?? 0)
                               .First()
                    );

                Console.WriteLine($"Processed latest scores for {latestScores.Count} employees");
                return latestScores;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error details: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return new Dictionary<int, EjadahEmployeeScore>();
            }
        }
    }
}