namespace EOM.Web.Models;

public class DepartmentQuota
{
    public long DepartmentId { get; set; }
    
    public int AwardTypeId { get; set; }
    
    public int MaxNominations { get; set; }
    
    // Navigation properties
    public virtual AwardType AwardType { get; set; } = null!;
}