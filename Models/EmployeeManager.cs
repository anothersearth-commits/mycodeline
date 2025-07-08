using System.ComponentModel.DataAnnotations;

namespace EOM.Web.Models;

public class EmployeeManager
{
    [Key]
    public int Id { get; set; }
    
    public int EmployeeId { get; set; }
    
    public int ManagerId { get; set; }
    
    public DateTime StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual Employee Employee { get; set; } = null!;
    public virtual Employee Manager { get; set; } = null!;
}