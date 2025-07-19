# AI Objectives & Messaging System - Database Schema

## Overview
This document contains the complete database schema and SQL scripts for the AI Objectives & Messaging system. The system manages employee objectives in half-yearly cycles and generates personalized AI-powered messages and advice.

## Database Schema Design

### 1. ObjectiveCycles Table
Manages half-yearly objective cycles (2 per year: Jan-Jun, Jul-Dec).

```sql
CREATE TABLE ObjectiveCycles (
    ObjectiveCycleId INT IDENTITY(1,1) PRIMARY KEY,
    Year SMALLINT NOT NULL,
    Half TINYINT NOT NULL CHECK (Half IN (1, 2)), -- 1 = Jan-Jun, 2 = Jul-Dec
    StartDate DATE NULL,
    EndDate DATE NULL,
    IsActive BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    
    CONSTRAINT UQ_ObjectiveCycles_Year_Half UNIQUE (Year, Half)
);

-- Index for active cycle lookups
CREATE INDEX IX_ObjectiveCycles_IsActive ON ObjectiveCycles (IsActive) WHERE IsActive = 1;
```

### 2. Objectives Table
Stores individual employee objectives imported from external systems.

```sql
CREATE TABLE Objectives (
    ObjectiveId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ObjectiveCycleId INT NOT NULL,
    EmployeeId INT NOT NULL, -- References Employee view
    ObjectiveTitle NVARCHAR(300) NOT NULL,
    Classification NVARCHAR(200) NULL,
    ResultDescription NVARCHAR(MAX) NULL,
    WeightScore DECIMAL(8,2) NULL,
    ThresholdExceeds DECIMAL(8,2) NULL,
    ThresholdMeets DECIMAL(8,2) NULL,
    ThresholdBelow DECIMAL(8,2) NULL,
    ActualScore DECIMAL(8,2) NULL,
    HighLevelGoal NVARCHAR(300) NULL,
    Category NVARCHAR(100) NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Objectives_ObjectiveCycles 
        FOREIGN KEY (ObjectiveCycleId) REFERENCES ObjectiveCycles(ObjectiveCycleId)
);

-- Primary indexes for performance
CREATE INDEX IX_Objectives_EmployeeId_CycleId ON Objectives (EmployeeId, ObjectiveCycleId);
CREATE INDEX IX_Objectives_CycleId ON Objectives (ObjectiveCycleId);
CREATE INDEX IX_Objectives_EmployeeId ON Objectives (EmployeeId);
```

### 3. AiGeneratedMessages Table
Stores AI-generated messages and advice for each objective.

```sql
CREATE TABLE AiGeneratedMessages (
    AiMessageId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ObjectiveId BIGINT NOT NULL,
    EmployeeId INT NOT NULL, -- Denormalized for faster filtering
    ObjectiveCycleId INT NOT NULL, -- Denormalized for faster filtering
    MessageBody NVARCHAR(MAX) NOT NULL,
    AdviceBody NVARCHAR(MAX) NOT NULL,
    StyleTag NVARCHAR(50) NULL,
    ModelName NVARCHAR(50) NULL,
    GeneratedAt DATETIME2 DEFAULT GETUTCDATE(),
    IsActive BIT NOT NULL DEFAULT 1,
    
    CONSTRAINT FK_AiGeneratedMessages_Objectives 
        FOREIGN KEY (ObjectiveId) REFERENCES Objectives(ObjectiveId) ON DELETE CASCADE
);

-- Composite index for employee message lookups
CREATE INDEX IX_AiGeneratedMessages_Employee_Cycle_Active 
    ON AiGeneratedMessages (EmployeeId, ObjectiveCycleId, IsActive);

-- Index for objective-specific message lookups
CREATE INDEX IX_AiGeneratedMessages_ObjectiveId_Active 
    ON AiGeneratedMessages (ObjectiveId, IsActive);
```

## Database Initialization Scripts

