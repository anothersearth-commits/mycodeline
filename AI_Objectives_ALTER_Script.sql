-- =====================================================
-- AI Objectives & Messaging System - ALTER Script
-- Add MAIN_GOAL_ID column to existing OBJECTIVES table
-- Ready to run in Toad on existing database
-- =====================================================

-- 1. ALTER EXISTING OBJECTIVES TABLE
-- =====================================================

-- Add MAIN_GOAL_ID column to existing OBJECTIVES table
ALTER TABLE OBJECTIVES ADD (
    MAIN_GOAL_ID NUMBER(10)
);

-- Update column sizes for longer Arabic text
ALTER TABLE OBJECTIVES MODIFY (
    OBJECTIVETITLE NVARCHAR2(500),
    HIGHLEVELGOAL NVARCHAR2(500)
);

-- Make MAIN_GOAL_ID NOT NULL after we populate it
-- (We'll do this after inserting data)

-- 2. CREATE ADDITIONAL INDEX
-- =====================================================

-- Add index for MAIN_GOAL_ID for better performance
CREATE INDEX IX_OBJECTIVE_MAIN_GOAL ON OBJECTIVES (MAIN_GOAL_ID);

-- 3. CLEAR EXISTING SAMPLE DATA (if any)
-- =====================================================

-- Clear any existing test data
DELETE FROM AIGENERATEDMESSAGES;
DELETE FROM OBJECTIVES;
DELETE FROM OBJECTIVECYCLES;

-- Reset sequences
DROP SEQUENCE SEQ_OBJECTIVECYCLE;
DROP SEQUENCE SEQ_OBJECTIVE;
DROP SEQUENCE SEQ_AIMESSAGE;

-- Recreate sequences
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

-- 4. INSERT FRESH SAMPLE DATA
-- =====================================================

-- Insert ObjectiveCycle for 2025 Half 2
INSERT INTO OBJECTIVECYCLES (YEAR, HALF, STARTDATE, ENDDATE, ISACTIVE)
VALUES (2025, 2, DATE '2025-07-01', DATE '2025-12-31', 1);

-- Insert Objectives for Employee 7426 with MAIN_GOAL_ID
DECLARE
    v_cycle_id NUMBER;
BEGIN
    -- Get the cycle ID
    SELECT OBJECTIVECYCLEID INTO v_cycle_id 
    FROM OBJECTIVECYCLES 
    WHERE YEAR = 2025 AND HALF = 2;
    
    -- MAIN GOAL 1: تنفيذ وإطلاق وتفعيل منصة الزواج على مستوى سلطنة عمان
    -- =============================================================
    
    -- Objective 1.1: تطوير النظام الأساسي مع واجهة تقديم الطلبات
    INSERT INTO OBJECTIVES (
        OBJECTIVECYCLEID, EMPLOYEEID, MAIN_GOAL_ID, OBJECTIVETITLE, CLASSIFICATION, 
        RESULTDESCRIPTION, WEIGHTSCORE, HIGHLEVELGOAL
    ) VALUES (
        v_cycle_id, 7426, 1,
        'تطوير النظام الأساسي مع واجهة تقديم الطلبات من قبل المواطنين، وربطه بصندوق الزواج',
        'هدف يساهم في تحقيق الخطة السنوية',
        'تطوير النظام الأساسي مع واجهة تقديم الطلبات من قبل المواطنين، وربطه بصندوق الزواج',
        30.00,
        'تنفيذ وإطلاق وتفعيل منصة الزواج على مستوى سلطنة عمان'
    );
    
    -- Objective 1.2: إنشاء لوحة تحكم للموظفين
    INSERT INTO OBJECTIVES (
        OBJECTIVECYCLEID, EMPLOYEEID, MAIN_GOAL_ID, OBJECTIVETITLE, CLASSIFICATION, 
        RESULTDESCRIPTION, WEIGHTSCORE, HIGHLEVELGOAL
    ) VALUES (
        v_cycle_id, 7426, 1,
        'إنشاء لوحة تحكم للموظفين للتعامل مع الطلبات',
        'هدف يساهم في تحقيق الخطة السنوية',
        'إنشاء لوحة تحكم للموظفين للتعامل مع الطلبات',
        10.00,
        'تنفيذ وإطلاق وتفعيل منصة الزواج على مستوى سلطنة عمان'
    );
    
    -- MAIN GOAL 2: بناء نظام إلكتروني لاختيار موظف الشهر والمبادر والمبتكر
    -- =============================================================
    
    -- Objective 2.1: تصميم آلية الترشيح
    INSERT INTO OBJECTIVES (
        OBJECTIVECYCLEID, EMPLOYEEID, MAIN_GOAL_ID, OBJECTIVETITLE, CLASSIFICATION, 
        RESULTDESCRIPTION, WEIGHTSCORE, HIGHLEVELGOAL
    ) VALUES (
        v_cycle_id, 7426, 2,
        'تصميم آلية الترشيح من قبل مديري الدوائر باستخدام تقييم بنسب للبنود',
        'هدف يساهم في تحقيق الخطة السنوية',
        'تصميم آلية الترشيح من قبل مديري الدوائر باستخدام تقييم بنسب للبنود',
        5.00,
        'بناء نظام إلكتروني لاختيار موظف الشهر والمبادر والمبتكر'
    );
    
    -- Objective 2.2: تمكين اللجنة من ادارة الدورات
    INSERT INTO OBJECTIVES (
        OBJECTIVECYCLEID, EMPLOYEEID, MAIN_GOAL_ID, OBJECTIVETITLE, CLASSIFICATION, 
        RESULTDESCRIPTION, WEIGHTSCORE, HIGHLEVELGOAL
    ) VALUES (
        v_cycle_id, 7426, 2,
        'تمكين اللجنة من ادارة الدورات',
        'هدف يساهم في تحقيق الخطة السنوية',
        'تمكين اللجنة من ادارة الدورات',
        5.00,
        'بناء نظام إلكتروني لاختيار موظف الشهر والمبادر والمبتكر'
    );
    
    -- Objective 2.3: تمكين اللجنة من التقييم الآلي
    INSERT INTO OBJECTIVES (
        OBJECTIVECYCLEID, EMPLOYEEID, MAIN_GOAL_ID, OBJECTIVETITLE, CLASSIFICATION, 
        RESULTDESCRIPTION, WEIGHTSCORE, HIGHLEVELGOAL
    ) VALUES (
        v_cycle_id, 7426, 2,
        'تمكين اللجنة من التقييم الآلي واختيار الفائزين',
        'هدف يساهم في تحقيق الخطة السنوية',
        'تمكين اللجنة من التقييم الآلي واختيار الفائزين',
        5.00,
        'بناء نظام إلكتروني لاختيار موظف الشهر والمبادر والمبتكر'
    );
    
    -- Objective 2.4: دعم فئات إضافية
    INSERT INTO OBJECTIVES (
        OBJECTIVECYCLEID, EMPLOYEEID, MAIN_GOAL_ID, OBJECTIVETITLE, CLASSIFICATION, 
        RESULTDESCRIPTION, WEIGHTSCORE, HIGHLEVELGOAL
    ) VALUES (
        v_cycle_id, 7426, 2,
        'دعم فئات إضافية (الموظف المبادر، المبتكر)',
        'هدف يساهم في تحقيق الخطة السنوية',
        'دعم فئات إضافية (الموظف المبادر، المبتكر)',
        10.00,
        'بناء نظام إلكتروني لاختيار موظف الشهر والمبادر والمبتكر'
    );
    
    -- MAIN GOAL 3: تقديم الدعم الفني للانظمة الالكترونية
    -- =============================================================
    
    -- Objective 3.1: دعم منصة المعاملات الالكترونية
    INSERT INTO OBJECTIVES (
        OBJECTIVECYCLEID, EMPLOYEEID, MAIN_GOAL_ID, OBJECTIVETITLE, CLASSIFICATION, 
        RESULTDESCRIPTION, WEIGHTSCORE, THRESHOLDEXCEEDS, THRESHOLDMEETS, THRESHOLDBELOW, HIGHLEVELGOAL
    ) VALUES (
        v_cycle_id, 7426, 3,
        'تقديم الدعم الفني لمنصة المعاملات الالكترونية لبلدية صحار',
        'هدف يساهم في تحقيق الخطة السنوية',
        'تقديم الدعم الفني لمنصة المعاملات الالكترونية لبلدية صحار',
        10.00, 80.00, 60.00, 50.00,
        'تقديم الدعم الفني للانظمة الالكترونية'
    );
    
    -- Objective 3.2: دعم منصة الزواج
    INSERT INTO OBJECTIVES (
        OBJECTIVECYCLEID, EMPLOYEEID, MAIN_GOAL_ID, OBJECTIVETITLE, CLASSIFICATION, 
        RESULTDESCRIPTION, WEIGHTSCORE, THRESHOLDEXCEEDS, THRESHOLDMEETS, THRESHOLDBELOW, HIGHLEVELGOAL
    ) VALUES (
        v_cycle_id, 7426, 3,
        'تقديم الدعم الفني لمنصة الزواج',
        'هدف يساهم في تحقيق الخطة السنوية',
        'تقديم الدعم الفني لمنصة الزواج',
        5.00, 80.00, 60.00, 50.00,
        'تقديم الدعم الفني للانظمة الالكترونية'
    );
    
    COMMIT;
    
    -- Display confirmation
    DBMS_OUTPUT.PUT_LINE('=== OBJECTIVES TABLE UPDATED SUCCESSFULLY ===');
    DBMS_OUTPUT.PUT_LINE('✓ MAIN_GOAL_ID column added');
    DBMS_OUTPUT.PUT_LINE('✓ Column sizes increased for Arabic text');
    DBMS_OUTPUT.PUT_LINE('✓ New index created for MAIN_GOAL_ID');
    DBMS_OUTPUT.PUT_LINE('✓ Sample data inserted with hierarchy');
    DBMS_OUTPUT.PUT_LINE('✓ Employee 7426: 8 objectives, 3 main goals');
    DBMS_OUTPUT.PUT_LINE('✓ Total weight: 80.00 points');
    DBMS_OUTPUT.PUT_LINE('=============================================');
    
END;
/

-- 5. NOW MAKE MAIN_GOAL_ID NOT NULL
-- =====================================================

-- After inserting data, make MAIN_GOAL_ID required
ALTER TABLE OBJECTIVES MODIFY (
    MAIN_GOAL_ID NUMBER(10) NOT NULL
);

-- 6. VERIFICATION QUERIES
-- =====================================================

-- Verify the new column exists and has data
SELECT 'COLUMN VERIFICATION' AS STATUS FROM DUAL;

-- Check table structure
SELECT column_name, data_type, data_length, nullable 
FROM user_tab_columns 
WHERE table_name = 'OBJECTIVES' 
AND column_name IN ('MAIN_GOAL_ID', 'OBJECTIVETITLE', 'HIGHLEVELGOAL')
ORDER BY column_name;

-- Verify the new index
SELECT 'INDEX VERIFICATION' AS STATUS FROM DUAL;
SELECT index_name, table_name, column_name
FROM user_ind_columns 
WHERE table_name = 'OBJECTIVES' 
AND column_name = 'MAIN_GOAL_ID';

-- Check sample data with hierarchy
SELECT 'DATA VERIFICATION' AS STATUS FROM DUAL;
SELECT 
    MAIN_GOAL_ID,
    HIGHLEVELGOAL,
    COUNT(*) AS OBJECTIVE_COUNT,
    SUM(WEIGHTSCORE) AS TOTAL_WEIGHT
FROM OBJECTIVES
GROUP BY MAIN_GOAL_ID, HIGHLEVELGOAL
ORDER BY MAIN_GOAL_ID;

-- Show detailed objectives
SELECT 'DETAILED OBJECTIVES' AS STATUS FROM DUAL;
SELECT 
    MAIN_GOAL_ID,
    OBJECTIVETITLE,
    WEIGHTSCORE,
    CASE 
        WHEN THRESHOLDEXCEEDS IS NOT NULL THEN 'With Thresholds'
        ELSE 'No Thresholds'
    END AS THRESHOLD_STATUS
FROM OBJECTIVES
ORDER BY MAIN_GOAL_ID, OBJECTIVEID;

-- Final summary
SELECT 'FINAL SUMMARY' AS STATUS FROM DUAL;
SELECT 
    COUNT(*) AS TOTAL_OBJECTIVES,
    COUNT(DISTINCT MAIN_GOAL_ID) AS MAIN_GOALS,
    SUM(WEIGHTSCORE) AS TOTAL_WEIGHT
FROM OBJECTIVES;

-- Success message
SELECT 'ALTER SCRIPT COMPLETED SUCCESSFULLY' AS FINAL_STATUS FROM DUAL;

-- =====================================================
-- ALTER SCRIPT EXECUTION COMPLETE
-- =====================================================
-- 
-- CHANGES MADE:
-- ✓ Added MAIN_GOAL_ID column to OBJECTIVES table
-- ✓ Increased OBJECTIVETITLE size to 500 characters
-- ✓ Increased HIGHLEVELGOAL size to 500 characters
-- ✓ Created index on MAIN_GOAL_ID for performance
-- ✓ Cleared old data and inserted new hierarchical data
-- ✓ Made MAIN_GOAL_ID NOT NULL after populating
-- 
-- READY FOR:
-- ✓ Entity Framework model updates
-- ✓ AI message generation with hierarchy
-- ✓ Application testing
-- 
-- Status: SUCCESS - Table structure updated
-- =====================================================