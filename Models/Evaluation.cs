using System.ComponentModel.DataAnnotations;

namespace EOM.Web.Models;

public class Evaluation
{
    [Key]
    public int EvaluationId { get; set; }
    
    public int NominationId { get; set; }
    
    public int CommitteeMemberId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Nomination Nomination { get; set; } = null!;
    public virtual CommitteeMember CommitteeMember { get; set; } = null!;
    public virtual ICollection<EvaluationScore> EvaluationScores { get; set; } = new List<EvaluationScore>();
}