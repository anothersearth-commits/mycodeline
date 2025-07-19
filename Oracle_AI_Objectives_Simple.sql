-- =====================================================
-- AI Objectives & Messaging System - Oracle 19c Scripts
-- Simplified version - Tables, Sequences, Indexes only
-- Entity Framework will handle all data access
-- =====================================================

-- 1. CREATE SEQUENCES
-- =====================================================

CREATE SEQUENCE SEQ_OBJECTIVECYCLE
    START WITH 1
    INCREMENT BY 1
    NOCACHE
    NOCYCLE;

CREATE SEQUENCE SEQ_OBJECTIVE
    START WITH 1
    INCREMENT BY 1
    NOCACHE
    NOCYCLE;

CREATE SEQUENCE SEQ_AIMESSAGE
    START WITH 1
    INCREMENT BY 1
    NOCACHE
    NOCYCLE;

-- 2. CREATE TABLES
-- =====================================================

-- ObjectiveCycles Table
CREATE TABLE OBJECTIVECYCLES (
    OBJECTIVECYCLEID NUMBER(10) DEFAULT SEQ_OBJECTIVECYCLE.NEXTVAL PRIMARY KEY,
    YEAR NUMBER(4) NOT NULL,
    HALF NUMBER(1) NOT NULL CHECK (HALF IN (1, 2)),
    STARTDATE DATE,
    ENDDATE DATE,
    ISACTIVE NUMBER(1) DEFAULT 0 NOT NULL,
    CREATEDAT DATE DEFAULT SYSDATE,
    UPDATEDAT DATE DEFAULT SYSDATE,
    CONSTRAINT CHK_OBJECTIVECYCLE_ACTIVE CHECK (ISACTIVE IN (0, 1))
);

-- Objectives Table
CREATE TABLE OBJECTIVES (
    OBJECTIVEID NUMBER(19) DEFAULT SEQ_OBJECTIVE.NEXTVAL PRIMARY KEY,
    OBJECTIVECYCLEID NUMBER(10) NOT NULL,
    EMPLOYEEID NUMBER(10) NOT NULL,
    OBJECTIVETITLE NVARCHAR2(300) NOT NULL,
    CLASSIFICATION NVARCHAR2(200),
    RESULTDESCRIPTION NCLOB,
    WEIGHTSCORE NUMBER(8,2),
    THRESHOLDEXCEEDS NUMBER(8,2),
    THRESHOLDMEETS NUMBER(8,2),
    THRESHOLDBELOW NUMBER(8,2),
    ACTUALSCORE NUMBER(8,2),
    HIGHLEVELGOAL NVARCHAR2(300),
    CATEGORY NVARCHAR2(100),
    CREATEDAT DATE DEFAULT SYSDATE,
    UPDATEDAT DATE DEFAULT SYSDATE,
    CONSTRAINT FK_OBJECTIVE_CYCLE FOREIGN KEY (OBJECTIVECYCLEID) REFERENCES OBJECTIVECYCLES(OBJECTIVECYCLEID)
);

-- AiGeneratedMessages Table
CREATE TABLE AIGENERATEDMESSAGES (
    AIMESSAGEID NUMBER(19) DEFAULT SEQ_AIMESSAGE.NEXTVAL PRIMARY KEY,
    OBJECTIVEID NUMBER(19) NOT NULL,
    EMPLOYEEID NUMBER(10) NOT NULL,
    OBJECTIVECYCLEID NUMBER(10) NOT NULL,
    MESSAGEBODY NCLOB NOT NULL,
    ADVICEBODY NCLOB NOT NULL,
    STYLETAG NVARCHAR2(50),
    MODELNAME NVARCHAR2(50),
    GENERATEDAT DATE DEFAULT SYSDATE,
    ISACTIVE NUMBER(1) DEFAULT 1 NOT NULL,
    CONSTRAINT FK_AIMESSAGE_OBJECTIVE FOREIGN KEY (OBJECTIVEID) REFERENCES OBJECTIVES(OBJECTIVEID) ON DELETE CASCADE,
    CONSTRAINT CHK_AIMESSAGE_ACTIVE CHECK (ISACTIVE IN (0, 1))
);

