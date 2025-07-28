using System.ComponentModel.DataAnnotations;

namespace EOM.Web.Models;

public class SubCriteria
{
    [Key]
    public int SubCriteriaId { get; set; }
    
    public int CriterionId { get; set; }
    
    [Required]
    [MaxLength(10)]
    public string SubCriteriaCode { get; set; } = string.Empty; // e.g., "1.1", "1.2", "2.1"
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [Range(0, 100)]
    public int MaxScore { get; set; }
    
    [MaxLength(1000)]
    public string? GradingScale { get; set; } // JSON string of grading scale
    
    // Navigation properties
    public virtual Criterion Criterion { get; set; } = null!;
    public virtual ICollection<ManagerScore> ManagerScores { get; set; } = new List<ManagerScore>();
    public virtual ICollection<EvaluationScore> EvaluationScores { get; set; } = new List<EvaluationScore>();
}