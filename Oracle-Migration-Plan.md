# Oracle Database Migration Plan
## EOM (Employee of the Month) System - MySQL to Oracle Migration

### Executive Summary

This document outlines the comprehensive migration plan for moving the EOM system from MySQL to Oracle database while maintaining all existing functionality and Arabic language support.

**Migration Complexity**: Medium  
**Estimated Effort**: 3-5 days  
**Risk Level**: Low (HR views already exist in Oracle EOM schema)

---

## Current State Analysis

### Technology Stack
- **Framework**: ASP.NET Core MVC 9.0 (.NET 9.0)
- **ORM**: Entity Framework Core 9.0.6
- **Current Provider**: Pomelo MySQL 9.0.0-preview.3
- **Database**: MySQL 8889 (MAMP/XAMPP local)
- **Connection**: `Server=127.0.0.1;Port=8889;Database=EOM;Uid=root;Pwd=root;`

### Database Structure
The EOM system contains 10 main tables with complex relationships:
- **Core Tables**: AwardTypes, AwardCycles, Criteria, SubCriteria, DepartmentQuotas
- **Workflow Tables**: Nominations, ManagerScores, Evaluations, EvaluationScores, CommitteeMembers
- **HR Integration**: Will connect to existing Oracle HR views in EOM schema (VW_EOM_EMPLOYEES, VW_EOM_DEPARTMENTS, VW_EOM_MANAGERS)

---

## Migration Strategy

### Phase 1: Environment Preparation

#### 1.1 Oracle Database Access
- **Host**: 10.20.3.3:1521
- **Service**: hrdb (Oracle 10g)
- **Schema**: EOM user/schema
- **HR Views**: Use existing VW_EOM_EMPLOYEES, VW_EOM_DEPARTMENTS, VW_EOM_MANAGERS views
- **Permissions**: EOM user has CREATE TABLE, CREATE SEQUENCE privileges for EOM tables

#### 1.2 .NET Package Updates
```xml
<!-- Remove from EOM.Web.csproj -->
<PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="9.0.0-preview.3.efcore.9.0.0" />

<!-- Add to EOM.Web.csproj -->
<PackageReference Include="Oracle.EntityFrameworkCore" Version="9.0.6" />
```

#### 1.3 Connection String Configuration
```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=10.20.3.3:1521/hrdb;User Id=EOM;Password=EOM;"
  }
}
```

### Phase 2: Code Modifications

#### 2.1 Program.cs Updates
```csharp
// Change from:
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// To:
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseOracle(connectionString));
```

#### 2.2 ApplicationDbContext Modifications
```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    // Remove MySQL-specific configurations
    // Add Oracle-specific configurations
    
    // Boolean fields configuration for Oracle
    builder.Entity<AwardType>()
        .Property(at => at.IsActive)
        .HasConversion<int>();
        
    builder.Entity<CommitteeMember>()
        .Property(cm => cm.IsActive)
        .HasConversion<int>();
        
    // Byte fields for Oracle
    builder.Entity<SubCriteria>()
        .Property(sc => sc.MaxScore)
        .HasColumnType("NUMBER(3)");
        
    // Ensure Arabic text support
    builder.Entity<AwardType>()
        .Property(at => at.Name)
        .HasColumnType("NVARCHAR2(100)");
        
    // Continue with existing configurations...
}
```

### Phase 3: Data Type Mapping

#### 3.1 Critical Data Type Conversions

| MySQL Type | Oracle Type | EF Core Action Required |
|------------|-------------|-------------------------|
| `datetime(6)` | `TIMESTAMP(6)` | ✅ Automatic |
| `tinyint(1)` (bool) | `NUMBER(1)` | ⚠️ Manual conversion needed |
| `tinyint unsigned` (byte) | `NUMBER(3)` | ⚠️ Manual mapping required |
| `varchar(N) utf8mb4` | `NVARCHAR2(N)` | ✅ Automatic (Arabic support) |
| `int AUTO_INCREMENT` | `NUMBER IDENTITY` | ✅ Automatic |
| `decimal(5,2)` | `NUMBER(5,2)` | ✅ Automatic |

#### 3.2 Special Handling Required

**Boolean Fields:**
- AwardType.IsActive
- CommitteeMember.IsActive
- Employee.IsActive (HR view)

**Byte Fields:**
- SubCriteria.MaxScore

**Arabic Text Fields:**
- All Name, Description, and text fields requiring Arabic support

### Phase 4: Migration Execution

#### 4.1 Data Backup
```bash
# Export MySQL data
mysqldump -u root -p --databases EOM > eom_backup.sql

# Export CSV for Oracle import
mysql -u root -p -e "SELECT * FROM EOM.AwardTypes" > awardtypes.csv
# Repeat for all tables
```

