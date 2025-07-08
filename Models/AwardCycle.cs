using System.ComponentModel.DataAnnotations;

namespace EOM.Web.Models;

public class AwardCycle
{
    [Key]
    public int CycleId { get; set; }
    
    public int AwardTypeId { get; set; }
    
    public int Month { get; set; }
    
    public int Year { get; set; }
    
    [Required]
    public DateTime NominationStart { get; set; }
    
    [Required]
    public DateTime NominationEnd { get; set; }
    
    [Required]
    public CycleStatus Status { get; set; } = CycleStatus.Pending;
    
    // Navigation properties
    public virtual AwardType AwardType { get; set; } = null!;
    public virtual ICollection<Nomination> Nominations { get; set; } = new List<Nomination>();
}

public enum CycleStatus
{
    Pending,
    Nomination,
    Evaluating,
    Closed,
    Published
}