# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Employee of the Month (EOM) System - An ASP.NET Core MVC application for managing monthly employee recognition awards with nomination and evaluation workflows.

## Development Environment

**Technology Stack:**
- ASP.NET Core MVC 9.0 (targeting .NET 9.0)
- Entity Framework Core 9.0.6 with MySQL (Pomelo provider)
- Cookie-based authentication (HR table integration, prepared for AD)
- Bootstrap 5 for responsive UI with Arabic RTL support

**Database Configuration:**
- **Development**: EOM_DIV (Oracle) - Oracle 19c on 10.20.40.71:1521/orcl - Credentials: EOM_DIV/EOM_DIV
- **Staging**: EOM_DIV (Oracle) - Oracle 19c on 10.20.40.71:1521/orcl - Credentials: EOM_DIV/EOM_DIV
- **Production**: EOM (Oracle) - Oracle 19c on 10.20.40.71:1521/orcl - Credentials: EOM/EOM
- **Backup Database**: EOM (MySQL) on 127.0.0.1:8889 (MAMP/XAMPP setup) - Credentials: root/root

**Environment-Specific Connection Strings:**
- `appsettings.Development.json`: Uses EOM_DIV for local development
- `appsettings.Staging.json`: Uses EOM_DIV for staging server (testing with dev data)
- `appsettings.json` & `appsettings.Production.json`: Uses EOM for production deployment

## Essential Commands

```bash
# Build the project
dotnet build

# Run the application (Development - uses EOM_DIV)
dotnet run

# Run in Staging environment (uses EOM_DIV)
dotnet run --environment Staging

# Run in Production environment (uses EOM)
dotnet run --environment Production

# Set environment explicitly
$env:ASPNETCORE_ENVIRONMENT="Development"  # PowerShell
$env:ASPNETCORE_ENVIRONMENT="Staging"      # PowerShell
$env:ASPNETCORE_ENVIRONMENT="Production"   # PowerShell
export ASPNETCORE_ENVIRONMENT=Development  # Bash
export ASPNETCORE_ENVIRONMENT=Staging      # Bash
export ASPNETCORE_ENVIRONMENT=Production   # Bash

# Create database migration
dotnet ef migrations add <MigrationName>

# Update database with migrations
dotnet ef database update

# Publishing Commands
# Publish for Staging (uses EOM_DIV database)
dotnet publish --configuration Release --output "C:\Publish\EOM-Staging"

# Publish for Production (uses EOM database)
dotnet publish --configuration Release --output "C:\Publish\EOM-Production"

# Install EF tools (if needed)
dotnet tool install --global dotnet-ef
export PATH="$PATH:/Users/majid/.dotnet/tools"
```

## Deployment Guide

### **Deployment Process (Simplified):**

Since you have a **web.config in the root folder**, deployment is much simpler:

#### **Method 1: Change web.config Before Publishing**
```bash
# For Staging: Edit web.config and set environment to "Staging"
# Then publish:
dotnet publish --configuration Release --output "C:\Publish\EOM-Staging"

# For Production: Edit web.config and set environment to "Production"  
# Then publish:
dotnet publish --configuration Release --output "C:\Publish\EOM-Production"
```

#### **Method 2: Edit After Publishing**
```bash
# Publish once:
dotnet publish --configuration Release --output "C:\Publish\EOM"

# Then edit the published web.config and change environment value:
# For Staging: <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Staging" />
# For Production: <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
```

### **Environment Mapping:**
- **`value="Development"`** → Uses EOM_DIV database  
- **`value="Staging"`** → Uses EOM_DIV database
- **`value="Production"`** → Uses EOM database

### **Current web.config Structure:**
The web.config includes environment variables and file upload limits:
```xml
<aspNetCore processPath="dotnet" arguments=".\EOM.Web.dll" hostingModel="inprocess">
  <environmentVariables>
    <!-- Change this value: Development, Staging, or Production -->
    <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
  </environmentVariables>
</aspNetCore>
```

## Project Structure

```
/Models/           # Entity models for EOM domain
/Data/             # ApplicationDbContext and database configuration
/Controllers/      # MVC controllers for web endpoints
/Views/            # Razor views and layouts
/Migrations/       # EF Core database migrations
```

## Key Domain Models

