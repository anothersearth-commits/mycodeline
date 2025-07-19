-- =====================================================
-- AI Objectives Sample Data Insert Script
-- Employee ID: 7426 (ماجد بن عمر بن عبدالفتاح الشيزاوي)
-- Cycle: 2025 Half 2
-- =====================================================

-- 1. Insert ObjectiveCycle for 2025 Half 2
-- =====================================================
INSERT INTO OBJECTIVECYCLES (YEAR, HALF, STARTDATE, ENDDATE, ISACTIVE)
VALUES (2025, 2, DATE '2025-07-01', DATE '2025-12-31', 1);

-- Get the cycle ID we just inserted
DECLARE
    v_cycle_id NUMBER;
BEGIN
    SELECT OBJECTIVECYCLEID INTO v_cycle_id 
    FROM OBJECTIVECYCLES 
    WHERE YEAR = 2025 AND HALF = 2;
    
    -- 2. Insert Objectives for Employee 7426
    -- =====================================================
    
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

-- 3. Verification Query
-- =====================================================
SELECT 
    oc.YEAR,
    oc.HALF,
    COUNT(o.OBJECTIVEID) as TOTAL_OBJECTIVES,
    SUM(o.WEIGHTSCORE) as TOTAL_WEIGHT
FROM OBJECTIVECYCLES oc
    LEFT JOIN OBJECTIVES o ON oc.OBJECTIVECYCLEID = o.OBJECTIVECYCLEID
WHERE oc.YEAR = 2025 AND oc.HALF = 2
GROUP BY oc.YEAR, oc.HALF;

-- Display all objectives for Employee 7426
SELECT 
    o.OBJECTIVETITLE,
    o.CLASSIFICATION,
    o.WEIGHTSCORE,
    o.THRESHOLDEXCEEDS,
    o.THRESHOLDMEETS,
    o.THRESHOLDBELOW
FROM OBJECTIVES o
    INNER JOIN OBJECTIVECYCLES oc ON o.OBJECTIVECYCLEID = oc.OBJECTIVECYCLEID
WHERE oc.YEAR = 2025 AND oc.HALF = 2 AND o.EMPLOYEEID = 7426
ORDER BY o.OBJECTIVETITLE;

-- =====================================================
-- SCRIPT COMPLETE
-- =====================================================