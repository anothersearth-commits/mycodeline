using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EOM.Web.Models;

public class Nomination
{
    [Key]
    public int NominationId { get; set; }
    
    public int CycleId { get; set; }
    
    public int EmployeeId { get; set; }
    
    public int? ManagerId { get; set; } // Nullable for self-nominations
    
    public string? SupportingDocPath { get; set; }
    
    // Self-nomination fields
    public bool IsSelfNomination { get; set; } = false;
    public string? Title { get; set; } // Title of the self-nomination
    public string? InitiativeDetails { get; set; } // Details about the initiative/innovation
    public string? AttachmentPath { get; set; } // PDF attachment for self-nominations
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Winner tracking (0 = not winner, 1 = final winner, 2 = preliminary winner)
    public int IsWinner { get; set; } = 0;
    public DateTime? WonAt { get; set; }
    public int? SelectedByCommitteeMemberId { get; set; }
    
    // Navigation properties
    public virtual AwardCycle? AwardCycle { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    public virtual Employee? Employee { get; set; }

    [ForeignKey(nameof(ManagerId))]
    public virtual Employee? Manager { get; set; }
    
    // Group nomination support
    public virtual ICollection<GroupNominationMember> GroupMembers { get; set; } = new List<GroupNominationMember>();

    [ForeignKey(nameof(SelectedByCommitteeMemberId))]
    public virtual Employee? SelectedByCommitteeMember { get; set; }
    public virtual ICollection<ManagerScore> ManagerScores { get; set; } = new List<ManagerScore>();
    public virtual ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();
}