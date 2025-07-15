using Microsoft.EntityFrameworkCore;
using EOM.Web.Models;

namespace EOM.Web.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Employee model now maps to VW_EOM_EMPLOYEES view (existing Oracle view)
    public DbSet<Employee> Employees { get; set; }
    
    // Department view for HR integration
    public DbSet<VwEomDepartments> Departments { get; set; }
    
    // EOM Tables
    public DbSet<AwardType> AwardTypes { get; set; }
    public DbSet<AwardCycle> AwardCycles { get; set; }
    public DbSet<Criterion> Criteria { get; set; }
    public DbSet<SubCriteria> SubCriteria { get; set; }
    public DbSet<DepartmentQuota> DepartmentQuotas { get; set; }
    public DbSet<Nomination> Nominations { get; set; }
    public DbSet<ManagerScore> ManagerScores { get; set; }
    public DbSet<CommitteeMember> CommitteeMembers { get; set; }
    public DbSet<Administrator> Administrators { get; set; }
    public DbSet<Evaluation> Evaluations { get; set; }
    public DbSet<EvaluationScore> EvaluationScores { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Note: Old HR table relationships removed - now using HR views

        // Configure Employee as existing Oracle view (exclude from migrations)
        builder.Entity<Employee>()
            .ToView("VW_EOM_EMPLOYEES_V")
            .HasKey(e => e.EmployeeId);
            
        // Configure VwEomDepartments as existing Oracle view (exclude from migrations)
        builder.Entity<VwEomDepartments>()
            .ToView("VW_EOM_DEPARTMENTS")
            .HasKey(d => d.DepartmentId);

        // Configure composite keys
        builder.Entity<DepartmentQuota>()
            .HasKey(dq => new { dq.DepartmentId, dq.AwardTypeId });

        builder.Entity<ManagerScore>()
            .HasKey(ms => new { ms.NominationId, ms.SubCriteriaId });

        builder.Entity<EvaluationScore>()
            .HasKey(es => new { es.EvaluationId, es.SubCriteriaId });

        // Configure relationships with Oracle 10g constraint name limits (30 chars max)
        builder.Entity<AwardCycle>()
            .HasOne(ac => ac.AwardType)
            .WithMany(at => at.AwardCycles)
            .HasForeignKey(ac => ac.AwardTypeId)
            .HasConstraintName("FK_AwardCycle_AwardType");

        builder.Entity<Criterion>()
            .HasOne(c => c.AwardType)
            .WithMany(at => at.Criteria)
            .HasForeignKey(c => c.AwardTypeId)
            .HasConstraintName("FK_Criterion_AwardType");

        builder.Entity<SubCriteria>()
            .HasOne(sc => sc.Criterion)
            .WithMany(c => c.SubCriteria)
            .HasForeignKey(sc => sc.CriterionId)
            .HasConstraintName("FK_SubCriteria_Criterion");

        builder.Entity<DepartmentQuota>()
            .HasOne(dq => dq.AwardType)
            .WithMany(at => at.DepartmentQuotas)
            .HasForeignKey(dq => dq.AwardTypeId)
            .HasConstraintName("FK_DeptQuota_AwardType");

        builder.Entity<Nomination>()
            .HasOne(n => n.AwardCycle)
            .WithMany(ac => ac.Nominations)
            .HasForeignKey(n => n.CycleId)
            .HasConstraintName("FK_Nomination_AwardCycle");

        builder.Entity<Nomination>()
            .HasOne(n => n.Employee)
            .WithMany()
            .HasForeignKey(n => n.EmployeeId)
            .HasConstraintName("FK_Nomination_Employee");

        builder.Entity<Nomination>()
            .HasOne(n => n.Manager)
            .WithMany()
            .HasForeignKey(n => n.ManagerId)
            .HasConstraintName("FK_Nomination_Manager");

        builder.Entity<Nomination>()
            .HasOne(n => n.SelectedByCommitteeMember)
            .WithMany()
            .HasForeignKey(n => n.SelectedByCommitteeMemberId)
            .HasConstraintName("FK_Nomination_Committee");

        builder.Entity<ManagerScore>()
            .HasOne(ms => ms.Nomination)
            .WithMany(n => n.ManagerScores)
            .HasForeignKey(ms => ms.NominationId)
            .HasConstraintName("FK_MgrScore_Nomination");

        builder.Entity<ManagerScore>()
            .HasOne(ms => ms.SubCriteria)
            .WithMany(sc => sc.ManagerScores)
            .HasForeignKey(ms => ms.SubCriteriaId)
            .HasConstraintName("FK_MgrScore_SubCriteria");

        builder.Entity<Evaluation>()
            .HasOne(e => e.Nomination)
            .WithMany(n => n.Evaluations)
            .HasForeignKey(e => e.NominationId)
            .HasConstraintName("FK_Evaluation_Nomination");

        builder.Entity<Evaluation>()
            .HasOne(e => e.CommitteeMember)
            .WithMany(cm => cm.Evaluations)
            .HasForeignKey(e => e.CommitteeMemberId)
            .HasConstraintName("FK_Evaluation_Committee");

        builder.Entity<EvaluationScore>()
            .HasOne(es => es.Evaluation)
            .WithMany(e => e.EvaluationScores)
            .HasForeignKey(es => es.EvaluationId)
            .HasConstraintName("FK_EvalScore_Evaluation");

        builder.Entity<EvaluationScore>()
            .HasOne(es => es.SubCriteria)
            .WithMany(sc => sc.EvaluationScores)
            .HasForeignKey(es => es.SubCriteriaId)
            .HasConstraintName("FK_EvalScore_SubCriteria");

        // Configure precision for decimal columns
        builder.Entity<Criterion>()
            .Property(c => c.WeightPercent)
            .HasPrecision(5, 2);

        // Oracle-specific configurations for Oracle 10g compatibility
        // Configure boolean fields for Oracle (use NUMBER(1))
        builder.Entity<AwardType>()
            .Property(at => at.IsActive)
            .HasColumnType("NUMBER(1)");
            
        builder.Entity<CommitteeMember>()
            .Property(cm => cm.IsActive)
            .HasColumnType("NUMBER(1)");
            
        // Configure byte fields for Oracle
        builder.Entity<SubCriteria>()
            .Property(sc => sc.MaxScore)
            .HasColumnType("NUMBER(3)");
            
        // Configure Oracle sequences for Oracle 10g with proper value generation
        builder.Entity<AwardType>()
            .Property(at => at.AwardTypeId)
            .HasColumnType("NUMBER(10)")
            .HasDefaultValueSql("SEQ_AWARDTYPE.NEXTVAL")
            .ValueGeneratedOnAdd();
            
        builder.Entity<AwardCycle>()
            .Property(ac => ac.CycleId)
            .HasColumnType("NUMBER(10)")
            .HasDefaultValueSql("SEQ_AWARDCYCLE.NEXTVAL")
            .ValueGeneratedOnAdd();
            
        builder.Entity<Criterion>()
            .Property(c => c.CriterionId)
            .HasColumnType("NUMBER(10)")
            .HasDefaultValueSql("SEQ_CRITERION.NEXTVAL")
            .ValueGeneratedOnAdd();
            
        builder.Entity<SubCriteria>()
            .Property(sc => sc.SubCriteriaId)
            .HasColumnType("NUMBER(10)")
            .HasDefaultValueSql("SEQ_SUBCRITERIA.NEXTVAL")
            .ValueGeneratedOnAdd();
            
        builder.Entity<Nomination>()
            .Property(n => n.NominationId)
            .HasColumnType("NUMBER(10)")
            .HasDefaultValueSql("SEQ_NOMINATION.NEXTVAL")
            .ValueGeneratedOnAdd();
            
        builder.Entity<Evaluation>()
            .Property(e => e.EvaluationId)
            .HasColumnType("NUMBER(10)")
            .HasDefaultValueSql("SEQ_EVALUATION.NEXTVAL")
            .ValueGeneratedOnAdd();
            
        builder.Entity<CommitteeMember>()
            .Property(cm => cm.Id)
            .HasColumnType("NUMBER(10)")
            .HasDefaultValueSql("SEQ_COMMITTEE.NEXTVAL")
            .ValueGeneratedOnAdd();
            
        // Configure CommitteeMember date fields as Oracle DATE instead of TIMESTAMP
        builder.Entity<CommitteeMember>()
            .Property(cm => cm.StartDate)
            .HasColumnType("DATE");
            
        builder.Entity<CommitteeMember>()
            .Property(cm => cm.EndDate)
            .HasColumnType("DATE");
            
        // Configure Administrator entity for Oracle
        builder.Entity<Administrator>()
            .ToTable("ADMINISTRATORS")
            .Property(a => a.AdministratorId)
            .HasColumnName("ADMINISTRATORID")
            .HasColumnType("NUMBER(10)")
            .HasDefaultValueSql("SEQ_ADMINISTRATOR.NEXTVAL")
            .ValueGeneratedOnAdd();
            
        builder.Entity<Administrator>()
            .Property(a => a.EmployeeId)
            .HasColumnName("EMPLOYEEID")
            .HasColumnType("NUMBER(10)");
            
        builder.Entity<Administrator>()
            .Property(a => a.IsActive)
            .HasColumnName("ISACTIVE")
            .HasColumnType("NUMBER(1)");
            
        // Configure Administrator foreign key relationship
        builder.Entity<Administrator>()
            .HasOne(a => a.Employee)
            .WithMany()
            .HasForeignKey(a => a.EmployeeId)
            .HasConstraintName("FK_Administrator_Employee");
            
        // Ensure Arabic text support with NVARCHAR2
        builder.Entity<AwardType>()
            .Property(at => at.Name)
            .HasColumnType("NVARCHAR2(100)");
            
        builder.Entity<AwardType>()
            .Property(at => at.Description)
            .HasColumnType("NVARCHAR2(500)");
            
        builder.Entity<Criterion>()
            .Property(c => c.Name)
            .HasColumnType("NVARCHAR2(200)");
            
        builder.Entity<SubCriteria>()
            .Property(sc => sc.Name)
            .HasColumnType("NVARCHAR2(200)");
            
        builder.Entity<SubCriteria>()
            .Property(sc => sc.GradingScale)
            .HasColumnType("NCLOB");  // Use NCLOB for longer Arabic text



        builder.Entity<SubCriteria>()
            .Property(sc => sc.SubCriteriaCode)
            .HasColumnType("NVARCHAR2(10)");

        builder.Entity<ManagerScore>()
            .Property(ms => ms.Note)
            .HasColumnType("NVARCHAR2(500)");

        builder.Entity<EvaluationScore>()
            .Property(es => es.Note)
            .HasColumnType("NVARCHAR2(500)");

        builder.Entity<Nomination>()
            .Property(n => n.SupportingDocPath)
            .HasColumnType("NVARCHAR2(500)");
            
        // Configure boolean fields in Nomination for Oracle
        builder.Entity<Nomination>()
            .Property(n => n.IsWinner)
            .HasColumnType("NUMBER(1)");

        builder.Entity<CommitteeMember>()
            .HasOne(cm => cm.Employee)
            .WithMany()
            .HasForeignKey(cm => cm.EmployeeId)
            .HasConstraintName("FK_Committee_Employee");

        // Configure short index names for Oracle 10g (30 character limit)
        builder.Entity<DepartmentQuota>()
            .HasIndex(dq => dq.AwardTypeId)
            .HasDatabaseName("IX_DeptQuota_AwardType");
            
        builder.Entity<AwardCycle>()
            .HasIndex(ac => ac.AwardTypeId)
            .HasDatabaseName("IX_AwardCycle_AwardType");
            
        builder.Entity<Criterion>()
            .HasIndex(c => c.AwardTypeId)
            .HasDatabaseName("IX_Criterion_AwardType");
            
        builder.Entity<Nomination>()
            .HasIndex(n => n.CycleId)
            .HasDatabaseName("IX_Nomination_Cycle");
            
        builder.Entity<Nomination>()
            .HasIndex(n => n.EmployeeId)
            .HasDatabaseName("IX_Nomination_Employee");
            
        builder.Entity<Nomination>()
            .HasIndex(n => n.ManagerId)
            .HasDatabaseName("IX_Nomination_Manager");
            
        builder.Entity<SubCriteria>()
            .HasIndex(sc => sc.CriterionId)
            .HasDatabaseName("IX_SubCriteria_Criterion");
            
        builder.Entity<ManagerScore>()
            .HasIndex(ms => ms.NominationId)
            .HasDatabaseName("IX_MgrScore_Nomination");
            
        builder.Entity<ManagerScore>()
            .HasIndex(ms => ms.SubCriteriaId)
            .HasDatabaseName("IX_MgrScore_SubCriteria");
            
        builder.Entity<Evaluation>()
            .HasIndex(e => e.NominationId)
            .HasDatabaseName("IX_Evaluation_Nomination");
            
        builder.Entity<Evaluation>()
            .HasIndex(e => e.CommitteeMemberId)
            .HasDatabaseName("IX_Evaluation_Committee");
            
        builder.Entity<EvaluationScore>()
            .HasIndex(es => es.EvaluationId)
            .HasDatabaseName("IX_EvalScore_Evaluation");
            
        builder.Entity<EvaluationScore>()
            .HasIndex(es => es.SubCriteriaId)
            .HasDatabaseName("IX_EvalScore_SubCriteria");
            
        builder.Entity<CommitteeMember>()
            .HasIndex(cm => cm.EmployeeId)
            .HasDatabaseName("IX_Committee_Employee");
            
        builder.Entity<Administrator>()
            .HasIndex(a => a.EmployeeId)
            .HasDatabaseName("IX_ADMINISTRATOR_EMPLOYEE");
            
        builder.Entity<Nomination>()
            .HasIndex(n => n.SelectedByCommitteeMemberId)
            .HasDatabaseName("IX_Nomination_SelCommittee");

        // Employee model now maps to VW_EOM_EMPLOYEES view instead of physical table
    }
}