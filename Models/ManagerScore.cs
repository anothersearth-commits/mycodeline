using System.ComponentModel.DataAnnotations;

namespace EOM.Web.Models;

public class ManagerScore
{
    public int NominationId { get; set; }
    
    public int SubCriteriaId { get; set; }
    
    [Range(0, 100)]
    public byte Score { get; set; }
    
    [MaxLength(500)]
    public string? Note { get; set; }
    
    // Navigation properties
    public virtual Nomination Nomination { get; set; } = null!;
    public virtual SubCriteria SubCriteria { get; set; } = null!;
}