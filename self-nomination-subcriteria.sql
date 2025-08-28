-- Sub-criteria insertion script for Self-Nomination Award Types
-- Run this AFTER the main migration script and after getting the Criterion IDs

-- ============================================
-- SUB-CRITERIA FOR "الموظف المبادر"
-- ============================================

-- Using Criterion IDs: 5-8 for الموظف المبادر, 9-13 for الموظف المبتكر
-- SubCriteria IDs starting from 17

-- 1. مدى ارتباط المبادرة بأهداف الوحدة (30%)
-- Sub-criterion 1.1
INSERT INTO "SubCriteria" ("SubCriteriaId", "CriterionId", "SubCriteriaCode", "Name", "MaxScore", "GradingScale")
VALUES (17, 5, '1.1', 'وضوح العلاقة بين أهداف المبادرة وأهداف الوحدة', 10, 
'[{"range":"8-10","description":"وضوح عال بين أهداف المبادرة وأهداف المحافظة الاستراتيجية/التشغيلية"},{"range":"3-7","description":"وضوح متوسط بين أهداف المبادرة وأهداف المحافظة غير شامل جميع الجوانب"},{"range":"0-2","description":"وضوح منخفض بين أهداف المبادرة وأهداف المحافظة، علاقة غير واضحة/ضعيفة"}]');

-- Sub-criterion 1.2
INSERT INTO "SubCriteria" ("SubCriteriaId", "CriterionId", "SubCriteriaCode", "Name", "MaxScore", "GradingScale")
VALUES (18, 5, '1.2', 'مرونة المبادرة في التكيف مع التغيرات داخل الوحدة', 10, 
'[{"range":"8-10","description":"المبادرة قابلة للتعديل بسهولة عند حدوث تغييرات داخلية في بيئة العمل"},{"range":"3-7","description":"المبادرة تسمح بالتعديل الجزئي مع بعض التحديات/الحاجة لوقت أطول"},{"range":"0-2","description":"المبادرة غير قابلة للتغيير/تتطلب إعادة تصميم كاملة للتكيف"}]');

-- Sub-criterion 1.3
INSERT INTO "SubCriteria" ("SubCriteriaId", "CriterionId", "SubCriteriaCode", "Name", "MaxScore", "GradingScale")
VALUES (19, 5, '1.3', 'توافق المبادرة مع السياسات والإجراءات المعمول بها في الوحدة', 10, 
'[{"range":"8-10","description":"توافق عال مع السياسات والإجراءات"},{"range":"3-7","description":"توافق جزئي مع السياسات والإجراءات ويحتاج إلى بعض المواءمات"},{"range":"0-2","description":"لا تتماشى مع السياسات والإجراءات بشكل كاف"}]');

-- 2. الأثر الإيجابي (30%)
-- Sub-criterion 2.1
INSERT INTO "SubCriteria" ("SubCriteriaId", "CriterionId", "SubCriteriaCode", "Name", "MaxScore", "GradingScale")
VALUES (20, 6, '2.1', 'تحقيق وفْر مالي أو تعظيم العائد على الاستثمار', 10, 
'[{"range":"8-10","description":"وفر مالي كبير/عائد استثماري واضح"},{"range":"3-7","description":"حقق وفر مالي/أثر استثماري جزئي غير شامل"},{"range":"0-2","description":"الوفر المالي/العائد غير واضح/محدود"}]');

-- Sub-criterion 2.2
INSERT INTO "SubCriteria" ("SubCriteriaId", "CriterionId", "SubCriteriaCode", "Name", "MaxScore", "GradingScale")
VALUES (21, 6, '2.2', 'تحسين تجربة الموظف أو العميل', 10, 
'[{"range":"8-10","description":"تحسينات واضحة وملموسة"},{"range":"3-7","description":"تحسينات جزئية/في نطاق محدود"},{"range":"0-2","description":"تحسينات طفيفة/غير واضحة"}]');

-- Sub-criterion 2.3
INSERT INTO "SubCriteria" ("SubCriteriaId", "CriterionId", "SubCriteriaCode", "Name", "MaxScore", "GradingScale")
VALUES (22, 6, '2.3', 'الاستجابة لاحتياجات وتحديات قائمة في المؤسسة', 10, 
'[{"range":"8-10","description":"المبادرة تعالج حاجة أو تحديًا أساسيًا"},{"range":"3-7","description":"المبادرة تستجيب لجزء من التحديات أو بعض الاحتياجات"},{"range":"0-2","description":"المبادرة لا تستجيب بشكل كافٍ للاحتياجات"}]');

