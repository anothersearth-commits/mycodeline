-- Check if there are objectives for testing
-- Run this to see current data state

-- Check active cycles
SELECT 'Active Cycles:' AS INFO FROM DUAL;
SELECT OBJECTIVECYCLEID, YEAR, HALF, STARTDATE, ENDDATE, ISACTIVE 
FROM EOM."OBJECTIVECYCLES" 
WHERE ISACTIVE = 1;

-- Check employees
SELECT 'Employees:' AS INFO FROM DUAL;
SELECT EMPLOYEEID, FIRSTNAME, LASTNAME, EMAIL 
FROM EOM."EMPLOYEES" 
WHERE ROWNUM <= 5;

-- Check objectives
SELECT 'Objectives:' AS INFO FROM DUAL;
SELECT OBJECTIVEID, EMPLOYEEID, OBJECTIVECYCLEID, OBJECTIVETITLE
FROM EOM."OBJECTIVES"
WHERE ROWNUM <= 5;

-- Check specific employee objectives in active cycle
SELECT 'Employee 1 Objectives in Active Cycle:' AS INFO FROM DUAL;
SELECT O.OBJECTIVEID, O.EMPLOYEEID, O.OBJECTIVECYCLEID, O.OBJECTIVETITLE
FROM EOM."OBJECTIVES" O
JOIN EOM."OBJECTIVECYCLES" OC ON O.OBJECTIVECYCLEID = OC.OBJECTIVECYCLEID
WHERE O.EMPLOYEEID = 1 AND OC.ISACTIVE = 1;

-- Insert test objective if none exists
INSERT INTO EOM."OBJECTIVES" (
    OBJECTIVEID,
    EMPLOYEEID,
    OBJECTIVECYCLEID,
    OBJECTIVETITLE,
    HIGHLEVELGOAL,
    CLASSIFICATION,
    RESULTDESCRIPTION,
    CREATEDAT,
    UPDATEDAT
) 
SELECT 
    1,
    1,
    1,
    'تطوير نظام إدارة الموارد البشرية',
    'تحسين كفاءة إدارة الموارد البشرية',
    'هدف يساهم في تحقيق المهام والاختصاصات الوظيفية',
    'تطوير وتحسين نظام إدارة الموارد البشرية لزيادة الكفاءة وتبسيط العمليات الإدارية',
    SYSDATE,
    SYSDATE
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM EOM."OBJECTIVES" 
    WHERE EMPLOYEEID = 1 AND OBJECTIVECYCLEID = 1
);

COMMIT;