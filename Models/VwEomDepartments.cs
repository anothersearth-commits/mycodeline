using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EOM.Web.Models;

[Table("VW_EOM_DEPARTMENTS")]
public class VwEomDepartments
{
    [Column("DEPARTMENTID")]
    public long DepartmentId { get; set; }

    [Column("NAME")]
    public string Name { get; set; } = string.Empty;

    [Column("DESCRIPTION")]
    public string? Description { get; set; }

    [Column("ISACTIVE")]
    public int IsActive { get; set; } = 1;
}