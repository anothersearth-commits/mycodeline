using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EOM.Web.Models;

public class CommitteeMember
{
    [Key]
    public int Id { get; set; }
    
    public int EmployeeId { get; set; }
    
    // Award type this committee membership applies to
    public int AwardTypeId { get; set; }
    
    // Directorate this committee member evaluates for this award type
    // For Mode A (by-directorate committees), every member for (AwardTypeId, Directorate)
    // evaluates all nominations for employees in that directorate.
    public int Directorate { get; set; }
    
    public DateTime StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
    
    public bool IsActive { get; set; } = true;

    // Navigation property to employee record (view)
    [ForeignKey(nameof(EmployeeId))]
    public virtual Employee? Employee { get; set; }
    
    // Navigation to award type
    public virtual AwardType? AwardType { get; set; }
    
    // Employee data resolved via manual lookup from HR view when needed
    public virtual ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();
}