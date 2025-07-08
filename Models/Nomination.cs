using System.ComponentModel.DataAnnotations;

namespace EOM.Web.Models;

public class Nomination
{
    [Key]
    public int NominationId { get; set; }
    
    public int CycleId { get; set; }
    
    public int EmployeeId { get; set; }
    
    public int ManagerId { get; set; }
    
    public string? SupportingDocPath { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual AwardCycle AwardCycle { get; set; } = null!;
    public virtual Employee Employee { get; set; } = null!;
    public virtual Employee Manager { get; set; } = null!;
    public virtual ICollection<ManagerScore> ManagerScores { get; set; } = new List<ManagerScore>();
    public virtual ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();
}