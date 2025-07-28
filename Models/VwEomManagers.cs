using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EOM.Web.Models;

[Table("VW_EOM_MANAGERS")]
public class VwEomManagers
{
    [Column("MANAGERID")]
    public int ManagerId { get; set; }

    [Column("MANAGERNAME")]
    public string ManagerName { get; set; } = string.Empty;

    [Column("MANAGERNAME_AR")]
    public string? ManagerNameAr { get; set; }

    [Column("EMAIL")]
    public string? Email { get; set; }

    [Column("DEPARTMENTID")]
    public long DepartmentId { get; set; }

    [Column("DEPARTMENTNAME")]
    public string? DepartmentName { get; set; }

    [Column("JOBTITLE")]
    public string? JobTitle { get; set; }

    [Column("PHONE")]
    public string? Phone { get; set; }

    [Column("ACTIVEDIRECTORYID")]
    public string? ActiveDirectoryId { get; set; }
}