-- Migration Script for Self-Nomination Support
-- Database: Oracle 19c
-- Date: Generated for EOM System

-- 1. Add IsSelfNomination flag to AwardTypes table
ALTER TABLE "AwardTypes" ADD "IsSelfNomination" NUMBER(1) DEFAULT 0;

-- 2. Modify Nominations table to support self-nominations
-- Make ManagerId nullable for self-nominations
ALTER TABLE "Nominations" MODIFY "ManagerId" NUMBER(10) NULL;

-- Add self-nomination fields
ALTER TABLE "Nominations" ADD "IsSelfNomination" NUMBER(1) DEFAULT 0;
ALTER TABLE "Nominations" ADD "Title" NVARCHAR2(200);
ALTER TABLE "Nominations" ADD "InitiativeDetails" NCLOB;
ALTER TABLE "Nominations" ADD "AttachmentPath" NVARCHAR2(500);

-- 3. Ensure Nominations table has primary key (if not already present)
-- ALTER TABLE "Nominations" ADD CONSTRAINT PK_NOMINATIONS PRIMARY KEY ("NominationId");

-- 4. Create GroupNominationMembers table for group nominations
CREATE TABLE GROUPNOMINATIONMEMBERS (
    GROUPMEMBERID NUMBER(10) NOT NULL,
    NOMINATIONID NUMBER(10) NOT NULL,
    EMPLOYEEID NUMBER(10) NOT NULL,
    CONSTRAINT PK_GROUPNOMINATIONMEMBERS PRIMARY KEY (GROUPMEMBERID),
    CONSTRAINT FK_GROUPNOM_NOMINATION FOREIGN KEY (NOMINATIONID) REFERENCES "Nominations"("NominationId") ON DELETE CASCADE,
    CONSTRAINT UQ_GROUPNOM_NOM_EMP UNIQUE (NOMINATIONID, EMPLOYEEID)
);

-- 4. Create sequence for GroupNominationMembers
-- Drop sequence if exists and recreate
BEGIN
    EXECUTE IMMEDIATE 'DROP SEQUENCE SEQ_GROUPNOMINATION';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -2289 THEN
            RAISE;
        END IF;
END;
/

CREATE SEQUENCE SEQ_GROUPNOMINATION START WITH 1 INCREMENT BY 1;

-- Create trigger to auto-populate GROUPMEMBERID
CREATE OR REPLACE TRIGGER TRG_GROUPNOMINATION_ID
BEFORE INSERT ON GROUPNOMINATIONMEMBERS
FOR EACH ROW
WHEN (NEW.GROUPMEMBERID IS NULL)
BEGIN
    SELECT SEQ_GROUPNOMINATION.NEXTVAL INTO :NEW.GROUPMEMBERID FROM DUAL;
END;
/

-- 5. Create indexes for performance
CREATE INDEX IX_GROUPNOM_NOMINATION ON GROUPNOMINATIONMEMBERS(NOMINATIONID);
CREATE INDEX IX_GROUPNOM_EMPLOYEE ON GROUPNOMINATIONMEMBERS(EMPLOYEEID);

-- 6. Insert the two new award types
INSERT INTO "AwardTypes" ("AwardTypeId", "Name", "Description", "IsActive", "WinnerCount", "IsSelfNomination")
VALUES (2, 'الموظف المبادر', 'جائزة الموظف المبادر للمبادرات المتميزة', 1, 1, 1);

INSERT INTO "AwardTypes" ("AwardTypeId", "Name", "Description", "IsActive", "WinnerCount", "IsSelfNomination")
VALUES (3, 'الموظف المبتكر', 'جائزة الموظف المبتكر للأفكار الإبداعية', 1, 1, 1);

-- 7. Get the IDs of the newly inserted award types (you'll need these for criteria insertion)
-- Note: Run this query after inserting to get the actual IDs
SELECT "AwardTypeId", "Name" FROM "AwardTypes" WHERE "IsSelfNomination" = 1;

-- 8. Insert criteria for "الموظف المبادر" (AwardTypeId = 2)
-- Main criteria with specific IDs (5-8)
INSERT INTO "Criteria" ("CriterionId", "AwardTypeId", "Name", "WeightPercent")
VALUES (5, 2, 'مدى ارتباط المبادرة بأهداف الوحدة', 30);

INSERT INTO "Criteria" ("CriterionId", "AwardTypeId", "Name", "WeightPercent")
VALUES (6, 2, 'الأثر الإيجابي', 30);

INSERT INTO "Criteria" ("CriterionId", "AwardTypeId", "Name", "WeightPercent")
VALUES (7, 2, 'استدامة المبادرة', 20);

INSERT INTO "Criteria" ("CriterionId", "AwardTypeId", "Name", "WeightPercent")
VALUES (8, 2, 'المشاركة مع الفريق', 20);

-- 9. Insert criteria for "الموظف المبتكر" (AwardTypeId = 3)
-- Main criteria with specific IDs (9-13)
INSERT INTO "Criteria" ("CriterionId", "AwardTypeId", "Name", "WeightPercent")
VALUES (9, 3, 'مستوى الإبداع في الفكرة والتميّز عن الحلول التقليدية', 30);

INSERT INTO "Criteria" ("CriterionId", "AwardTypeId", "Name", "WeightPercent")
VALUES (10, 3, 'إمكانية تطبيق الفكرة وتحقيق أثر إيجابي', 25);

INSERT INTO "Criteria" ("CriterionId", "AwardTypeId", "Name", "WeightPercent")
VALUES (11, 3, 'مساهمة الابتكار في تقليل التكاليف أو تحسين الكفاءة', 20);

INSERT INTO "Criteria" ("CriterionId", "AwardTypeId", "Name", "WeightPercent")
VALUES (12, 3, 'استمرارية الفكرة أو قابلية التوسع', 15);

INSERT INTO "Criteria" ("CriterionId", "AwardTypeId", "Name", "WeightPercent")
VALUES (13, 3, 'مدى تأثير الابتكار على المستفيدين ورضاهم', 10);

-- 10. Sub-criteria insertion would follow here
-- Due to the complexity and length, you'll need to:
-- a) Get the CriterionId values for each main criterion inserted above
-- b) Insert the corresponding sub-criteria with their grading scales
-- Example format:
/*
INSERT INTO "SubCriteria" ("SubCriteriaId", "CriterionId", "SubCriteriaCode", "Name", "MaxScore", "GradingScale")
VALUES (SEQ_SUBCRITERIA.NEXTVAL, {CRITERION_ID}, '1.1', 'وضوح العلاقة بين أهداف المبادرة وأهداف الوحدة', 10, 
'[{"range":"8-10","description":"وضوح عال بين أهداف المبادرة وأهداف المحافظة الاستراتيجية/التشغيلية"},
{"range":"3-7","description":"وضوح متوسط بين أهداف المبادرة وأهداف المحافظة غير شامل جميع الجوانب"},
{"range":"0-2","description":"وضوح منخفض بين أهداف المبادرة وأهداف المحافظة، علاقة غير واضحة/ضعيفة"}]');
*/

-- COMMIT the transaction
COMMIT;