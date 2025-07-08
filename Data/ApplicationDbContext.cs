using Microsoft.EntityFrameworkCore;
using EOM.Web.Models;

namespace EOM.Web.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // HR Tables (existing)
    public DbSet<Employee> Employees { get; set; }
    public DbSet<EmployeeManager> EmployeeManagers { get; set; }
    
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

        // Configure HR relationships
        builder.Entity<EmployeeManager>()
            .HasOne(em => em.Employee)
            .WithMany(e => e.Managers)
            .HasForeignKey(em => em.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<EmployeeManager>()
            .HasOne(em => em.Manager)
            .WithMany(e => e.ManagedEmployees)
            .HasForeignKey(em => em.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CommitteeMember>()
            .HasOne(cm => cm.Employee)
            .WithMany()
            .HasForeignKey(cm => cm.EmployeeId);

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

        builder.Entity<Employee>()
            .Property(e => e.Password)
            .HasMaxLength(100);
    }
}