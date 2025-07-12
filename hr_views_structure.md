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
   MANAGERNAME,
   IS_MANAGER
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
   COALESCE(m.EMP_NAME_AR, m.EMP_NAME) AS MANAGERNAME,
   COALESCE(e.IS_DEP_HEAD, 0) AS IS_MANAGER
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
- `IS_MANAGER` → `IS_DEP_HEAD` (1 = department head/manager, 0 = regular employee)

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








CREATE TABLE DIGIHR_TEST.AD_EMPLOYEE
(
  EMP_ID                     NUMBER(19)         NOT NULL,
  COMPANY_EMP_ID             VARCHAR2(50 BYTE),
  APP_USER_ID                VARCHAR2(100 BYTE),
  EMP_NAME                   VARCHAR2(100 BYTE) NOT NULL,
  EMP_TYPE                   VARCHAR2(20 BYTE)  NOT NULL,
  DESIGNATION                VARCHAR2(100 BYTE),
  DEPARTMENT                 VARCHAR2(100 BYTE),
  DIVISION                   VARCHAR2(100 BYTE),
  STATUS                     NUMBER(3),
  PHONE                      VARCHAR2(30 BYTE),
  GSM                        VARCHAR2(20 BYTE),
  EMAIL                      VARCHAR2(80 BYTE),
  SUPERVISOR_ID              NUMBER(19),
  CONTRACTOR_COMPANY         VARCHAR2(100 BYTE),
  HSE_PP_NO                  VARCHAR2(20 BYTE),
  HSE_PP_ISSUED_BY           VARCHAR2(100 BYTE),
  ADDRESS                    VARCHAR2(300 BYTE),
  DOB                        DATE,
  DRIVING_LICENSE_NO         VARCHAR2(20 BYTE),
  DRIVING_LICENSE_EXPIRY     DATE,
  CIVIL_ID                   VARCHAR2(50 BYTE),
  CIVIL_ID_EXPIRY            DATE,
  PP_NO                      VARCHAR2(50 BYTE),
  PP_EXPIRY                  DATE,
  DD_PERMIT_EXPIRY           DATE,
  NATIONALITY                VARCHAR2(50 BYTE),
  GENDER                     VARCHAR2(1 BYTE),
  FAX_NO                     VARCHAR2(50 BYTE),
  AD_REMOVE_DATE             DATE,
  IS_EXIST_IN_AD             VARCHAR2(1 BYTE),
  PHOTO                      NCLOB,
  INSERT_DATE                DATE               NOT NULL,
  UPDATE_BY                  NUMBER(19),
  UPDATE_DATE                DATE,
  PHOTO_PATH                 VARCHAR2(1000 BYTE),
  PWD                        VARCHAR2(20 BYTE),
  SALARY                     NUMBER(10),
  IS_DRIVER                  NUMBER(3),
  GSM2                       VARCHAR2(20 BYTE),
  VISA_NO                    VARCHAR2(50 BYTE),
  VISA_EXPIRY                DATE,
  PP_DEPOSITED               NUMBER(3),
  DATE_OF_JOIN               DATE,
  IS_USER                    NUMBER(3),
  DATE_OF_LEAVING            DATE,
  EMP_LOC_TYPE               VARCHAR2(1 BYTE),
  LOC_ID                     NUMBER(10),
  COMPANY_ID                 NUMBER(10),
  IS_FIELD_SUPER             NUMBER(3),
  MOB_LOGIN_ACCESS           NUMBER(3),
  WEB_LOGIN_ACCESS           NUMBER(3),
  TS_LOGIN_ACCESS            NUMBER(3),
  IS_WORK_FROM_HOME          NUMBER(3),
  LABOUR_CARD_NO             VARCHAR2(20 BYTE),
  LABOUR_CARD_EXPIRY         DATE,
  EMIRATES_ID_NO             VARCHAR2(20 BYTE),
  EMIRATES_ID_EXPIRY         DATE,
  FILE_NO                    VARCHAR2(30 BYTE),
  REMARKS                    VARCHAR2(500 BYTE),
  LAST_WORKING_DAY           DATE,
  FATHER_NAME                VARCHAR2(50 BYTE),
  MOTHER_NAME                VARCHAR2(50 BYTE),
  SPOUSE_NAME                VARCHAR2(50 BYTE),
  MARITAL_STATUS             VARCHAR2(2 BYTE),
  NO_OF_CHILDREN             NUMBER(3),
  RELIGION                   VARCHAR2(30 BYTE),
  EMP_RELATIVE               NUMBER(19),
  DATE_OF_RESIGN             DATE,
  BLOCK_MOB_LOGIN            VARCHAR2(1 BYTE),
  MOB_DEVICE_ID              NCLOB,
  EMP_NAME_AR                VARCHAR2(100 BYTE),
  QR_LOGIN                   NUMBER(3),
  CALENDAR_HDR_ID            NUMBER(5),
  DESIGNATION_DESC           VARCHAR2(100 BYTE),
  DEPARTMENT_DESC            VARCHAR2(100 BYTE),
  LM                         VARCHAR2(200 BYTE),
  PAYMENT_MODE               VARCHAR2(10 BYTE),
  BRANCH                     VARCHAR2(20 BYTE),
  REGION                     VARCHAR2(20 BYTE),
  SECTION                    VARCHAR2(20 BYTE),
  DEVICE_USER_ID             NUMBER(19),
  IS_MANAGER                 NUMBER(19),
  IS_ADMIN                   NUMBER(19),
  LEAVE_RIGHT                NUMBER(19),
  CALENDAR_RIGHT             NUMBER(19),
  SHIFT_RIGHT                NUMBER(19),
  REPORT_RIGHT               NUMBER(19),
  NOT_SHOW_IN_REPORT         NUMBER(19),
  EMP_CLASS                  VARCHAR2(10 BYTE),
  ADMINISTRATIVE_DIVISION    NUMBER(19),
  GOVERNORATE_BRANCH_OFFICE  NUMBER(19),
  LOC3                       NUMBER(19),
  LOC4                       NUMBER(19),
  DAILY_REPORT_RIGHT         NUMBER(10),
  LEAVE_REPORT_RIGHT         NUMBER(10),
  ABSENSE_REPORT_RIGHT       NUMBER(10),
  WORK_REPORT_RIGHT          NUMBER(10),
  EMPLOYEE_RIGHT             NUMBER(19),
  IS_ADMIN_HEAD              NUMBER(3),
  IS_BRANCH_HEAD             NUMBER(3),
  IS_DEP_HEAD                NUMBER(3),
  IS_SECTION_HEAD            NUMBER(3),
  IS_HR                      NUMBER(3),
  ALL_USER_ACCESS            NUMBER(3),
  JOB_LEVEL                  VARCHAR2(20 BYTE),
  GOVERNOR                   NUMBER(3),
  DIRECTORATE_GENERAL        NUMBER(3),
  IS_EMAIL                   NUMBER(3),
  ROLE_LEVEL                 VARCHAR2(50 BYTE)
)



CREATE TABLE DIGIHR_TEST.AD_LOCATION
(
  LOC_ID       NUMBER(10)                       NOT NULL,
  TYPE_ID      NUMBER(10)                       NOT NULL,
  LOC_NAME     VARCHAR2(100 BYTE)               NOT NULL,
  LOC_NAME_AR  VARCHAR2(100 BYTE),
  PARENT_ID    NUMBER(10),
  DISPLAY_SEQ  NUMBER(10),
  UPDATE_BY    VARCHAR2(100 BYTE),
  UPDATE_DATE  DATE
)