### 1. Create Tables Script
```sql
-- Create ObjectiveCycles table
CREATE TABLE ObjectiveCycles (
    ObjectiveCycleId INT IDENTITY(1,1) PRIMARY KEY,
    Year SMALLINT NOT NULL,
    Half TINYINT NOT NULL CHECK (Half IN (1, 2)),
    StartDate DATE NULL,
    EndDate DATE NULL,
    IsActive BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    
    CONSTRAINT UQ_ObjectiveCycles_Year_Half UNIQUE (Year, Half)
);

-- Create Objectives table
CREATE TABLE Objectives (
    ObjectiveId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ObjectiveCycleId INT NOT NULL,
    EmployeeId INT NOT NULL,
    ObjectiveTitle NVARCHAR(300) NOT NULL,
    Classification NVARCHAR(200) NULL,
    ResultDescription NVARCHAR(MAX) NULL,
    WeightScore DECIMAL(8,2) NULL,
    ThresholdExceeds DECIMAL(8,2) NULL,
    ThresholdMeets DECIMAL(8,2) NULL,
    ThresholdBelow DECIMAL(8,2) NULL,
    ActualScore DECIMAL(8,2) NULL,
    HighLevelGoal NVARCHAR(300) NULL,
    Category NVARCHAR(100) NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Objectives_ObjectiveCycles 
        FOREIGN KEY (ObjectiveCycleId) REFERENCES ObjectiveCycles(ObjectiveCycleId)
);

-- Create AiGeneratedMessages table
CREATE TABLE AiGeneratedMessages (
    AiMessageId BIGINT IDENTITY(1,1) PRIMARY KEY,
    ObjectiveId BIGINT NOT NULL,
    EmployeeId INT NOT NULL,
    ObjectiveCycleId INT NOT NULL,
    MessageBody NVARCHAR(MAX) NOT NULL,
    AdviceBody NVARCHAR(MAX) NOT NULL,
    StyleTag NVARCHAR(50) NULL,
    ModelName NVARCHAR(50) NULL,
    GeneratedAt DATETIME2 DEFAULT GETUTCDATE(),
    IsActive BIT NOT NULL DEFAULT 1,
    
    CONSTRAINT FK_AiGeneratedMessages_Objectives 
        FOREIGN KEY (ObjectiveId) REFERENCES Objectives(ObjectiveId) ON DELETE CASCADE
);
```

### 2. Create Indexes Script
```sql
-- ObjectiveCycles indexes
CREATE INDEX IX_ObjectiveCycles_IsActive ON ObjectiveCycles (IsActive) WHERE IsActive = 1;

-- Objectives indexes
CREATE INDEX IX_Objectives_EmployeeId_CycleId ON Objectives (EmployeeId, ObjectiveCycleId);
CREATE INDEX IX_Objectives_CycleId ON Objectives (ObjectiveCycleId);
CREATE INDEX IX_Objectives_EmployeeId ON Objectives (EmployeeId);

-- AiGeneratedMessages indexes
CREATE INDEX IX_AiGeneratedMessages_Employee_Cycle_Active 
    ON AiGeneratedMessages (EmployeeId, ObjectiveCycleId, IsActive);
CREATE INDEX IX_AiGeneratedMessages_ObjectiveId_Active 
    ON AiGeneratedMessages (ObjectiveId, IsActive);
```

### 3. Sample Data Script
```sql
-- Insert sample objective cycles
INSERT INTO ObjectiveCycles (Year, Half, StartDate, EndDate, IsActive)
VALUES 
    (2024, 1, '2024-01-01', '2024-06-30', 0),
    (2024, 2, '2024-07-01', '2024-12-31', 1),
    (2025, 1, '2025-01-01', '2025-06-30', 0);

-- Sample objectives (using example data from specification)
DECLARE @CycleId INT = (SELECT ObjectiveCycleId FROM ObjectiveCycles WHERE Year = 2024 AND Half = 2);

INSERT INTO Objectives (ObjectiveCycleId, EmployeeId, ObjectiveTitle, Classification, ResultDescription, WeightScore, ThresholdExceeds, ThresholdMeets, ThresholdBelow)
VALUES 
    (@CycleId, 1, N'تفعيل التحول الرقمي', N'هدف يساهم في تحقيق الخطة السنوية', N'تطوير وتحسين برنامج إدارة الوثائق والمراسلات لزيادة كفاءة التحول الرقمي', 14.00, 12.00, 9.00, 2.00),
    (@CycleId, 1, N'زيادة الكفاءة و الترشيد الإنفاق المالي', N'هدف يساهم في تحقيق الخطة السنوية', N'تحسين التكامل الإلكتروني بين الأنظمة لتقليل التكاليف وزيادة الكفاءة', 8.00, 7.00, 5.00, 3.00),
    (@CycleId, 2, N'إدارة وصيانة Active Directory', N'هدف يساهم في تحقيق المهام والاختصاصات الوظيفية', N'إدارة حسابات المستخدمين، الصلاحيات، متابعة الـ GPO وReplication', NULL, NULL, NULL, NULL);
```

## Views for Data Access

### 1. Employee Objectives View
```sql
CREATE VIEW VW_EmployeeObjectives AS
SELECT 
    o.ObjectiveId,
    o.ObjectiveCycleId,
    o.EmployeeId,
    o.ObjectiveTitle,
    o.Classification,
    o.ResultDescription,
    o.WeightScore,
    o.ThresholdExceeds,
    o.ThresholdMeets,
    o.ThresholdBelow,
    o.ActualScore,
    o.HighLevelGoal,
    o.Category,
    oc.Year,
    oc.Half,
    oc.IsActive AS CycleIsActive,
    e.FirstName,
    e.LastName,
    e.Email,
    e.JobTitle,
    e.DepartmentId
FROM Objectives o
    INNER JOIN ObjectiveCycles oc ON o.ObjectiveCycleId = oc.ObjectiveCycleId
    INNER JOIN EOM.VW_EOM_EMPLOYEES_V e ON o.EmployeeId = e.EmployeeId
WHERE e.IsActive = 1;
```

