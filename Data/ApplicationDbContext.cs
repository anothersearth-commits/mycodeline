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
    
    // Managers view for HR integration
    public DbSet<VwEomManagers> Managers { get; set; }
    
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
    public DbSet<GroupNominationMember> GroupNominationMembers { get; set; }
    
    // AI Objectives & Messaging Tables
    public DbSet<ObjectiveCycle> ObjectiveCycles { get; set; }
    public DbSet<Objective> Objectives { get; set; }
    public DbSet<AiGeneratedMessage> AiGeneratedMessages { get; set; }
    
    // Ejadah Evaluation Tables
    public DbSet<EjadahCycle> EjadahCycles { get; set; }
    public DbSet<EjadahEmployeeScore> EjadahEmployeeScores { get; set; }
    
    // Attendance View
    public DbSet<AttendanceRecord> AttendanceRecords { get; set; }

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
            
        // Configure VwEomManagers as existing Oracle view (exclude from migrations)
        builder.Entity<VwEomManagers>()
            .ToView("VW_EOM_MANAGERS")
            .HasKey(m => m.ManagerId);
            
        // Configure AttendanceRecord as existing Oracle view (exclude from migrations)
        builder.Entity<AttendanceRecord>()
            .ToView("VW_EOM_ATTENDANCE")
            .HasKey(a => new { a.EmployeeNumber, a.AttendanceDate });
            
        // Configure AttendanceRecord column types for Oracle
        builder.Entity<AttendanceRecord>()
            .Property(a => a.EmployeeNumber)
            .HasColumnType("NUMBER(10)");
            
        builder.Entity<AttendanceRecord>()
            .Property(a => a.AttendanceDate)
            .HasColumnType("DATE");
            
        builder.Entity<AttendanceRecord>()
            .Property(a => a.AttendanceIn)
            .HasColumnType("VARCHAR2(8)");
            
        builder.Entity<AttendanceRecord>()
            .Property(a => a.AttendanceOut)
            .HasColumnType("VARCHAR2(8)");
            
        builder.Entity<AttendanceRecord>()
            .Property(a => a.Difference)
            .HasColumnType("VARCHAR2(8)");
            
        // Configure Employee to Department relationship
        builder.Entity<Employee>()
            .HasOne(e => e.Department)
            .WithMany()
            .HasForeignKey(e => e.DepartmentId)
            .HasPrincipalKey(d => d.DepartmentId);

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

        builder.Entity<AwardType>()
            .Property(at => at.IsSelfNomination)
            .HasColumnType("NUMBER(1)");
        
        builder.Entity<AwardType>()
            .Property(at => at.UsesDirectorateCommittees)
            .HasColumnType("NUMBER(1)");
            
        builder.Entity<CommitteeMember>()
            .Property(cm => cm.IsActive)
            .HasColumnType("NUMBER(1)");
        
        builder.Entity<CommitteeMember>()
            .Property(cm => cm.AwardTypeId)
            .HasColumnType("NUMBER(10)");
        
        builder.Entity<CommitteeMember>()
            .Property(cm => cm.Directorate)
            .HasColumnType("NUMBER(10)");
            
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
            
        // Configure IsWinner field in Nomination for Oracle (0=not winner, 1=final winner, 2=preliminary winner)
        builder.Entity<Nomination>()
            .Property(n => n.IsWinner)
            .HasColumnType("NUMBER(10)");
            
        // Configure self-nomination fields
        builder.Entity<Nomination>()
            .Property(n => n.IsSelfNomination)
            .HasColumnType("NUMBER(1)");
            
        builder.Entity<Nomination>()
            .Property(n => n.InitiativeDetails)
            .HasColumnType("NCLOB");
            
        builder.Entity<Nomination>()
            .Property(n => n.AttachmentPath)
            .HasColumnType("NVARCHAR2(500)");

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
            
        // Add unique constraint to prevent duplicate evaluations
        builder.Entity<Evaluation>()
            .HasIndex(e => new { e.NominationId, e.CommitteeMemberId })
            .IsUnique()
            .HasDatabaseName("UQ_Eval_Nom_Committee");
            
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
            
        // Configure GroupNominationMember
        builder.Entity<GroupNominationMember>()
            .ToTable("GROUPNOMINATIONMEMBERS")
            .Property(gnm => gnm.GroupMemberId)
            .HasColumnName("GROUPMEMBERID")
            .HasColumnType("NUMBER(10)")
            .HasDefaultValueSql("SEQ_GROUPNOMINATION.NEXTVAL")
            .ValueGeneratedOnAdd();
            
        builder.Entity<GroupNominationMember>()
            .Property(gnm => gnm.NominationId)
            .HasColumnName("NOMINATIONID")
            .HasColumnType("NUMBER(10)");
            
        builder.Entity<GroupNominationMember>()
            .Property(gnm => gnm.EmployeeId)
            .HasColumnName("EMPLOYEEID")
            .HasColumnType("NUMBER(10)");
            
        builder.Entity<GroupNominationMember>()
            .HasOne(gnm => gnm.Nomination)
            .WithMany(n => n.GroupMembers)
            .HasForeignKey(gnm => gnm.NominationId)
            .HasConstraintName("FK_GroupNom_Nomination");
            
        builder.Entity<GroupNominationMember>()
            .HasOne(gnm => gnm.Employee)
            .WithMany()
            .HasForeignKey(gnm => gnm.EmployeeId)
            .HasConstraintName("FK_GroupNom_Employee");
            
        builder.Entity<GroupNominationMember>()
            .HasIndex(gnm => new { gnm.NominationId, gnm.EmployeeId })
            .IsUnique()
            .HasDatabaseName("UQ_GroupNom_Nom_Emp");

        // Configure AI Objectives & Messaging entities
        ConfigureAiObjectivesEntities(builder);

        // Configure Ejadah Evaluation entities
        ConfigureEjadahEntities(builder);

        // Employee model now maps to VW_EOM_EMPLOYEES view instead of physical table
    }

    private void ConfigureAiObjectivesEntities(ModelBuilder builder)
    {
        // ObjectiveCycles configuration
        builder.Entity<ObjectiveCycle>()
            .ToTable("OBJECTIVECYCLES")
            .HasKey(oc => oc.ObjectiveCycleId);

        builder.Entity<ObjectiveCycle>()
            .Property(oc => oc.ObjectiveCycleId)
            .HasColumnName("OBJECTIVECYCLEID")
            .HasColumnType("NUMBER(10)")
            .HasDefaultValueSql("SEQ_OBJECTIVECYCLE.NEXTVAL")
            .ValueGeneratedOnAdd();

        builder.Entity<ObjectiveCycle>()
            .Property(oc => oc.Year)
            .HasColumnName("YEAR")
            .HasColumnType("NUMBER(10)");

        builder.Entity<ObjectiveCycle>()
            .Property(oc => oc.Half)
            .HasColumnName("HALF")
            .HasColumnType("NUMBER(10)");

        builder.Entity<ObjectiveCycle>()
            .Property(oc => oc.IsActive)
            .HasColumnName("ISACTIVE")
            .HasColumnType("NUMBER(10)");

        builder.Entity<ObjectiveCycle>()
            .Property(oc => oc.StartDate)
            .HasColumnName("STARTDATE")
            .HasColumnType("DATE");

        builder.Entity<ObjectiveCycle>()
            .Property(oc => oc.EndDate)
            .HasColumnName("ENDDATE")
            .HasColumnType("DATE");

        builder.Entity<ObjectiveCycle>()
            .Property(oc => oc.CreatedAt)
            .HasColumnName("CREATEDAT")
            .HasColumnType("TIMESTAMP");

        builder.Entity<ObjectiveCycle>()
            .Property(oc => oc.UpdatedAt)
            .HasColumnName("UPDATEDAT")
            .HasColumnType("TIMESTAMP");

        // Unique constraint on Year and Half
        builder.Entity<ObjectiveCycle>()
            .HasIndex(oc => new { oc.Year, oc.Half })
            .IsUnique()
            .HasDatabaseName("UQ_OBJECTIVECYCLE_YEAR_HALF");

        // Objectives configuration
        builder.Entity<Objective>()
            .ToTable("OBJECTIVES")
            .HasKey(o => o.ObjectiveId);

        builder.Entity<Objective>()
            .Property(o => o.ObjectiveId)
            .HasColumnName("OBJECTIVEID")
            .HasColumnType("NUMBER(19)")
            .HasDefaultValueSql("SEQ_OBJECTIVE.NEXTVAL")
            .ValueGeneratedOnAdd();

        builder.Entity<Objective>()
            .Property(o => o.ObjectiveCycleId)
            .HasColumnName("OBJECTIVECYCLEID")
            .HasColumnType("NUMBER(10)");

        builder.Entity<Objective>()
            .Property(o => o.EmployeeId)
            .HasColumnName("EMPLOYEEID")
            .HasColumnType("NUMBER(10)");

        builder.Entity<Objective>()
            .Property(o => o.MainGoalId)
            .HasColumnName("MAIN_GOAL_ID")
            .HasColumnType("NUMBER(10)");

        builder.Entity<Objective>()
            .Property(o => o.ObjectiveTitle)
            .HasColumnName("OBJECTIVETITLE")
            .HasColumnType("NVARCHAR2(500)");

        builder.Entity<Objective>()
            .Property(o => o.Classification)
            .HasColumnName("CLASSIFICATION")
            .HasColumnType("NVARCHAR2(200)");

        builder.Entity<Objective>()
            .Property(o => o.ResultDescription)
            .HasColumnName("RESULTDESCRIPTION")
            .HasColumnType("NCLOB");

        builder.Entity<Objective>()
            .Property(o => o.WeightScore)
            .HasColumnName("WEIGHTSCORE")
            .HasColumnType("NUMBER(8,2)");

        builder.Entity<Objective>()
            .Property(o => o.ThresholdExceeds)
            .HasColumnName("THRESHOLDEXCEEDS")
            .HasColumnType("NUMBER(8,2)");

        builder.Entity<Objective>()
            .Property(o => o.ThresholdMeets)
            .HasColumnName("THRESHOLDMEETS")
            .HasColumnType("NUMBER(8,2)");

        builder.Entity<Objective>()
            .Property(o => o.ThresholdBelow)
            .HasColumnName("THRESHOLDBELOW")
            .HasColumnType("NUMBER(8,2)");

        builder.Entity<Objective>()
            .Property(o => o.ActualScore)
            .HasColumnName("ACTUALSCORE")
            .HasColumnType("NUMBER(8,2)");

        builder.Entity<Objective>()
            .Property(o => o.HighLevelGoal)
            .HasColumnName("HIGHLEVELGOAL")
            .HasColumnType("NVARCHAR2(500)");

        builder.Entity<Objective>()
            .Property(o => o.Category)
            .HasColumnName("CATEGORY")
            .HasColumnType("NVARCHAR2(100)");

        builder.Entity<Objective>()
            .Property(o => o.CreatedAt)
            .HasColumnName("CREATEDAT")
            .HasColumnType("TIMESTAMP");

        builder.Entity<Objective>()
            .Property(o => o.UpdatedAt)
            .HasColumnName("UPDATEDAT")
            .HasColumnType("TIMESTAMP");

        // Objective relationships
        builder.Entity<Objective>()
            .HasOne(o => o.ObjectiveCycle)
            .WithMany(oc => oc.Objectives)
            .HasForeignKey(o => o.ObjectiveCycleId)
            .HasConstraintName("FK_OBJECTIVE_CYCLE");

        // Objective indexes
        builder.Entity<Objective>()
            .HasIndex(o => new { o.EmployeeId, o.ObjectiveCycleId })
            .HasDatabaseName("IX_OBJECTIVE_EMP_CYCLE");

        builder.Entity<Objective>()
            .HasIndex(o => o.ObjectiveCycleId)
            .HasDatabaseName("IX_OBJECTIVE_CYCLE");

        builder.Entity<Objective>()
            .HasIndex(o => o.MainGoalId)
            .HasDatabaseName("IX_OBJECTIVE_MAIN_GOAL");

        // AiGeneratedMessages configuration
        builder.Entity<AiGeneratedMessage>()
            .ToTable("AIGENERATEDMESSAGES")
            .HasKey(am => am.AiMessageId);

        builder.Entity<AiGeneratedMessage>()
            .Property(am => am.AiMessageId)
            .HasColumnName("AIMESSAGEID")
            .HasColumnType("NUMBER(19)")
            .HasDefaultValueSql("SEQ_AIMESSAGE.NEXTVAL")
            .ValueGeneratedOnAdd();

        builder.Entity<AiGeneratedMessage>()
            .Property(am => am.ObjectiveId)
            .HasColumnName("OBJECTIVEID")
            .HasColumnType("NUMBER(19)");

        builder.Entity<AiGeneratedMessage>()
            .Property(am => am.EmployeeId)
            .HasColumnName("EMPLOYEEID")
            .HasColumnType("NUMBER(10)");

        builder.Entity<AiGeneratedMessage>()
            .Property(am => am.ObjectiveCycleId)
            .HasColumnName("OBJECTIVECYCLEID")
            .HasColumnType("NUMBER(10)");

        builder.Entity<AiGeneratedMessage>()
            .Property(am => am.MessageBody)
            .HasColumnName("MESSAGEBODY")
            .HasColumnType("NCLOB");

        builder.Entity<AiGeneratedMessage>()
            .Property(am => am.AdviceBody)
            .HasColumnName("ADVICEBODY")
            .HasColumnType("NCLOB");

        builder.Entity<AiGeneratedMessage>()
            .Property(am => am.StyleTag)
            .HasColumnName("STYLETAG")
            .HasColumnType("NVARCHAR2(50)");

        builder.Entity<AiGeneratedMessage>()
            .Property(am => am.ModelName)
            .HasColumnName("MODELNAME")
            .HasColumnType("NVARCHAR2(50)");

        builder.Entity<AiGeneratedMessage>()
            .Property(am => am.GeneratedAt)
            .HasColumnName("GENERATEDAT")
            .HasColumnType("TIMESTAMP");

        builder.Entity<AiGeneratedMessage>()
            .Property(am => am.IsActive)
            .HasColumnName("ISACTIVE")
            .HasColumnType("NUMBER(1)");

        // AiGeneratedMessage relationships
        builder.Entity<AiGeneratedMessage>()
            .HasOne(am => am.Objective)
            .WithMany(o => o.AiGeneratedMessages)
            .HasForeignKey(am => am.ObjectiveId)
            .HasConstraintName("FK_AIMESSAGE_OBJECTIVE")
            .OnDelete(DeleteBehavior.Cascade);

        // AiGeneratedMessage indexes
        builder.Entity<AiGeneratedMessage>()
            .HasIndex(am => new { am.EmployeeId, am.ObjectiveCycleId, am.IsActive })
            .HasDatabaseName("IX_AIMSG_EMP_CYCLE_ACTIVE");

        builder.Entity<AiGeneratedMessage>()
            .HasIndex(am => new { am.ObjectiveId, am.IsActive })
            .HasDatabaseName("IX_AIMSG_OBJ_ACTIVE");
    }

    private void ConfigureEjadahEntities(ModelBuilder builder)
    {
        // EjadahCycles configuration
        builder.Entity<EjadahCycle>()
            .ToTable("EJADAH_CYCLES")
            .HasKey(ec => ec.EjadahCycleId);

        builder.Entity<EjadahCycle>()
            .Property(ec => ec.EjadahCycleId)
            .HasColumnName("EJADAH_CYCLE_ID")
            .HasColumnType("NUMBER(10)")
            .HasDefaultValueSql("SEQ_EJADAH_CYCLES.NEXTVAL")
            .ValueGeneratedOnAdd();

        builder.Entity<EjadahCycle>()
            .Property(ec => ec.Year)
            .HasColumnName("YEAR")
            .HasColumnType("NUMBER");

        builder.Entity<EjadahCycle>()
            .Property(ec => ec.Half)
            .HasColumnName("HALF")
            .HasColumnType("NUMBER");

        builder.Entity<EjadahCycle>()
            .Property(ec => ec.StartDate)
            .HasColumnName("START_DATE")
            .HasColumnType("DATE");

        builder.Entity<EjadahCycle>()
            .Property(ec => ec.EndDate)
            .HasColumnName("END_DATE")
            .HasColumnType("DATE");

        builder.Entity<EjadahCycle>()
            .Property(ec => ec.IsActive)
            .HasColumnName("IS_ACTIVE")
            .HasColumnType("NUMBER");

        builder.Entity<EjadahCycle>()
            .Property(ec => ec.CreatedDate)
            .HasColumnName("CREATED_DATE")
            .HasColumnType("DATE")
            .HasDefaultValueSql("SYSDATE");

        builder.Entity<EjadahCycle>()
            .Property(ec => ec.CreatedBy)
            .HasColumnName("CREATED_BY")
            .HasColumnType("NVARCHAR2(100)");

        // Unique constraint on Year and Half for EjadahCycles
        builder.Entity<EjadahCycle>()
            .HasIndex(ec => new { ec.Year, ec.Half })
            .IsUnique()
            .HasDatabaseName("UK_EJADAH_CYCLES_YEAR_HALF");

        // EjadahEmployeeScores configuration
        builder.Entity<EjadahEmployeeScore>()
            .ToTable("EJADAH_EMPLOYEE_SCORES")
            .HasKey(ees => ees.EjadahEmployeeScoreId);

        builder.Entity<EjadahEmployeeScore>()
            .Property(ees => ees.EjadahEmployeeScoreId)
            .HasColumnName("EJADAH_EMPLOYEE_SCORE_ID")
            .HasColumnType("NUMBER(10)")
            .HasDefaultValueSql("SEQ_EJADAH_EMPLOYEE_SCORES.NEXTVAL")
            .ValueGeneratedOnAdd();

        builder.Entity<EjadahEmployeeScore>()
            .Property(ees => ees.EjadahCycleId)
            .HasColumnName("EJADAH_CYCLE_ID")
            .HasColumnType("NUMBER(10)");

        builder.Entity<EjadahEmployeeScore>()
            .Property(ees => ees.EmployeeId)
            .HasColumnName("EMPLOYEE_ID")
            .HasColumnType("NUMBER(10)");

        builder.Entity<EjadahEmployeeScore>()
            .Property(ees => ees.Score)
            .HasColumnName("SCORE")
            .HasColumnType("VARCHAR2(50)");

        builder.Entity<EjadahEmployeeScore>()
            .Property(ees => ees.ScoreNumeric)
            .HasColumnName("SCORE_NUMERIC")
            .HasColumnType("NUMBER(22,0)");


        // EjadahEmployeeScore relationships
        builder.Entity<EjadahEmployeeScore>()
            .HasOne(ees => ees.EjadahCycle)
            .WithMany(ec => ec.EjadahEmployeeScores)
            .HasForeignKey(ees => ees.EjadahCycleId)
            .HasConstraintName("FK_EJADAH_SCORES_CYCLE");

        // Note: Relationship removed due to type mismatch (decimal vs int)
        // EjadahEmployeeScore.EmployeeId (decimal) vs Employee.EmployeeId (int)


        // Unique constraint on cycle and employee
        builder.Entity<EjadahEmployeeScore>()
            .HasIndex(ees => new { ees.EjadahCycleId, ees.EmployeeId })
            .IsUnique()
            .HasDatabaseName("UK_EJADAH_SCORES_CYCLE_EMP");

        // Performance indexes for EjadahEmployeeScores
        builder.Entity<EjadahEmployeeScore>()
            .HasIndex(ees => ees.EmployeeId)
            .HasDatabaseName("IDX_EJADAH_SCORES_EMPLOYEE");

        builder.Entity<EjadahEmployeeScore>()
            .HasIndex(ees => ees.EjadahCycleId)
            .HasDatabaseName("IDX_EJADAH_SCORES_CYCLE");

        builder.Entity<EjadahEmployeeScore>()
            .HasIndex(ees => ees.Score)
            .HasDatabaseName("IDX_EJADAH_SCORES_SCORE");


        // Performance indexes for EjadahCycles
        builder.Entity<EjadahCycle>()
            .HasIndex(ec => new { ec.Year, ec.Half })
            .HasDatabaseName("IDX_EJADAH_CYCLES_YEAR_HALF");

        builder.Entity<EjadahCycle>()
            .HasIndex(ec => ec.IsActive)
            .HasDatabaseName("IDX_EJADAH_CYCLES_ACTIVE");
    }
}