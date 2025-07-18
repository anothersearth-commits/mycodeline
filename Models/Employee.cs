using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EOM.Web.Models;

// Employee model now maps to VW_EOM_EMPLOYEES view instead of Employees table
// [Table] removed - using .ToView() configuration in DbContext instead
public class Employee
{
    [Key]
    [Column("EMPLOYEEID")]
    public int EmployeeId { get; set; }
    
    [Column("FIRSTNAME")]
    public string FirstName { get; set; } = string.Empty;
    
    [Column("LASTNAME")]
    public string? LastName { get; set; }
    
    [Column("EMAIL")]
    public string? Email { get; set; }
    
    [Column("DEPARTMENTID")]
    public long DepartmentId { get; set; }
    
    [Column("JOBTITLE")]
    public string? JobTitle { get; set; }
    
    [Column("HIREDATE")]
    public DateTime? HireDate { get; set; }
    
    [Column("ACTIVEDIRECTORYID")]
    public string? ActiveDirectoryId { get; set; }
    
    [Column("PASSWORD")]
    public string? Password { get; set; }
    
    [Column("ISACTIVE")]
    public int IsActive { get; set; } = 1;
    
    [Column("PHONENUMBER")]
    public string? PhoneNumber { get; set; }
    
    [Column("MANAGERID")]
    public int? ManagerId { get; set; }
    
    [Column("MANAGERNAME")]
    public string? ManagerName { get; set; }
    
    [Column("IS_MANAGER")]
    public int IsManager { get; set; } = 0;
    
    // Navigation property to department
    public virtual VwEomDepartments? Department { get; set; }
}