-- 3. استدامة المبادرة (20%)
-- Sub-criterion 3.1
INSERT INTO "SubCriteria" ("SubCriteriaId", "CriterionId", "SubCriteriaCode", "Name", "MaxScore", "GradingScale")
VALUES (23, 7, '3.1', 'استمرارية أثر المبادرة', 7, 
'[{"range":"6-7","description":"المبادرة تحدث أثر إيجابي مستمر/دائم بوضوح"},{"range":"3-5","description":"المبادرة تحدث أثر إيجابي لكن لفترة قصيرة/محدودة"},{"range":"0-2","description":"المبادرة تحدث أثر ضعيف/بسيط غير دائم"}]');

-- Sub-criterion 3.2
INSERT INTO "SubCriteria" ("SubCriteriaId", "CriterionId", "SubCriteriaCode", "Name", "MaxScore", "GradingScale")
VALUES (24, 7, '3.2', 'القدرة على التوسع أو التكرار', 6, 
'[{"range":"6","description":"المبادرة يمكن تطبيقها بسهولة في وحدات أو أقسام أخرى"},{"range":"3-5","description":"المبادرة قابلة للتكرار/التوسع لكنها تتطلب تعديلات"},{"range":"0-2","description":"يصعب تكرار المبادرة في التقسيمات الأخرى"}]');

-- Sub-criterion 3.3
INSERT INTO "SubCriteria" ("SubCriteriaId", "CriterionId", "SubCriteriaCode", "Name", "MaxScore", "GradingScale")
VALUES (25, 7, '3.3', 'مخاطر التطبيق ومدى التعامل معها', 7, 
'[{"range":"6-7","description":"تم تحديد المخاطر بدقة وخطط واضحة للتخفيف"},{"range":"3-5","description":"تم رصد بعض المخاطر لكن الخطط جزئية"},{"range":"0-2","description":"لم يتم تحديد أو تحليل المخاطر بشكل كاف"}]');

-- 4. المشاركة مع الفريق (20%)
-- Sub-criterion 4.1
INSERT INTO "SubCriteria" ("SubCriteriaId", "CriterionId", "SubCriteriaCode", "Name", "MaxScore", "GradingScale")
VALUES (26, 8, '4.1', 'وضوح توزيع الأدوار والمسؤوليات', 7, 
'[{"range":"6-7","description":"الأدوار والمسؤوليات محددة بوضوح منذ البداية"},{"range":"3-5","description":"الأدوار موزعة جزئياً مع بعض الغموض"},{"range":"0-2","description":"لا يوجد وضوح في توزيع الأدوار"}]');

-- Sub-criterion 4.2
INSERT INTO "SubCriteria" ("SubCriteriaId", "CriterionId", "SubCriteriaCode", "Name", "MaxScore", "GradingScale")
VALUES (27, 8, '4.2', 'مشاركة المعلومات والمعرفة', 6, 
'[{"range":"6","description":"تبادل المعلومات والمعرفة بشكل مستمر ومنهجي"},{"range":"3-5","description":"تبادل المعلومات فقط عند الحاجة"},{"range":"0-2","description":"غياب المشاركة أو الاحتفاظ بالمعلومات"}]');

-- Sub-criterion 4.3
INSERT INTO "SubCriteria" ("SubCriteriaId", "CriterionId", "SubCriteriaCode", "Name", "MaxScore", "GradingScale")
VALUES (28, 8, '4.3', 'ثقافة المراجعة وتبادل الخبرات', 7, 
'[{"range":"6-7","description":"تبادل الخبرات والملاحظات بشكل منتظم"},{"range":"3-5","description":"استعانة بعدد بسيط من الخبرات"},{"range":"0-2","description":"غياب ثقافة التغذية الراجعة"}]');

-- ============================================
-- SUB-CRITERIA FOR "الموظف المبتكر"
-- ============================================

-- 1. مستوى الإبداع في الفكرة والتميّز عن الحلول التقليدية (30%)
-- Sub-criterion 1.1
INSERT INTO "SubCriteria" ("SubCriteriaId", "CriterionId", "SubCriteriaCode", "Name", "MaxScore", "GradingScale")
VALUES (29, 9, '1.1', 'أصالة الفكرة أو الابتكار', 10, 
'[{"range":"8-10","description":"فكرة مبتكرة وجديدة كلياً"},{"range":"3-7","description":"فكرة مطورة/محسّنة من نموذج قائم"},{"range":"0-2","description":"فكرة تقليدية/مكررة"}]');

