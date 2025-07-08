namespace EOM.Web.Models;

public class DepartmentQuota
{
    public int DepartmentId { get; set; }
    
    public int AwardTypeId { get; set; }
    
    public int Quota { get; set; }
    
    public int MaxNominations { get; set; }
    
    // Navigation properties
    public virtual AwardType AwardType { get; set; } = null!;
}