-- 3. CREATE UNIQUE CONSTRAINTS
-- =====================================================

ALTER TABLE OBJECTIVECYCLES ADD CONSTRAINT UQ_OBJECTIVECYCLE_YEAR_HALF UNIQUE (YEAR, HALF);

-- 4. CREATE INDEXES FOR PERFORMANCE
-- =====================================================

-- ObjectiveCycles indexes
CREATE INDEX IX_OBJECTIVECYCLES_ACTIVE ON OBJECTIVECYCLES (ISACTIVE);

-- Objectives indexes
CREATE INDEX IX_OBJECTIVE_EMP_CYCLE ON OBJECTIVES (EMPLOYEEID, OBJECTIVECYCLEID);
CREATE INDEX IX_OBJECTIVE_CYCLE ON OBJECTIVES (OBJECTIVECYCLEID);
CREATE INDEX IX_OBJECTIVE_EMPLOYEE ON OBJECTIVES (EMPLOYEEID);

-- AiGeneratedMessages indexes
CREATE INDEX IX_AIMSG_EMP_CYCLE_ACTIVE ON AIGENERATEDMESSAGES (EMPLOYEEID, OBJECTIVECYCLEID, ISACTIVE);
CREATE INDEX IX_AIMSG_OBJ_ACTIVE ON AIGENERATEDMESSAGES (OBJECTIVEID, ISACTIVE);

-- 5. CREATE AUDIT TRIGGERS
-- =====================================================

-- Update audit columns on ObjectiveCycles
CREATE OR REPLACE TRIGGER TR_OBJECTIVECYCLES_UPDATE
    BEFORE UPDATE ON OBJECTIVECYCLES
    FOR EACH ROW
BEGIN
    :NEW.UPDATEDAT := SYSDATE;
END;
/

-- Update audit columns on Objectives
CREATE OR REPLACE TRIGGER TR_OBJECTIVES_UPDATE
    BEFORE UPDATE ON OBJECTIVES
    FOR EACH ROW
BEGIN
    :NEW.UPDATEDAT := SYSDATE;
END;
/

-- 6. SAMPLE DATA INSERTION
-- =====================================================
-- Insert sample data for testing (Employee 7426 - ماجد بن عمر بن عبدالفتاح الشيزاوي)

-- Insert ObjectiveCycle for 2025 Half 2
INSERT INTO OBJECTIVECYCLES (YEAR, HALF, STARTDATE, ENDDATE, ISACTIVE)
VALUES (2025, 2, DATE '2025-07-01', DATE '2025-12-31', 1);

-- Insert Objectives for Employee 7426
DECLARE
    v_cycle_id NUMBER;
