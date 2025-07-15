using System.ComponentModel.DataAnnotations;

namespace EOM.Web.Models;

public class AwardType
{
    [Key]
    public int AwardTypeId { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    [Required]
    public int WinnerCount { get; set; } = 1;
    
    // Navigation properties
    public virtual ICollection<AwardCycle> AwardCycles { get; set; } = new List<AwardCycle>();
    public virtual ICollection<Criterion> Criteria { get; set; } = new List<Criterion>();
    public virtual ICollection<DepartmentQuota> DepartmentQuotas { get; set; } = new List<DepartmentQuota>();
}