using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EOM.Web.Models
{
    [Table("EJADAH_CYCLES")]
    public class EjadahCycle
    {
        [Key]
        [Column("EJADAH_CYCLE_ID")]
        public int EjadahCycleId { get; set; }

        [Column("YEAR")]
        [Required]
        [Range(2020, 2050, ErrorMessage = "السنة يجب أن تكون بين 2020 و 2050")]
        public int Year { get; set; }

        [Column("HALF")]
        [Required]
        public int Half { get; set; }

        [Column("START_DATE")]
        [Required]
        [Display(Name = "تاريخ البداية")]
        public DateTime StartDate { get; set; }

        [Column("END_DATE")]
        [Required]
        [Display(Name = "تاريخ النهاية")]
        public DateTime EndDate { get; set; }

        [Column("IS_ACTIVE")]
        [Display(Name = "نشط")]
        public int IsActive { get; set; } = 0;

        [Column("CREATED_DATE")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Column("CREATED_BY")]
        [StringLength(100)]
        public string? CreatedBy { get; set; }

        // Navigation property
        public virtual ICollection<EjadahEmployeeScore> EjadahEmployeeScores { get; set; } = new List<EjadahEmployeeScore>();

        // Display properties
        [NotMapped]
        public string CycleName => $"دورة {Year} - النصف {(Half == 1 ? "الأول" : "الثاني")}";

        [NotMapped]
        public string HalfName => Half == 1 ? "النصف الأول" : "النصف الثاني";

        [NotMapped]
        public string StatusText => IsActive == 1 ? "نشط" : "غير نشط";
    }
}