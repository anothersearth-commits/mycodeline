# Ejadah Evaluation System Database Design

## Overview
This document outlines the database design for the Ejadah evaluation system that will be used to restrict nominations based on employee performance ratings. Managers cannot nominate employees who have "Poor" or "Moderate" ratings in their latest Ejadah evaluation.

## Table Structures

### 1. EJADAH_CYCLES Table
Stores the Ejadah evaluation cycles (twice per year).

```sql
CREATE TABLE EJADAH_CYCLES (
    EJADAH_CYCLE_ID     NUMBER(10)     PRIMARY KEY,
    YEAR                NUMBER(4)      NOT NULL,
    HALF                NUMBER(1)      NOT NULL CHECK (HALF IN (1, 2)),
    START_DATE          DATE           NOT NULL,
    END_DATE            DATE           NOT NULL,
    IS_ACTIVE           NUMBER(1)      DEFAULT 0 CHECK (IS_ACTIVE IN (0, 1)),
    CREATED_DATE        DATE           DEFAULT SYSDATE,
    CREATED_BY          VARCHAR2(100),
    CONSTRAINT UK_EJADAH_CYCLES_YEAR_HALF UNIQUE (YEAR, HALF)
);

-- Comments
COMMENT ON TABLE EJADAH_CYCLES IS 'Ejadah evaluation cycles - twice per year';
COMMENT ON COLUMN EJADAH_CYCLES.EJADAH_CYCLE_ID IS 'Primary key for Ejadah cycle';
COMMENT ON COLUMN EJADAH_CYCLES.YEAR IS 'Evaluation year (e.g., 2024)';
COMMENT ON COLUMN EJADAH_CYCLES.HALF IS 'Half of year (1 = First Half, 2 = Second Half)';
COMMENT ON COLUMN EJADAH_CYCLES.START_DATE IS 'Cycle start date';
COMMENT ON COLUMN EJADAH_CYCLES.END_DATE IS 'Cycle end date';
COMMENT ON COLUMN EJADAH_CYCLES.IS_ACTIVE IS 'Whether this cycle is currently active (0 = No, 1 = Yes)';
```

### 2. EJADAH_EMPLOYEE_SCORES Table
Stores individual employee scores for each Ejadah cycle.

```sql
CREATE TABLE EJADAH_EMPLOYEE_SCORES (
    EJADAH_EMPLOYEE_SCORE_ID    NUMBER(10)     PRIMARY KEY,
    EJADAH_CYCLE_ID             NUMBER(10)     NOT NULL,
    EMPLOYEE_ID                 NUMBER(10)     NOT NULL,
    SCORE                       VARCHAR2(20)   NOT NULL CHECK (SCORE IN ('EXCELLENT', 'VERY_GOOD', 'GOOD', 'MODERATE', 'POOR')),
    SCORE_NUMERIC               NUMBER(5,2),
    EVALUATION_DATE             DATE           NOT NULL,
    EVALUATOR_ID                NUMBER(10),
    COMMENTS                    CLOB,
    CREATED_DATE                DATE           DEFAULT SYSDATE,
    CREATED_BY                  VARCHAR2(100),
    UPDATED_DATE                DATE,
    UPDATED_BY                  VARCHAR2(100),
    CONSTRAINT FK_EJADAH_SCORES_CYCLE 
        FOREIGN KEY (EJADAH_CYCLE_ID) REFERENCES EJADAH_CYCLES(EJADAH_CYCLE_ID),
    CONSTRAINT FK_EJADAH_SCORES_EMPLOYEE 
        FOREIGN KEY (EMPLOYEE_ID) REFERENCES EMPLOYEES(EMPLOYEE_ID),
    CONSTRAINT FK_EJADAH_SCORES_EVALUATOR 
        FOREIGN KEY (EVALUATOR_ID) REFERENCES EMPLOYEES(EMPLOYEE_ID),
    CONSTRAINT UK_EJADAH_SCORES_CYCLE_EMPLOYEE UNIQUE (EJADAH_CYCLE_ID, EMPLOYEE_ID)
);

-- Comments
COMMENT ON TABLE EJADAH_EMPLOYEE_SCORES IS 'Employee scores for Ejadah evaluations';
COMMENT ON COLUMN EJADAH_EMPLOYEE_SCORES.EJADAH_EMPLOYEE_SCORE_ID IS 'Primary key for employee score';
COMMENT ON COLUMN EJADAH_EMPLOYEE_SCORES.EJADAH_CYCLE_ID IS 'Reference to Ejadah cycle';
COMMENT ON COLUMN EJADAH_EMPLOYEE_SCORES.EMPLOYEE_ID IS 'Reference to employee being evaluated';
COMMENT ON COLUMN EJADAH_EMPLOYEE_SCORES.SCORE IS 'Evaluation score (EXCELLENT, VERY_GOOD, GOOD, MODERATE, POOR)';
COMMENT ON COLUMN EJADAH_EMPLOYEE_SCORES.SCORE_NUMERIC IS 'Optional numeric score representation';
COMMENT ON COLUMN EJADAH_EMPLOYEE_SCORES.EVALUATION_DATE IS 'Date when evaluation was completed';
COMMENT ON COLUMN EJADAH_EMPLOYEE_SCORES.EVALUATOR_ID IS 'Employee who conducted the evaluation';
COMMENT ON COLUMN EJADAH_EMPLOYEE_SCORES.COMMENTS IS 'Additional evaluation comments';
```

