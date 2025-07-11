# HR Integration Views Structure

This document contains all the views created in DIGIHR_TEST database to integrate with the EOM (Employee of the Month) system.

## Overview

The EOM system integrates with the HR system through database views that map HR data structures to EOM requirements. These views replace the original EOM tables (Employees, Departments, EmployeeManagers).

## Views Created

### 1. VW_EOM_EMPLOYEES
**Purpose**: Replace the EOM `Employees` table with HR employee data

**SQL** (✅ **CREATED IN DATABASE**):
```sql
CREATE OR REPLACE FORCE VIEW DIGIHR_TEST.VW_EOM_EMPLOYEES
(
   EMPLOYEEID,
   FIRSTNAME,
   LASTNAME,
   EMAIL,
   DEPARTMENTID,
   JOBTITLE,
   HIREDATE,
   ACTIVEDIRECTORYID,
   PASSWORD,
   ISACTIVE,
   PHONENUMBER,
   MANAGERID,
   MANAGERNAME
)
AS
SELECT 
   e.APP_USER_ID AS EMPLOYEEID,
   COALESCE(e.EMP_NAME_AR, e.EMP_NAME) AS FIRSTNAME,
   '' AS LASTNAME,
   e.EMAIL,
   e.LOC4 AS DEPARTMENTID,                -- Section as department
   COALESCE(e.DESIGNATION_DESC, e.DESIGNATION) AS JOBTITLE,
   e.DATE_OF_JOIN AS HIREDATE,
   e.APP_USER_ID AS ACTIVEDIRECTORYID,
   '' AS PASSWORD,
   1 AS ISACTIVE,
   COALESCE(e.GSM, e.PHONE) AS PHONENUMBER,
   m.APP_USER_ID AS MANAGERID,
   COALESCE(m.EMP_NAME_AR, m.EMP_NAME) AS MANAGERNAME
FROM AD_EMPLOYEE e
LEFT JOIN AD_EMPLOYEE m ON e.LOC3 = m.LOC3 AND m.IS_DEP_HEAD = 1 AND e.EMP_ID != m.EMP_ID;
```

**Field Mappings**:
- `EMPLOYEEID` → `APP_USER_ID` (Login ID)
- `FIRSTNAME` → `EMP_NAME_AR` (Arabic name preferred)
- `LASTNAME` → Empty (using full name in firstname)
- `EMAIL` → `EMAIL`
- `DEPARTMENTID` → `LOC4` (Section ID)
- `JOBTITLE` → `DESIGNATION_DESC` or `DESIGNATION`
- `HIREDATE` → `DATE_OF_JOIN`
- `ACTIVEDIRECTORYID` → `APP_USER_ID`
- `PASSWORD` → Empty (will use AD authentication)
- `ISACTIVE` → Always 1 (show all employees)
- `PHONENUMBER` → `GSM` or `PHONE`
- `MANAGERID` → Manager's `APP_USER_ID`
- `MANAGERNAME` → Manager's name

### 2. VW_EOM_DEPARTMENTS
**Purpose**: Replace the EOM `Departments` table with HR location data

**SQL**:
```sql
CREATE OR REPLACE FORCE VIEW DIGIHR_TEST.VW_EOM_DEPARTMENTS
(
   DEPARTMENTID,
   NAME,
   DESCRIPTION,
   ISACTIVE
)
AS
SELECT 
   dept.LOC_ID AS DEPARTMENTID,
   dept.LOC_NAME_AR AS NAME,
   NVL(grandparent.LOC_NAME_AR, '') || 
   CASE WHEN grandparent.LOC_NAME_AR IS NOT NULL THEN ' - ' ELSE '' END ||
   NVL(parent.LOC_NAME_AR, '') ||
   CASE WHEN parent.LOC_NAME_AR IS NOT NULL THEN ' - ' ELSE '' END ||
   dept.LOC_NAME_AR AS DESCRIPTION,
   1 AS ISACTIVE
FROM AD_LOCATION dept
LEFT JOIN AD_LOCATION parent ON dept.PARENT_ID = parent.LOC_ID
LEFT JOIN AD_LOCATION grandparent ON parent.PARENT_ID = grandparent.LOC_ID
WHERE dept.TYPE_ID = 3;
```

