using System.ComponentModel.DataAnnotations;

namespace EOM.Web.Models;

public class Criterion
{
    [Key]
    public int CriterionId { get; set; }
    
    public int AwardTypeId { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public decimal WeightPercent { get; set; }
    
    // Navigation properties
    public virtual AwardType AwardType { get; set; } = null!;
    public virtual ICollection<SubCriteria> SubCriteria { get; set; } = new List<SubCriteria>();
}