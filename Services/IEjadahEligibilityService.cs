using EOM.Web.Models;

namespace EOM.Web.Services
{
    public interface IEjadahEligibilityService
    {
        /// <summary>
        /// Check if an employee can be nominated based on their latest Ejadah score
        /// </summary>
        /// <param name="employeeId">Employee ID to check</param>
        /// <returns>True if eligible for nomination, False if ineligible</returns>
        Task<bool> CanEmployeeBeNominatedAsync(int employeeId);

        /// <summary>
        /// Get the latest Ejadah score for an employee
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <returns>Latest EjadahEmployeeScore or null if no evaluation exists</returns>
        Task<EjadahEmployeeScore?> GetLatestEjadahScoreAsync(int employeeId);

        /// <summary>
        /// Get employees who are eligible for nomination in a specific department
        /// </summary>
        /// <param name="departmentId">Department ID</param>
        /// <param name="managerId">Manager ID (excluded from results)</param>
        /// <returns>List of eligible employees</returns>
        Task<List<Employee>> GetEligibleEmployeesAsync(long departmentId, int managerId);

        /// <summary>
        /// Get ineligible employees with their restriction reason
        /// </summary>
        /// <param name="employeeIds">List of employee IDs to check</param>
        /// <returns>Dictionary of employee ID and restriction message</returns>
        Task<Dictionary<int, string>> GetIneligibleEmployeesAsync(List<int> employeeIds);

        /// <summary>
        /// Get the latest active Ejadah cycle
        /// </summary>
        /// <returns>Latest EjadahCycle or null if none exists</returns>
        Task<EjadahCycle?> GetLatestEjadahCycleAsync();

        /// <summary>
        /// Bulk check eligibility for multiple employees (more efficient)
        /// </summary>
        /// <param name="employeeIds">List of employee IDs to check</param>
        /// <returns>Dictionary of employee ID and eligibility status</returns>
        Task<Dictionary<int, bool>> CheckMultipleEmployeeEligibilityAsync(List<int> employeeIds);
    }
}