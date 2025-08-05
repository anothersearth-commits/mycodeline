using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EOM.Web.Models;

[Table("VW_EOM_ATTENDANCE")]
public class AttendanceRecord
{
    [Column("EMP_NO")]
    public long EmployeeNumber { get; set; }
    
    [Column("ATT_DATE")]
    public DateTime AttendanceDate { get; set; }
    
    [Column("ATT_IN")]
    public string? AttendanceIn { get; set; }
    
    [Column("ATT_OUT")]
    public string? AttendanceOut { get; set; }
    
    [Column("DIFF")]
    public string? Difference { get; set; }
}