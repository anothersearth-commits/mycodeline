using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EOM.Web.Models
{
    /// <summary>
    /// Represents an individual employee objective imported from external systems
    /// </summary>
    public class Objective
    {
        [Key]
        public long ObjectiveId { get; set; }

        [Required]
        public int ObjectiveCycleId { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int MainGoalId { get; set; }

        [Required]
        [StringLength(500)]
        public string ObjectiveTitle { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Classification { get; set; }

        public string? ResultDescription { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal? WeightScore { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal? ThresholdExceeds { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal? ThresholdMeets { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal? ThresholdBelow { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal? ActualScore { get; set; }

        [StringLength(500)]
        public string? HighLevelGoal { get; set; }

        [StringLength(100)]
        public string? Category { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ObjectiveCycleId")]
        public virtual ObjectiveCycle ObjectiveCycle { get; set; } = null!;

        public virtual ICollection<AiGeneratedMessage> AiGeneratedMessages { get; set; } = new List<AiGeneratedMessage>();

        // Computed properties
        [NotMapped]
        public string TruncatedDescription => ResultDescription?.Length > 200 
            ? ResultDescription[..200] + "..." 
            : ResultDescription ?? string.Empty;

        [NotMapped]
        public bool HasScoring => WeightScore.HasValue || ThresholdExceeds.HasValue || ThresholdMeets.HasValue || ThresholdBelow.HasValue;

        [NotMapped]
        public string ScoreStatus
        {
            get
            {
                if (!ActualScore.HasValue) return "غير مقيّم";
                if (!ThresholdExceeds.HasValue || !ThresholdMeets.HasValue) return "مقيّم";
                
                if (ActualScore >= ThresholdExceeds) return "يفوق التوقعات";
                if (ActualScore >= ThresholdMeets) return "يحقق التوقعات";
                return "دون التوقعات";
            }
        }
    }
}