BEGIN
    SELECT OBJECTIVECYCLEID INTO v_cycle_id 
    FROM OBJECTIVECYCLES 
    WHERE YEAR = 2025 AND HALF = 2;
    
    -- Objective 1: تطوير النظام الأساسي مع واجهة تقديم الطلبات من قبل المواطنين
    INSERT INTO OBJECTIVES (
        OBJECTIVECYCLEID, EMPLOYEEID, OBJECTIVETITLE, CLASSIFICATION, 
        RESULTDESCRIPTION, WEIGHTSCORE, HIGHLEVELGOAL
    ) VALUES (
        v_cycle_id, 7426, 
        'تطوير النظام الأساسي مع واجهة تقديم الطلبات من قبل المواطنين، وربطه بصندوق الزواج',
        'هدف يساهم في تحقيق الخطة السنوية',
        'تنفيذ وإطلاق وتفعيل منصة الزواج على مستوى سلطنة عمان',
        30.00,
        'تنفيذ وإطلاق وتفعيل منصة الزواج على مستوى سلطنة عمان'
    );
    
    -- Objective 2: إنشاء لوحة تحكم للموظفين للتعامل مع الطلبات
    INSERT INTO OBJECTIVES (
        OBJECTIVECYCLEID, EMPLOYEEID, OBJECTIVETITLE, CLASSIFICATION, 
        RESULTDESCRIPTION, WEIGHTSCORE, HIGHLEVELGOAL
    ) VALUES (
        v_cycle_id, 7426, 
        'إنشاء لوحة تحكم للموظفين للتعامل مع الطلبات',
        'هدف يساهم في تحقيق الخطة السنوية',
        'تنفيذ وإطلاق وتفعيل منصة الزواج على مستوى سلطنة عمان',
        10.00,
        'تنفيذ وإطلاق وتفعيل منصة الزواج على مستوى سلطنة عمان'
    );
    
    -- Objective 3: تصميم آلية الترشيح من قبل مديري الدوائر باستخدام تقييم بنسب للبنود
    INSERT INTO OBJECTIVES (
        OBJECTIVECYCLEID, EMPLOYEEID, OBJECTIVETITLE, CLASSIFICATION, 
        RESULTDESCRIPTION, WEIGHTSCORE, HIGHLEVELGOAL
    ) VALUES (
        v_cycle_id, 7426, 
        'تصميم آلية الترشيح من قبل مديري الدوائر باستخدام تقييم بنسب للبنود',
        'هدف يساهم في تحقيق الخطة السنوية',
        'بناء نظام إلكتروني لاختيار موظف الشهر والمبادر والمبتكر',
        5.00,
        'بناء نظام إلكتروني لاختيار موظف الشهر والمبادر والمبتكر'
    );
    
    -- Objective 4: تمكين اللجنة من ادارة الدورات
    INSERT INTO OBJECTIVES (
        OBJECTIVECYCLEID, EMPLOYEEID, OBJECTIVETITLE, CLASSIFICATION, 
        RESULTDESCRIPTION, WEIGHTSCORE, HIGHLEVELGOAL
    ) VALUES (
        v_cycle_id, 7426, 
        'تمكين اللجنة من ادارة الدورات',
        'هدف يساهم في تحقيق الخطة السنوية',
        'بناء نظام إلكتروني لاختيار موظف الشهر والمبادر والمبتكر',
        5.00,
        'بناء نظام إلكتروني لاختيار موظف الشهر والمبادر والمبتكر'
    );
    
    -- Objective 5: تمكين اللجنة من التقييم الآلي واختيار الفائزين
    INSERT INTO OBJECTIVES (
        OBJECTIVECYCLEID, EMPLOYEEID, OBJECTIVETITLE, CLASSIFICATION, 
        RESULTDESCRIPTION, WEIGHTSCORE, HIGHLEVELGOAL
    ) VALUES (
        v_cycle_id, 7426, 
        'تمكين اللجنة من التقييم الآلي واختيار الفائزين',
        'هدف يساهم في تحقيق الخطة السنوية',
        'بناء نظام إلكتروني لاختيار موظف الشهر والمبادر والمبتكر',
        5.00,
        'بناء نظام إلكتروني لاختيار موظف الشهر والمبادر والمبتكر'
    );
    
    -- Objective 6: دعم فئات إضافية (الموظف المبادر، المبتكر)
    INSERT INTO OBJECTIVES (
        OBJECTIVECYCLEID, EMPLOYEEID, OBJECTIVETITLE, CLASSIFICATION, 
        RESULTDESCRIPTION, WEIGHTSCORE, HIGHLEVELGOAL
    ) VALUES (
        v_cycle_id, 7426, 
        'دعم فئات إضافية (الموظف المبادر، المبتكر)',
        'هدف يساهم في تحقيق الخطة السنوية',
        'بناء نظام إلكتروني لاختيار موظف الشهر والمبادر والمبتكر',
        10.00,
        'بناء نظام إلكتروني لاختيار موظف الشهر والمبادر والمبتكر'
    );
    
    -- Objective 7: تقديم الدعم الفني لمنصة المعاملات الالكترونية لبلدية صحار
    INSERT INTO OBJECTIVES (
        OBJECTIVECYCLEID, EMPLOYEEID, OBJECTIVETITLE, CLASSIFICATION, 
        RESULTDESCRIPTION, WEIGHTSCORE, THRESHOLDEXCEEDS, THRESHOLDMEETS, THRESHOLDBELOW, HIGHLEVELGOAL
    ) VALUES (
        v_cycle_id, 7426, 
        'تقديم الدعم الفني لمنصة المعاملات الالكترونية لبلدية صحار',
        'هدف يساهم في تحقيق الخطة السنوية',
        'تقديم الدعم الفني للانظمة الالكترونية',
        10.00, 80.00, 60.00, 50.00,
        'تقديم الدعم الفني للانظمة الالكترونية'
    );
    
    -- Objective 8: تقديم الدعم الفني لمنصة الزواج
    INSERT INTO OBJECTIVES (
        OBJECTIVECYCLEID, EMPLOYEEID, OBJECTIVETITLE, CLASSIFICATION, 
        RESULTDESCRIPTION, WEIGHTSCORE, THRESHOLDEXCEEDS, THRESHOLDMEETS, THRESHOLDBELOW, HIGHLEVELGOAL
    ) VALUES (
        v_cycle_id, 7426, 
        'تقديم الدعم الفني لمنصة الزواج',
        'هدف يساهم في تحقيق الخطة السنوية',
        'تقديم الدعم الفني للانظمة الالكترونية',
        5.00, 80.00, 60.00, 50.00,
        'تقديم الدعم الفني للانظمة الالكترونية'
    );
    
    COMMIT;
    
    -- Display confirmation
    DBMS_OUTPUT.PUT_LINE('Successfully inserted objectives for Employee 7426 (ماجد بن عمر بن عبدالفتاح الشيزاوي)');
    DBMS_OUTPUT.PUT_LINE('Cycle: 2025 Half 2');
    DBMS_OUTPUT.PUT_LINE('Total objectives inserted: 8');
    
