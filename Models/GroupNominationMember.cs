using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EOM.Web.Models;

[Table("GROUPNOMINATIONMEMBERS")]
public class GroupNominationMember
{
    [Key]
    [Column("GROUPMEMBERID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int GroupMemberId { get; set; }
    
    [Column("NOMINATIONID")]
    public int NominationId { get; set; }
    
    [Column("EMPLOYEEID")]
    public int EmployeeId { get; set; }
    
    // Navigation properties
    [ForeignKey(nameof(NominationId))]
    public virtual Nomination? Nomination { get; set; }
    
    [ForeignKey(nameof(EmployeeId))]
    public virtual Employee? Employee { get; set; }
}