- **Employee**: HR employee records with Active Directory integration
- **EmployeeManager**: HR manager-employee hierarchical relationships
- **CommitteeMember**: EOM committee members (references Employee table)
- **AwardType**: Award categories (Employee of Month, Creative Employee, etc.)
- **AwardCycle**: Monthly award cycles with nomination/evaluation periods
- **Criterion**: Main evaluation criteria (4 criteria with weight percentages)
- **SubCriteria**: Detailed sub-criteria with scoring scales (e.g., 1.1, 1.2, 2.1)
- **Nomination**: Manager nominations of employees
- **ManagerScore**: Manager scoring on sub-criteria level
- **Evaluation**: Committee member evaluations of nominees
- **EvaluationScore**: Committee scoring on sub-criteria level

## Database Schema

The system uses a normalized schema with composite keys for scoring tables. Key relationships:
- Employee 1:M EmployeeManager (as Manager)
- Employee 1:M EmployeeManager (as Employee)
- Employee 1:1 CommitteeMember
- AwardType 1:M AwardCycle
- AwardType 1:M Criterion
- Criterion 1:M SubCriteria
- AwardCycle 1:M Nomination
- Nomination 1:M ManagerScore (composite key: NominationId, SubCriteriaId)
- Nomination 1:M Evaluation
- Evaluation 1:M EvaluationScore (composite key: EvaluationId, SubCriteriaId)

**Sub-Criteria Structure:**
- 4 main criteria with weight percentages (25%, 30%, 25%, 20%)
- 16 total sub-criteria with individual scoring and grading scales
- Each sub-criterion has SubCriteriaCode (e.g., "1.1", "2.3"), MaxScore, and JSON GradingScale

## Authentication & Authorization

Uses HR-based cookie authentication with role-based authorization:
- **EOM-Admin**: Admin role (EmployeeId = 1) - System configuration and cycle management
- **Manager**: HR managers - Employee nomination and scoring
- **EOM-Committee**: Committee members - Employee evaluation and scoring
- Authentication via Employee email lookup (prepared for Active Directory integration)
- No password storage (temporary development authentication)

## Development Notes

- **Language**: Arabic interface with RTL layout support
- **Authentication**: HR-based authentication (removed ASP.NET Identity)
- **Evaluation Structure**: Redesigned for sub-criteria level scoring (16 sub-criteria total)
- **Active Directory**: Prepared for AD integration (ActiveDirectoryId field in Employee model)
- **Database**: MySQL connection configured for local development environment
- **Migrations**: Latest migration "UpdateForSubCriteria" includes sub-criteria schema
- **Test Data**: Arabic seed data with complete sub-criteria definitions from form.md

## Build Performance Issues & Solutions

### MSBuild Hang Issue (Fixed)
The project was experiencing MSBuild hangs where builds would stop after restore with no error messages. This was caused by shared compilation deadlock in .NET 9.

**Solution Applied:**
```xml
<PropertyGroup>
  <UseSharedCompilation>false</UseSharedCompilation>
</PropertyGroup>
```

**Diagnostic Steps for Similar Issues:**
1. **Capture binlog**: `dotnet build --no-restore -bl:hang.binlog`
2. **Test without analyzers**: `dotnet build --no-restore -p:RunAnalyzers=false`
3. **Test Razor tweaks**: `dotnet build --no-restore -p:RazorCompileOnBuild=false`
4. **Test MSBuild fixes**:
   - Single-proc: `dotnet build --no-restore -m:1`
   - No shared compilation: `dotnet build --no-restore /p:UseSharedCompilation=false`
   - Disable node reuse: `MSBUILDDISABLENODEREUSE=1 dotnet build --no-restore`

**Performance Result**: Build time reduced from indefinite hang to ~3 seconds.

## Recent Architecture Changes (2024)

1. **Removed ASP.NET Identity**: Replaced with HR table-based authentication using Employee and EmployeeManager tables
2. **Sub-Criteria Implementation**: Updated database schema to support detailed sub-criteria evaluation as per form.md requirements
3. **Arabic Localization**: Full Arabic interface with proper RTL support
4. **Controller Updates**: Fixed NominationsController and EvaluationsController to work with new SubCriteria model
5. **Build Performance**: Fixed MSBuild shared compilation deadlock issue