-- Sub-criterion 1.2
INSERT INTO "SubCriteria" ("SubCriteriaId", "CriterionId", "SubCriteriaCode", "Name", "MaxScore", "GradingScale")
VALUES (30, 9, '1.2', 'تقديم قيمة مضافة حقيقية', 10, 
'[{"range":"8-10","description":"قيمة واضحة ومؤثرة وقابلة للقياس"},{"range":"3-7","description":"قيمة جزئية محدودة وقابلة للقياس"},{"range":"0-2","description":"لا توجد قيمة مضافة واضحة"}]');

-- Sub-criterion 1.3
INSERT INTO "SubCriteria" ("SubCriteriaId", "CriterionId", "SubCriteriaCode", "Name", "MaxScore", "GradingScale")
VALUES (31, 9, '1.3', 'استخدام تقنيات/أدوات جديدة', 10, 
'[{"range":"8-10","description":"استخدام فعّال ومتكامل لأدوات وتقنيات جديدة"},{"range":"3-7","description":"استخدام جزئي/محدود لأدوات جديدة"},{"range":"0-2","description":"لا يوجد استخدام لأدوات أو تقنيات جديدة"}]');

-- 2. إمكانية تطبيق الفكرة وتحقيق أثر إيجابي (25%)
-- Sub-criterion 2.1
INSERT INTO "SubCriteria" ("SubCriteriaId", "CriterionId", "SubCriteriaCode", "Name", "MaxScore", "GradingScale")
VALUES (32, 10, '2.1', 'وضوح خطة التنفيذ', 8, 
'[{"range":"7-8","description":"خطة واضحة وقابلة للتنفيذ بجدول زمني محدد"},{"range":"3-6","description":"خطة جزئية تحتاج إلى تفاصيل إضافية"},{"range":"0-2","description":"خطة غير واضحة/غير مكتملة"}]');

-- Sub-criterion 2.2
INSERT INTO "SubCriteria" ("SubCriteriaId", "CriterionId", "SubCriteriaCode", "Name", "MaxScore", "GradingScale")
VALUES (33, 10, '2.2', 'توفر الموارد المطلوبة', 9, 
'[{"range":"7-9","description":"الموارد متوفرة/يمكن توفيرها بسهولة"},{"range":"3-6","description":"بعض الموارد متوفرة وتحتاج موارد إضافية محدودة"},{"range":"0-2","description":"تتطلب موارد كبيرة غير متوفرة"}]');

-- Sub-criterion 2.3
INSERT INTO "SubCriteria" ("SubCriteriaId", "CriterionId", "SubCriteriaCode", "Name", "MaxScore", "GradingScale")
VALUES (34, 10, '2.3', 'قابلية القياس والمتابعة', 8, 
'[{"range":"7-8","description":"مؤشرات أداء واضحة وقابلة للقياس"},{"range":"3-6","description":"مؤشرات جزئية تحتاج إلى تطوير"},{"range":"0-2","description":"لا توجد مؤشرات واضحة للقياس"}]');

-- 3. مساهمة الابتكار في تقليل التكاليف أو تحسين الكفاءة (20%)
-- Sub-criterion 3.1
INSERT INTO "SubCriteria" ("SubCriteriaId", "CriterionId", "SubCriteriaCode", "Name", "MaxScore", "GradingScale")
VALUES (35, 11, '3.1', 'حجم التوفير المالي المتوقع', 7, 
'[{"range":"6-7","description":"توفير مالي كبير ومستمر"},{"range":"3-5","description":"توفير مالي متوسط"},{"range":"0-2","description":"توفير محدود أو غير واضح"}]');

-- Sub-criterion 3.2
INSERT INTO "SubCriteria" ("SubCriteriaId", "CriterionId", "SubCriteriaCode", "Name", "MaxScore", "GradingScale")
VALUES (36, 11, '3.2', 'تحسين الإنتاجية وسرعة الإنجاز', 7, 
'[{"range":"6-7","description":"تحسن كبير في الإنتاجية/تقليل الوقت بشكل ملحوظ"},{"range":"3-5","description":"تحسن متوسط في الإنتاجية"},{"range":"0-2","description":"تحسن طفيف أو غير ملموس"}]');