END;
/

-- 7. VERIFICATION QUERIES
-- =====================================================

-- Verify table creation
SELECT table_name FROM user_tables WHERE table_name IN ('OBJECTIVECYCLES', 'OBJECTIVES', 'AIGENERATEDMESSAGES');

-- Verify sequences
SELECT sequence_name FROM user_sequences WHERE sequence_name IN ('SEQ_OBJECTIVECYCLE', 'SEQ_OBJECTIVE', 'SEQ_AIMESSAGE');

-- Verify indexes
SELECT index_name FROM user_indexes WHERE table_name IN ('OBJECTIVECYCLES', 'OBJECTIVES', 'AIGENERATEDMESSAGES');

-- Verify sample data insertion
SELECT 'Cycles' AS TYPE, COUNT(*) AS COUNT FROM OBJECTIVECYCLES
UNION ALL
SELECT 'Objectives' AS TYPE, COUNT(*) AS COUNT FROM OBJECTIVES
UNION ALL
SELECT 'Messages' AS TYPE, COUNT(*) AS COUNT FROM AIGENERATEDMESSAGES;

-- Display objectives summary
SELECT 
    oc.YEAR,
    oc.HALF,
    COUNT(o.OBJECTIVEID) as TOTAL_OBJECTIVES,
    SUM(o.WEIGHTSCORE) as TOTAL_WEIGHT
FROM OBJECTIVECYCLES oc
    LEFT JOIN OBJECTIVES o ON oc.OBJECTIVECYCLEID = o.OBJECTIVECYCLEID
WHERE oc.YEAR = 2025 AND oc.HALF = 2
GROUP BY oc.YEAR, oc.HALF;

-- =====================================================
-- SCRIPT COMPLETE
-- =====================================================
-- 
-- This simplified script creates only what's needed:
-- - 3 sequences for auto-incrementing IDs
-- - 3 main tables for AI objectives system  
-- - Unique constraints and indexes for performance
-- - 2 audit triggers for UpdatedAt fields
-- - Sample data for Employee 7426 (8 objectives for 2025 Half 2)
-- 
-- Entity Framework will handle all data access operations
-- No views or stored procedures needed
-- =====================================================