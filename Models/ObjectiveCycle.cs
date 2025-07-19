using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EOM.Web.Models
{
    /// <summary>
    /// Represents a half-yearly objective cycle (2 per year: Jan-Jun, Jul-Dec)
    /// </summary>
    public class ObjectiveCycle
    {
        [Key]
        public int ObjectiveCycleId { get; set; }

        [Required]
        [Range(2020, 2100)]
        public int Year { get; set; }

        [Required]
        [Range(1, 2)]
        public int Half { get; set; } // 1 = Jan-Jun, 2 = Jul-Dec

        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Required]
        public bool IsActive { get; set; } = false;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Objective> Objectives { get; set; } = new List<Objective>();
        public virtual ICollection<AiGeneratedMessage> AiGeneratedMessages { get; set; } = new List<AiGeneratedMessage>();

        // Computed properties
        [NotMapped]
        public string HalfName => Half == 1 ? "النصف الأول" : "النصف الثاني";

        [NotMapped]
        public string DisplayName => $"{Year} - {HalfName}";

        [NotMapped]
        public string PeriodDescription => Half == 1 ? "يناير - يونيو" : "يوليو - ديسمبر";
    }
}