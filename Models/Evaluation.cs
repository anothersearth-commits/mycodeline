using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace EOM.Web.Models;

public class Evaluation
{
    [Key]
    public int EvaluationId { get; set; }
    
    public int NominationId { get; set; }
    
    public int CommitteeMemberId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    [ValidateNever]
    public virtual Nomination Nomination { get; set; } = null!;
    [ValidateNever]
    public virtual CommitteeMember CommitteeMember { get; set; } = null!;
    [ValidateNever]
    public virtual ICollection<EvaluationScore> EvaluationScores { get; set; } = new List<EvaluationScore>();
}