**Field Mappings**:
- `DEPARTMENTID` → `LOC_ID` (Department ID)
- `NAME` → `LOC_NAME_AR` (Arabic department name)
- `DESCRIPTION` → Full hierarchy path (e.g., "المديرية العامة للشؤون الإدارية والمالية - دائرة تقنية المعلومات")
- `ISACTIVE` → Always 1

### 3. VW_EOM_MANAGERS
**Purpose**: Show only managers (department heads) for management purposes

**SQL** (✅ **CREATED IN DATABASE**):
```sql
CREATE OR REPLACE FORCE VIEW DIGIHR_TEST.VW_EOM_MANAGERS
(
   MANAGERID,
   MANAGERNAME,
   MANAGERNAME_AR,
   EMAIL,
   DEPARTMENTID,
   DEPARTMENTNAME,
   JOBTITLE,
   PHONE,
   ACTIVEDIRECTORYID
)
AS
SELECT
   e.APP_USER_ID AS MANAGERID,
   e.EMP_NAME AS MANAGERNAME,
   e.EMP_NAME_AR AS MANAGERNAME_AR,
   e.EMAIL,
   e.LOC3 AS DEPARTMENTID,
   d.LOC_NAME_AR AS DEPARTMENTNAME,
   COALESCE(e.DESIGNATION_DESC, e.DESIGNATION) AS JOBTITLE,
   COALESCE(e.GSM, e.PHONE) AS PHONE,
   e.APP_USER_ID AS ACTIVEDIRECTORYID
FROM AD_EMPLOYEE e
LEFT JOIN AD_LOCATION d ON e.LOC3 = d.LOC_ID
WHERE e.IS_DEP_HEAD = 1;
```

**Field Mappings**:
- `MANAGERID` → `APP_USER_ID`
- `MANAGERNAME` → `EMP_NAME` (English name)
- `MANAGERNAME_AR` → `EMP_NAME_AR` (Arabic name)
- `EMAIL` → `EMAIL`
- `DEPARTMENTID` → `LOC3` (Department they manage)
- `DEPARTMENTNAME` → Department name from `AD_LOCATION`
- `JOBTITLE` → Job title
- `PHONE` → Phone number
- `ACTIVEDIRECTORYID` → `APP_USER_ID`

## HR System Structure

### AD_EMPLOYEE Table Key Fields:
- `EMP_ID` - Primary key (internal)
- `APP_USER_ID` - Employee number used for login
- `EMP_NAME` - English name
- `EMP_NAME_AR` - Arabic name
- `EMAIL` - Email address
- `LOC3` - Department ID
- `LOC4` - Section ID
- `SUPERVISOR_ID` - Direct supervisor (not used in EOM)
- `IS_DEP_HEAD` - Department head flag (1 = manager)
- `DESIGNATION` - Job title
- `DATE_OF_JOIN` - Hire date
- `GSM/PHONE` - Phone numbers

### AD_LOCATION Table Key Fields:
- `LOC_ID` - Primary key
- `TYPE_ID` - Location type (2=Directorate, 3=Department, 4=Section)
- `LOC_NAME` - English name
- `LOC_NAME_AR` - Arabic name
- `PARENT_ID` - Parent location ID

### Location Hierarchy:
- **TYPE_ID = 2**: Directorate (المديرية)
- **TYPE_ID = 3**: Department (الدائرة)
- **TYPE_ID = 4**: Section (القسم)

## Integration Notes

1. **Employee ID**: Use `APP_USER_ID` as the primary employee identifier
2. **Department Structure**: Use `LOC4` (sections) as departments in EOM
3. **Manager Relationships**: Department heads (`IS_DEP_HEAD = 1`) are managers
4. **Authentication**: Will use Active Directory with `APP_USER_ID`
5. **Names**: Arabic names preferred, English as fallback
6. **Status**: Show all employees regardless of HR status

## Next Steps

1. Create synonyms or database links in EOM database
2. Update EOM application connection strings
3. Test all EOM functionality with HR views
4. Implement Active Directory authentication
5. Handle any data type differences between Oracle and MySQL

## Sample Data

Based on the test data:
- Employee 7426 (Majid) - LOC3=52, LOC4=267
- Department 52 - "دائرة تقنية المعلومات"
- Section 267 - "قسم نظم المعلومات والشبكات"
- Manager relationship: Find IS_DEP_HEAD=1 in same LOC3