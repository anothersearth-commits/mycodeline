using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EOM.Web.Models
{
    /// <summary>
    /// Represents AI-generated motivational messages and advice for employee objectives
    /// </summary>
    public class AiGeneratedMessage
    {
        [Key]
        public long AiMessageId { get; set; }

        [Required]
        public long ObjectiveId { get; set; }

        [Required]
        public long EmployeeId { get; set; } // Denormalized for performance

        [Required]
        public int ObjectiveCycleId { get; set; } // Denormalized for performance

        [Required]
        public string MessageBody { get; set; } = string.Empty;

        [Required]
        public string AdviceBody { get; set; } = string.Empty;

        [StringLength(50)]
        public string? StyleTag { get; set; }

        [StringLength(50)]
        public string? ModelName { get; set; }

        [Required]
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("ObjectiveId")]
        public virtual Objective Objective { get; set; } = null!;

        // Computed properties
        [NotMapped]
        public int MessageWordCount => MessageBody.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

        [NotMapped]
        public bool IsValidMessage => MessageWordCount <= 35; // Allow some flexibility

        [NotMapped]
        public string FormattedGeneratedAt => GeneratedAt.ToString("yyyy-MM-dd HH:mm");

        [NotMapped]
        public string StyleDisplayName => StyleTag switch
        {
            "Formal" => "رسمي",
            "Inspirational" => "تحفيزي",
            "Encouraging" => "مشجع",
            "Professional" => "مهني",
            _ => "عام"
        };

        // Validation methods
        public bool ContainsDateReferences()
        {
            var content = $"{MessageBody} {AdviceBody}".ToLower();
            var datePatterns = new[] { "يوم", "أسبوع", "شهر", "سنة", "متبق", "باق", "2024", "2025" };
            return datePatterns.Any(pattern => content.Contains(pattern));
        }

        public int CountEmojis()
        {
            var content = MessageBody;
            return content.Count(c => char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.OtherSymbol);
        }

        public bool IsValidFormat()
        {
            return !string.IsNullOrWhiteSpace(MessageBody) && 
                   !string.IsNullOrWhiteSpace(AdviceBody) && 
                   MessageWordCount <= 35 && 
                   CountEmojis() <= 1 && 
                   !ContainsDateReferences();
        }
    }
}