#### 4.2 Schema Recreation
```bash
# Remove existing migrations
rm -rf Migrations/

# Create new Oracle migration
dotnet ef migrations add InitialOracle

# Review and modify migration if needed
# Apply to Oracle database
dotnet ef database update
```

#### 4.3 Data Import Strategy
```sql
-- Oracle SQL*Loader or INSERT statements
-- Example for AwardTypes:
INSERT INTO AwardTypes (Id, Name, Description, IsActive, CreatedDate)
VALUES (1, N'موظف الشهر', N'جائزة موظف الشهر', 1, TIMESTAMP '2024-01-01 00:00:00');
```

### Phase 5: Testing & Validation

#### 5.1 Functional Testing
- [ ] CRUD operations on all entities
- [ ] Nomination workflow (Manager → Committee)
- [ ] Scoring calculations and rankings
- [ ] Arabic text storage and display
- [ ] Date filtering and operations
- [ ] File upload functionality

#### 5.2 Integration Testing
- [ ] HR views connectivity (existing VW_EOM_EMPLOYEES, VW_EOM_DEPARTMENTS, VW_EOM_MANAGERS in EOM schema)
- [ ] Authentication via HR employee lookup
- [ ] Role-based authorization (Admin, Manager, Committee)

#### 5.3 Performance Testing
- [ ] Query performance comparison
- [ ] Complex scoring calculations
- [ ] Report generation speed

---

## Risk Mitigation

### High Priority Risks

#### 5.1 Boolean Field Conversion
**Risk**: MySQL `tinyint(1)` vs Oracle `NUMBER(1)` incompatibility  
**Mitigation**: Explicit HasConversion<int>() configuration in EF Core

#### 5.2 Arabic Text Encoding
**Risk**: Character encoding issues with Arabic text  
**Mitigation**: Use NVARCHAR2 and test thoroughly with Arabic data

#### 5.3 Identity Column Behavior
**Risk**: Auto-increment differences between MySQL and Oracle  
**Mitigation**: Test entity creation and verify ID generation

### Medium Priority Risks

#### 5.4 Migration History
**Risk**: `__EFMigrationsHistory` table compatibility  
**Mitigation**: Fresh migration approach, manual history if needed

#### 5.5 Decimal Precision
**Risk**: Precision handling differences  
**Mitigation**: Explicit HasPrecision() configuration testing

#### 5.6 Date/Time Operations
**Risk**: DateTime comparison and filtering differences  
**Mitigation**: Comprehensive date operation testing

---

## Rollback Plan

### Emergency Rollback Procedure
1. **Immediate**: Revert connection string to MySQL
2. **Code**: Git revert to last known good commit
3. **Database**: Restore from MySQL backup
4. **Verify**: Run full test suite

### Rollback Triggers
- Critical functionality failures
- Data integrity issues
- Performance degradation > 50%
- Arabic text display problems

---

## Post-Migration Tasks

### 6.1 Performance Optimization
- [ ] Index analysis and optimization
- [ ] Query execution plan review
- [ ] Connection pooling configuration

### 6.2 Monitoring Setup
- [ ] Database performance monitoring
- [ ] Error logging and alerting
- [ ] Connection health checks

### 6.3 Documentation Updates
- [ ] Update CLAUDE.md with Oracle configuration
- [ ] Update deployment documentation
- [ ] Update developer setup guides

---

## Timeline & Resources

### Estimated Timeline
- **Day 1**: Environment setup and package updates
- **Day 2**: Code modifications and configuration
- **Day 3**: Data migration and schema recreation
- **Day 4**: Testing and validation
- **Day 5**: Performance optimization and documentation

### Required Resources
- ✅ Oracle database server (10.20.3.3:1521/hrdb, Oracle 10g)
- Development environment access
- Backup storage for MySQL data
- Testing environment for validation

### Success Criteria
- [ ] All existing functionality works correctly
- [ ] Arabic text displays properly
- [ ] HR integration remains functional
- [ ] Performance meets or exceeds current levels
- [ ] No data loss during migration
- [ ] Full test suite passes

---

## Next Steps

**Oracle Environment Confirmed**: 
- ✅ Host: 10.20.3.3:1521/hrdb (Oracle 10g)
- ✅ Credentials: EOM/EOM
- ✅ HR Views: Already exist in EOM schema

**Ready to Proceed**: All Oracle details confirmed. Migration can begin immediately following this plan.

**Next Step**: Execute Phase 1 (Environment Preparation) to start the migration process.