## Indexes

```sql
-- Performance indexes
CREATE INDEX IDX_EJADAH_CYCLES_YEAR_HALF ON EJADAH_CYCLES (YEAR, HALF);
CREATE INDEX IDX_EJADAH_CYCLES_ACTIVE ON EJADAH_CYCLES (IS_ACTIVE);
CREATE INDEX IDX_EJADAH_SCORES_EMPLOYEE ON EJADAH_EMPLOYEE_SCORES (EMPLOYEE_ID);
CREATE INDEX IDX_EJADAH_SCORES_CYCLE ON EJADAH_EMPLOYEE_SCORES (EJADAH_CYCLE_ID);
CREATE INDEX IDX_EJADAH_SCORES_SCORE ON EJADAH_EMPLOYEE_SCORES (SCORE);
CREATE INDEX IDX_EJADAH_SCORES_EVAL_DATE ON EJADAH_EMPLOYEE_SCORES (EVALUATION_DATE);
```

## Sequences

```sql
-- Sequences for primary keys
CREATE SEQUENCE SEQ_EJADAH_CYCLES
    START WITH 1
    INCREMENT BY 1
    NOCACHE
    NOCYCLE;

CREATE SEQUENCE SEQ_EJADAH_EMPLOYEE_SCORES
    START WITH 1
    INCREMENT BY 1
    NOCACHE
    NOCYCLE;
```

## Business Rules & Restrictions

### Nomination Restriction Logic

1. **Core Restriction**: Managers cannot nominate employees who have "POOR" or "MODERATE" scores in their latest Ejadah evaluation.

2. **Latest Evaluation Logic**: 
   - Find the most recent completed Ejadah cycle
   - Check the employee's score in that cycle
   - If score is "POOR" or "MODERATE", prevent nomination

3. **No Evaluation Exception**: 
   - If an employee has never been evaluated in Ejadah, they can be nominated
   - New employees who haven't completed an evaluation cycle are eligible

### Score Hierarchy
```
EXCELLENT    - Can be nominated ✅
VERY_GOOD    - Can be nominated ✅
GOOD         - Can be nominated ✅
MODERATE     - Cannot be nominated ❌
POOR         - Cannot be nominated ❌
```

## Implementation Logic

### SQL Query to Check Nomination Eligibility