-- Sub-criterion 3.3
INSERT INTO "SubCriteria" ("SubCriteriaId", "CriterionId", "SubCriteriaCode", "Name", "MaxScore", "GradingScale")
VALUES (37, 11, '3.3', 'تقليل الهدر وترشيد استخدام الموارد', 6, 
'[{"range":"5-6","description":"تقليل كبير في الهدر وترشيد فعال للموارد"},{"range":"2-4","description":"تقليل متوسط في الهدر"},{"range":"0-1","description":"تأثير محدود على الهدر"}]');

-- 4. استمرارية الفكرة أو قابلية التوسع (15%)
-- Sub-criterion 4.1
INSERT INTO "SubCriteria" ("SubCriteriaId", "CriterionId", "SubCriteriaCode", "Name", "MaxScore", "GradingScale")
VALUES (38, 12, '4.1', 'قابلية التطبيق في أقسام/فروع أخرى', 5, 
'[{"range":"5","description":"يمكن تطبيقها بسهولة في جميع الأقسام/الفروع"},{"range":"2-4","description":"يمكن تطبيقها في بعض الأقسام مع تعديلات"},{"range":"0-1","description":"محدودة لقسم واحد فقط"}]');

-- Sub-criterion 4.2
INSERT INTO "SubCriteria" ("SubCriteriaId", "CriterionId", "SubCriteriaCode", "Name", "MaxScore", "GradingScale")
VALUES (39, 12, '4.2', 'القدرة على التطوير المستقبلي', 5, 
'[{"range":"5","description":"إمكانية تطوير عالية وإضافة مزايا جديدة"},{"range":"2-4","description":"إمكانية تطوير محدودة"},{"range":"0-1","description":"لا توجد إمكانية للتطوير"}]');

-- Sub-criterion 4.3
INSERT INTO "SubCriteria" ("SubCriteriaId", "CriterionId", "SubCriteriaCode", "Name", "MaxScore", "GradingScale")
VALUES (40, 12, '4.3', 'استدامة النتائج على المدى الطويل', 5, 
'[{"range":"5","description":"نتائج مستدامة ودائمة"},{"range":"2-4","description":"نتائج متوسطة المدى"},{"range":"0-1","description":"نتائج قصيرة المدى"}]');

-- 5. مدى تأثير الابتكار على المستفيدين ورضاهم (10%)
-- Sub-criterion 5.1
INSERT INTO "SubCriteria" ("SubCriteriaId", "CriterionId", "SubCriteriaCode", "Name", "MaxScore", "GradingScale")
VALUES (41, 13, '5.1', 'تحسين تجربة المستفيدين', 4, 
'[{"range":"4","description":"تحسين جذري في تجربة المستفيدين"},{"range":"2-3","description":"تحسين ملموس في بعض الجوانب"},{"range":"0-1","description":"تحسين طفيف أو غير ملموس"}]');

-- Sub-criterion 5.2
INSERT INTO "SubCriteria" ("SubCriteriaId", "CriterionId", "SubCriteriaCode", "Name", "MaxScore", "GradingScale")
VALUES (42, 13, '5.2', 'سهولة الوصول والاستخدام', 3, 
'[{"range":"3","description":"سهولة عالية في الوصول والاستخدام"},{"range":"2","description":"متوسط السهولة"},{"range":"0-1","description":"صعوبة في الوصول أو الاستخدام"}]');

-- Sub-criterion 5.3
INSERT INTO "SubCriteria" ("SubCriteriaId", "CriterionId", "SubCriteriaCode", "Name", "MaxScore", "GradingScale")
VALUES (43, 13, '5.3', 'معالجة شكاوى أو احتياجات المستفيدين', 3, 
'[{"range":"3","description":"يعالج مشاكل رئيسية للمستفيدين"},{"range":"2","description":"يعالج بعض المشاكل"},{"range":"0-1","description":"لا يعالج مشاكل محددة"}]');

-- COMMIT the transaction
COMMIT;

-- ============================================
-- SUMMARY:
-- ============================================
-- Award Types: 2 (الموظف المبادر), 3 (الموظف المبتكر)
-- Criteria IDs: 5-8 for الموظف المبادر, 9-13 for الموظف المبتكر
-- SubCriteria IDs: 17-43 (27 total subcriteria)
-- 
-- الموظف المبادر subcriteria: 17-28 (12 subcriteria)
-- الموظف المبتكر subcriteria: 29-43 (15 subcriteria)