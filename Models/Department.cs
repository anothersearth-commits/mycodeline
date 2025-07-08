using System.ComponentModel.DataAnnotations;

namespace EOM.Web.Models;

public class Department
{
    [Key]
    public int DepartmentId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public virtual ICollection<DepartmentQuota> DepartmentQuotas { get; set; } = new List<DepartmentQuota>();
}