-- Run this script manually in your Oracle database

-- Insert an active objective cycle for 2025 - First Half
INSERT INTO "ObjectiveCycles" (
    "ObjectiveCycleId",
    "Year", 
    "Half", 
    "StartDate", 
    "EndDate", 
    "IsActive", 
    "CreatedAt", 
    "UpdatedAt"
) VALUES (
    1,
    2025,
    1,
    DATE '2025-01-01',
    DATE '2025-06-30',
    1,
    SYSDATE,
    SYSDATE
);

-- Insert sample objective data for testing (replace EmployeeId with actual employee ID)
INSERT INTO "Objectives" (
    "ObjectiveId",
    "EmployeeId",
    "ObjectiveCycleId",
    "ObjectiveTitle",
    "HighLevelGoal",
    "Classification",
    "ResultDescription",
    "CreatedAt",
    "UpdatedAt"
) VALUES (
    1,
    1, -- Replace with actual employee ID
    1,
    'تطوير نظام إدارة الموارد البشرية',
    'تحسين كفاءة إدارة الموارد البشرية',
    'هدف يساهم في تحقيق المهام والاختصاصات الوظيفية',
    'تطوير وتحسين نظام إدارة الموارد البشرية لزيادة الكفاءة وتبسيط العمليات الإدارية',
    SYSDATE,
    SYSDATE
);

-- Check if data was inserted successfully
SELECT * FROM "ObjectiveCycles" WHERE "IsActive" = 1;
SELECT * FROM "Objectives" WHERE "ObjectiveCycleId" = 1;

COMMIT;