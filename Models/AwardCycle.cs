using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EOM.Web.Models;

public class AwardCycle
{
    [Key]
    public int CycleId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "يجب اختيار نوع الجائزة")]
    public int AwardTypeId { get; set; }
    
    [Required]
    [Range(1, 12, ErrorMessage = "يجب اختيار الشهر")]
    public int Month { get; set; }
    
    [Required]
    [Range(2020, 2030, ErrorMessage = "يجب اختيار السنة")]
    public int Year { get; set; }
    
    [Required(ErrorMessage = "يجب تحديد تاريخ بداية الترشيح")]
    public DateTime NominationStart { get; set; }
    
    [Required(ErrorMessage = "يجب تحديد تاريخ نهاية الترشيح")]
    public DateTime NominationEnd { get; set; }
    
    [Required]
    public CycleStatus Status { get; set; } = CycleStatus.Pending;
    
    // Navigation properties
    public virtual AwardType? AwardType { get; set; }
    public virtual ICollection<Nomination> Nominations { get; set; } = new List<Nomination>();
}

public enum CycleStatus
{
    Pending,
    Nomination,
    Evaluating,
    Closed,
    Published
}