using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EOM.Web.Models
{
    [Table("EJADAH_EMPLOYEE_SCORES")]
    public class EjadahEmployeeScore
    {
        [Key]
        [Column("EJADAH_EMPLOYEE_SCORE_ID")]
        public int EjadahEmployeeScoreId { get; set; }

        [Column("EJADAH_CYCLE_ID")]
        [Required]
        public int EjadahCycleId { get; set; }

        [Column("EMPLOYEE_ID")]
        [Required]
        public int EmployeeId { get; set; }

        [Column("SCORE")]
        [Required]
        [StringLength(20)]
        [Display(Name = "التقييم")]
        public string Score { get; set; } = string.Empty;

        [Column("SCORE_NUMERIC")]
        [Range(0, 100, ErrorMessage = "الدرجة الرقمية يجب أن تكون بين 0 و 100")]
        [Display(Name = "الدرجة الرقمية")]
        public decimal? ScoreNumeric { get; set; }

        // Navigation properties
        [ForeignKey("EjadahCycleId")]
        public virtual EjadahCycle? EjadahCycle { get; set; }

        // Note: Employee navigation removed due to type mismatch (decimal vs int)

        // Display properties
        [NotMapped]
        public string ScoreArabic => Score switch
        {
            "EXCELLENT" => "ممتاز",
            "VERY_GOOD" => "جيد جداً",
            "GOOD" => "جيد",
            "MODERATE" => "متوسط",
            "POOR" => "ضعيف",
            _ => Score
        };

        [NotMapped]
        public string ScoreClass => Score switch
        {
            "EXCELLENT" => "success",
            "VERY_GOOD" => "primary",
            "GOOD" => "info",
            "MODERATE" => "warning",
            "POOR" => "danger",
            _ => "secondary"
        };

        [NotMapped]
        public bool IsEligibleForNomination => Score is not ("POOR" or "MODERATE");

        [NotMapped]
        public string EligibilityStatus => IsEligibleForNomination ? "مؤهل للترشيح" : "غير مؤهل للترشيح";

        // Static methods for score validation
        public static List<string> GetValidScores()
        {
            return new List<string> { "EXCELLENT", "VERY_GOOD", "GOOD", "MODERATE", "POOR" };
        }

        public static Dictionary<string, string> GetScoreDescriptions()
        {
            return new Dictionary<string, string>
            {
                { "EXCELLENT", "ممتاز" },
                { "VERY_GOOD", "جيد جداً" },
                { "GOOD", "جيد" },
                { "MODERATE", "متوسط" },
                { "POOR", "ضعيف" }
            };
        }

        public static List<string> GetIneligibleScores()
        {
            return new List<string> { "POOR", "MODERATE" };
        }
    }
}