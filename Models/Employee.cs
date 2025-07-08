using System.ComponentModel.DataAnnotations;

namespace EOM.Web.Models;

public class Employee
{
    [Key]
    public int EmployeeId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? PhoneNumber { get; set; }
    
    public int DepartmentId { get; set; }
    public virtual Department? Department { get; set; }
    
    [MaxLength(100)]
    public string? JobTitle { get; set; }
    
    public DateTime HireDate { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // AD User ID for authentication
    [MaxLength(200)]
    public string? ActiveDirectoryId { get; set; }
    
    // Temporary password field for development (will be removed when AD is integrated)
    [MaxLength(100)]
    public string? Password { get; set; }
    
    
    // Navigation properties
    public virtual ICollection<EmployeeManager> ManagedEmployees { get; set; } = new List<EmployeeManager>();
    public virtual ICollection<EmployeeManager> Managers { get; set; } = new List<EmployeeManager>();
}