```sql
-- Query to get employees eligible for nomination
SELECT e.EMPLOYEE_ID, e.FIRST_NAME, e.LAST_NAME,
       es.SCORE as LATEST_EJADAH_SCORE,
       ec.YEAR, ec.HALF
FROM EMPLOYEES e
LEFT JOIN (
    -- Get latest Ejadah score for each employee
    SELECT es1.EMPLOYEE_ID, es1.SCORE, es1.EJADAH_CYCLE_ID
    FROM EJADAH_EMPLOYEE_SCORES es1
    INNER JOIN EJADAH_CYCLES ec1 ON es1.EJADAH_CYCLE_ID = ec1.EJADAH_CYCLE_ID
    WHERE (ec1.YEAR, ec1.HALF) = (
        SELECT MAX(ec2.YEAR), MAX(ec2.HALF)
        FROM EJADAH_CYCLES ec2
        WHERE ec2.YEAR = (SELECT MAX(YEAR) FROM EJADAH_CYCLES)
    )
) es ON e.EMPLOYEE_ID = es.EMPLOYEE_ID
LEFT JOIN EJADAH_CYCLES ec ON es.EJADAH_CYCLE_ID = ec.EJADAH_CYCLE_ID
WHERE 
    -- Employee is active
    e.IS_ACTIVE = 1
    -- Either no Ejadah score or score is not POOR/MODERATE
    AND (es.SCORE IS NULL OR es.SCORE NOT IN ('POOR', 'MODERATE'))
    -- Employee is in manager's department
    AND e.DEPARTMENT_ID = :managerDepartmentId
    -- Employee is not the manager themselves
    AND e.EMPLOYEE_ID != :managerId;
```

### Validation Logic for Nomination Controller

```csharp
// Method to check if employee can be nominated
public bool CanEmployeeBeNominated(long employeeId)
{
    var latestEjadahScore = _context.EjadahEmployeeScores
        .Include(es => es.EjadahCycle)
        .Where(es => es.EmployeeId == employeeId)
        .OrderByDescending(es => es.EjadahCycle.Year)
        .ThenByDescending(es => es.EjadahCycle.Half)
        .FirstOrDefault();

    // If no Ejadah evaluation exists, employee can be nominated
    if (latestEjadahScore == null)
        return true;

    // Check if latest score allows nomination
    return !new[] { "POOR", "MODERATE" }.Contains(latestEjadahScore.Score);
}
```

## Data Migration Considerations

1. **Historical Data**: Existing employees may need initial Ejadah scores populated
2. **Default Scores**: Consider if employees without evaluations should have default scores
3. **Cycle Setup**: Create initial cycles for current and previous years

## Sample Data

```sql
-- Sample Ejadah Cycles
INSERT INTO EJADAH_CYCLES (EJADAH_CYCLE_ID, YEAR, HALF, START_DATE, END_DATE, IS_ACTIVE)
VALUES (1, 2024, 1, DATE '2024-01-01', DATE '2024-06-30', 0);

INSERT INTO EJADAH_CYCLES (EJADAH_CYCLE_ID, YEAR, HALF, START_DATE, END_DATE, IS_ACTIVE)
VALUES (2, 2024, 2, DATE '2024-07-01', DATE '2024-12-31', 1);

-- Sample Employee Scores
INSERT INTO EJADAH_EMPLOYEE_SCORES (EJADAH_EMPLOYEE_SCORE_ID, EJADAH_CYCLE_ID, EMPLOYEE_ID, SCORE, EVALUATION_DATE)
VALUES (1, 1, 12345, 'EXCELLENT', DATE '2024-06-15');

INSERT INTO EJADAH_EMPLOYEE_SCORES (EJADAH_EMPLOYEE_SCORE_ID, EJADAH_CYCLE_ID, EMPLOYEE_ID, SCORE, EVALUATION_DATE)
VALUES (2, 1, 12346, 'MODERATE', DATE '2024-06-15');
```

## Integration Points

1. **Nomination Creation**: Check Ejadah scores before allowing nomination
2. **Employee Selection**: Filter out ineligible employees in the nomination UI
3. **Admin Interface**: Provide screens to manage Ejadah cycles and scores
4. **Reporting**: Generate reports on nomination eligibility based on Ejadah scores

## Error Messages

- Arabic: "لا يمكن ترشيح هذا الموظف بسبب تقييم أجادة ضعيف أو متوسط"
- English: "This employee cannot be nominated due to Poor or Moderate Ejadah evaluation"