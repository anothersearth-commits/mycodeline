using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EOM.Web.Models;

public class Administrator
{
    [Key]
    public int AdministratorId { get; set; }
    
    public int EmployeeId { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Navigation property to employee record
    [ForeignKey(nameof(EmployeeId))]
    public virtual Employee? Employee { get; set; }
}