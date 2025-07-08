using System.ComponentModel.DataAnnotations;

namespace EOM.Web.Models;

public class CommitteeMember
{
    [Key]
    public int Id { get; set; }
    
    public int EmployeeId { get; set; }
    
    public DateTime StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual Employee Employee { get; set; } = null!;
    public virtual ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();
}