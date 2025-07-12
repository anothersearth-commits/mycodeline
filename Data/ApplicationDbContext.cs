using Microsoft.EntityFrameworkCore;
using EOM.Web.Models;

namespace EOM.Web.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Employee model now maps to VW_EOM_EMPLOYEES view (replacing both Employees table and VwEomEmployees)
    public DbSet<Employee> Employees { get; set; }
    
    // HR Views
    public DbSet<VwEomDepartments> VwEomDepartments { get; set; }
    public DbSet<VwEomManagers> VwEomManagers { get; set; }
    
    // EOM Tables
    public DbSet<AwardType> AwardTypes { get; set; }
    public DbSet<AwardCycle> AwardCycles { get; set; }
    public DbSet<Criterion> Criteria { get; set; }
    public DbSet<SubCriteria> SubCriteria { get; set; }
    public DbSet<DepartmentQuota> DepartmentQuotas { get; set; }
    public DbSet<Nomination> Nominations { get; set; }
    public DbSet<ManagerScore> ManagerScores { get; set; }
    public DbSet<CommitteeMember> CommitteeMembers { get; set; }
    public DbSet<Evaluation> Evaluations { get; set; }
    public DbSet<EvaluationScore> EvaluationScores { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Note: Old HR table relationships removed - now using HR views

        // Configure Employee as a view (no foreign key constraints)
        builder.Entity<Employee>()
            .ToView("VW_EOM_EMPLOYEES");

        // Configure composite keys
        builder.Entity<DepartmentQuota>()
            .HasKey(dq => new { dq.DepartmentId, dq.AwardTypeId });

        builder.Entity<ManagerScore>()
            .HasKey(ms => new { ms.NominationId, ms.SubCriteriaId });

        builder.Entity<EvaluationScore>()
            .HasKey(es => new { es.EvaluationId, es.SubCriteriaId });

        // Configure relationships
        builder.Entity<AwardCycle>()
            .HasOne(ac => ac.AwardType)
            .WithMany(at => at.AwardCycles)
            .HasForeignKey(ac => ac.AwardTypeId);

        builder.Entity<Criterion>()
            .HasOne(c => c.AwardType)
            .WithMany(at => at.Criteria)
            .HasForeignKey(c => c.AwardTypeId);

        builder.Entity<SubCriteria>()
            .HasOne(sc => sc.Criterion)
            .WithMany(c => c.SubCriteria)
            .HasForeignKey(sc => sc.CriterionId);

        builder.Entity<DepartmentQuota>()
            .HasOne(dq => dq.AwardType)
            .WithMany(at => at.DepartmentQuotas)
            .HasForeignKey(dq => dq.AwardTypeId);

        builder.Entity<Nomination>()
            .HasOne(n => n.AwardCycle)
            .WithMany(ac => ac.Nominations)
            .HasForeignKey(n => n.CycleId);

        builder.Entity<Nomination>()
            .HasOne(n => n.Employee)
            .WithMany()
            .HasForeignKey(n => n.EmployeeId);

        builder.Entity<Nomination>()
            .HasOne(n => n.Manager)
            .WithMany()
            .HasForeignKey(n => n.ManagerId);

        builder.Entity<Nomination>()
            .HasOne(n => n.SelectedByCommitteeMember)
            .WithMany()
            .HasForeignKey(n => n.SelectedByCommitteeMemberId);

        builder.Entity<ManagerScore>()
            .HasOne(ms => ms.Nomination)
            .WithMany(n => n.ManagerScores)
            .HasForeignKey(ms => ms.NominationId);

        builder.Entity<ManagerScore>()
            .HasOne(ms => ms.SubCriteria)
            .WithMany(sc => sc.ManagerScores)
            .HasForeignKey(ms => ms.SubCriteriaId);

        builder.Entity<Evaluation>()
            .HasOne(e => e.Nomination)
            .WithMany(n => n.Evaluations)
            .HasForeignKey(e => e.NominationId);

        builder.Entity<Evaluation>()
            .HasOne(e => e.CommitteeMember)
            .WithMany(cm => cm.Evaluations)
            .HasForeignKey(e => e.CommitteeMemberId);

        builder.Entity<EvaluationScore>()
            .HasOne(es => es.Evaluation)
            .WithMany(e => e.EvaluationScores)
            .HasForeignKey(es => es.EvaluationId);

        builder.Entity<EvaluationScore>()
            .HasOne(es => es.SubCriteria)
            .WithMany(sc => sc.EvaluationScores)
            .HasForeignKey(es => es.SubCriteriaId);

        // Configure precision for decimal columns
        builder.Entity<Criterion>()
            .Property(c => c.WeightPercent)
            .HasPrecision(5, 2);

        // Configure string lengths
        builder.Entity<AwardType>()
            .Property(at => at.Name)
            .HasMaxLength(100);

        // Configure Employee to map to VW_EOM_EMPLOYEES view
        builder.Entity<Employee>()
            .HasKey(e => e.EmployeeId);

        builder.Entity<VwEomManagers>()
            .HasKey(m => m.ManagerId);

        builder.Entity<VwEomDepartments>()
            .HasKey(d => d.DepartmentId);

        builder.Entity<AwardType>()
            .Property(at => at.Description)
            .HasMaxLength(500);

        builder.Entity<Criterion>()
            .Property(c => c.Name)
            .HasMaxLength(200);

        builder.Entity<SubCriteria>()
            .Property(sc => sc.Name)
            .HasMaxLength(200);

        builder.Entity<SubCriteria>()
            .Property(sc => sc.SubCriteriaCode)
            .HasMaxLength(10);

        builder.Entity<SubCriteria>()
            .Property(sc => sc.GradingScale)
            .HasMaxLength(1000);

        builder.Entity<ManagerScore>()
            .Property(ms => ms.Note)
            .HasMaxLength(500);

        builder.Entity<EvaluationScore>()
            .Property(es => es.Note)
            .HasMaxLength(500);

        builder.Entity<Nomination>()
            .Property(n => n.SupportingDocPath)
            .HasMaxLength(500);

        builder.Entity<CommitteeMember>()
            .HasOne(cm => cm.Employee)
            .WithMany()
            .HasForeignKey(cm => cm.EmployeeId);

        // Employee model now maps to VW_EOM_EMPLOYEES view instead of physical table
    }
}