### 2. Active Messages View
```sql
CREATE VIEW VW_ActiveAiMessages AS
SELECT 
    am.AiMessageId,
    am.ObjectiveId,
    am.EmployeeId,
    am.ObjectiveCycleId,
    am.MessageBody,
    am.AdviceBody,
    am.StyleTag,
    am.ModelName,
    am.GeneratedAt,
    o.ObjectiveTitle,
    o.Classification,
    oc.Year,
    oc.Half,
    e.FirstName,
    e.LastName,
    e.Email
FROM AiGeneratedMessages am
    INNER JOIN Objectives o ON am.ObjectiveId = o.ObjectiveId
    INNER JOIN ObjectiveCycles oc ON am.ObjectiveCycleId = oc.ObjectiveCycleId
    INNER JOIN EOM.VW_EOM_EMPLOYEES_V e ON am.EmployeeId = e.EmployeeId
WHERE am.IsActive = 1 AND e.IsActive = 1;
```

## Stored Procedures

### 1. Get Employee Objectives for AI Generation
```sql
CREATE PROCEDURE sp_GetEmployeeObjectivesForAI
    @EmployeeId INT,
    @ObjectiveCycleId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        o.ObjectiveId,
        o.ObjectiveCycleId,
        o.EmployeeId,
        o.ObjectiveTitle,
        o.Classification,
        o.ResultDescription,
        e.FirstName,
        e.LastName,
        e.FirstName + ' ' + e.LastName AS FullName,
        CASE 
            WHEN am.AiMessageId IS NOT NULL THEN 1 
            ELSE 0 
        END AS HasActiveMessage
    FROM Objectives o
        INNER JOIN ObjectiveCycles oc ON o.ObjectiveCycleId = oc.ObjectiveCycleId
        INNER JOIN EOM.VW_EOM_EMPLOYEES_V e ON o.EmployeeId = e.EmployeeId
        LEFT JOIN AiGeneratedMessages am ON o.ObjectiveId = am.ObjectiveId AND am.IsActive = 1
    WHERE o.EmployeeId = @EmployeeId
        AND (@ObjectiveCycleId IS NULL OR o.ObjectiveCycleId = @ObjectiveCycleId)
        AND e.IsActive = 1
    ORDER BY o.ObjectiveId;
END;
```

### 2. Save AI Generated Message
```sql
CREATE PROCEDURE sp_SaveAiGeneratedMessage
    @ObjectiveId BIGINT,
    @EmployeeId INT,
    @ObjectiveCycleId INT,
    @MessageBody NVARCHAR(MAX),
    @AdviceBody NVARCHAR(MAX),
    @StyleTag NVARCHAR(50) = NULL,
    @ModelName NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRANSACTION;
    
    TRY
        -- Deactivate existing messages for this objective
        UPDATE AiGeneratedMessages 
        SET IsActive = 0, UpdatedAt = GETUTCDATE()
        WHERE ObjectiveId = @ObjectiveId AND IsActive = 1;
        
        -- Insert new message
        INSERT INTO AiGeneratedMessages (
            ObjectiveId, EmployeeId, ObjectiveCycleId, 
            MessageBody, AdviceBody, StyleTag, ModelName
        )
        VALUES (
            @ObjectiveId, @EmployeeId, @ObjectiveCycleId,
            @MessageBody, @AdviceBody, @StyleTag, @ModelName
        );
        
        COMMIT TRANSACTION;
        
        SELECT SCOPE_IDENTITY() AS NewMessageId;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
```

## Cleanup and Maintenance

### 1. Cleanup Old Messages
```sql
CREATE PROCEDURE sp_CleanupOldAiMessages
    @RetentionDays INT = 90
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM AiGeneratedMessages 
    WHERE IsActive = 0 
        AND GeneratedAt < DATEADD(DAY, -@RetentionDays, GETUTCDATE());
    
    SELECT @@ROWCOUNT AS DeletedRows;
END;
```

### 2. Update Audit Columns Trigger
```sql
CREATE TRIGGER tr_ObjectiveCycles_UpdateAudit
ON ObjectiveCycles
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE ObjectiveCycles
    SET UpdatedAt = GETUTCDATE()
    WHERE ObjectiveCycleId IN (SELECT ObjectiveCycleId FROM inserted);
END;

CREATE TRIGGER tr_Objectives_UpdateAudit
ON Objectives
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Objectives
    SET UpdatedAt = GETUTCDATE()
    WHERE ObjectiveId IN (SELECT ObjectiveId FROM inserted);
END;
```

## Security Considerations

1. **Access Control**: Implement proper role-based access for AI message generation
2. **Data Validation**: Validate input lengths and content before AI generation
3. **API Key Security**: Store OpenAI API keys securely (encrypted in configuration)
4. **Audit Trail**: All message generations should be logged
5. **Rate Limiting**: Implement rate limiting for AI generation requests