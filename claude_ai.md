open ai key : 

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

# AI Objectives & Messaging Specification

**Purpose:** Provide an internal AI agent with all necessary context, data structures, and prompt patterns to generate *two-part* outputs for employees:

1. **💬 الرسالة (Message)** – formal, motivational, concise (≤ \~30 words), no dates.
2. **💡 النصيحة (Advice)** – a separate, actionable, short practical tip tied to the objective/result.

The system ingests *Objectives* (imported from an external source we do **not** control) and stores them in a simplified schema. The AI uses these records plus employee identity (from a read‐only view) to generate tailored messages.

---

## 1. Data Model (Final Accepted Design)

### 1.1 Objective Cycles

| Field            | Type             | Notes                                          |
| ---------------- | ---------------- | ---------------------------------------------- |
| ObjectiveCycleId | INT IDENTITY PK  | Unique cycle id                                |
| Year             | SMALLINT         | Gregorian year                                 |
| Half             | TINYINT (1 or 2) | 1 = Jan–Jun, 2 = Jul–Dec (two cycles per year) |
| StartDate        | DATE NULL        | Optional boundaries                            |
| EndDate          | DATE NULL        | Optional boundaries                            |
| IsActive         | BIT              | Marks current/open cycle                       |

**Uniqueness:** (Year, Half).

### 1.2 Objectives

One row per imported objective (no import metadata retained).

| Field             | Type                     | Notes                                            |
| ----------------- | ------------------------ | ------------------------------------------------ |
| ObjectiveId       | BIGINT IDENTITY PK       |                                                  |
| ObjectiveCycleId  | INT FK → ObjectiveCycles | Grouping by half-year                            |
| EmployeeId        | INT                      | Points to Employee view (no enforced FK if view) |
| ObjectiveTitle    | NVARCHAR(300)            | "الهدف" (concise label)                          |
| Classification    | NVARCHAR(200) NULL       | e.g. "هدف يساهم في تحقيق الخطة السنوية"          |
| ResultDescription | NVARCHAR(MAX) NULL       | Narrative / expected outcome                     |
| WeightScore       | DECIMAL(8,2) NULL        | وزن النتيجة                                      |
| ThresholdExceeds  | DECIMAL(8,2) NULL        | يفوق التوقعات                                    |
| ThresholdMeets    | DECIMAL(8,2) NULL        | يحقق التوقعات                                    |
| ThresholdBelow    | DECIMAL(8,2) NULL        | دون التوقعات                                     |
| ActualScore       | DECIMAL(8,2) NULL        | Achieved score (if available)                    |
| HighLevelGoal     | NVARCHAR(300) NULL       | Umbrella strategic goal (optional)               |
| Category          | NVARCHAR(100) NULL       | Optional tagging (Strategic/Operational…)        |

**Indexes:** (EmployeeId, ObjectiveCycleId), (ObjectiveCycleId).

### 1.3 AiGeneratedMessages

| Field            | Type                   | Notes                      |
| ---------------- | ---------------------- | -------------------------- |
| AiMessageId      | BIGINT IDENTITY PK     |                            |
| ObjectiveId      | BIGINT FK → Objectives | Source objective           |
| EmployeeId       | INT                    | Denormalized for filtering |
| ObjectiveCycleId | INT                    | Denormalized               |
| MessageBody      | NVARCHAR(MAX)          | 💬 الرسالة                 |
| AdviceBody       | NVARCHAR(MAX)          | 💡 النصيحة                 |
| StyleTag         | NVARCHAR(50) NULL      | e.g. Formal, Inspirational |
| ModelName        | NVARCHAR(50) NULL      | GPT model used             |
| GeneratedAt      | DATETIME2 (UTC)        | Timestamp                  |
| IsActive         | BIT                    | Latest active vs archived  |

**Index:** (EmployeeId, ObjectiveCycleId, IsActive).

### 1.4 Employees (Read‑Only View)

`EOM.VW_EOM_EMPLOYEES_V` supplies: EmployeeId, FirstName, LastName, Email, DepartmentId, JobTitle, HireDate, IsActive, Manager info, etc. No writes.

---

## 2. AI Generation Principles

| Aspect           | Rule                                                                                                                                      |
| ---------------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
| Tone             | Formal Arabic (فصحى بسيطة)، مشحونة بالتحفيز لكن بلا مبالغة أو لهجة عامية                                                                  |
| Structure        | Two distinct blocks: **Message** then **Advice**                                                                                          |
| Length           | Message ≤ \~30 words. Advice ≤ 2–3 short lines                                                                                            |
| Dates            | **Do not** mention dates, remaining days, deadlines unless explicitly required later                                                      |
| Institution Name | Do **not** mention institution name unless it adds tangible contextual value                                                              |
| Emojis           | Optional, at most one **in the message only**; advice normally without emoji                                                              |
| Personalization  | Always include employee full name once (or first + last), reference objective title & optionally classification or strategic significance |
| Advice Focus     | Concrete action, review step, quality check, prioritization, stakeholder empathy, simplification, risk mitigation                         |
| Safety           | Avoid promises, guarantees, sensitive personal judgments                                                                                  |

---

## 3. System → User Prompt Pattern

**System Message Template:**

```
أنت مساعد رسمي لمؤسسة حكومية. اكتب فقرتين: الأولى "رسالة" ترحيبية تحفيزية رسمية ≤ 30 كلمة، تذكر اسم الموظف وهدفه (وقد تذكر التصنيف إن أضاف قيمة). الثانية "نصيحة" عملية قصيرة مرتبطة بالهدف أو الوصف. لا تذكر تواريخ أو مدد أو أرقام أيام. لا تكرر اسم المؤسسة إلا لو أضاف قيمة. استعمل فصحى واضحة، ولا تستخدم أكثر من إيموجي واحد في الرسالة فقط. افصل بين الفقرتين بسطر فارغ.
```

**User Message Template (dynamic fields):**

```
الاسم: {FullName}
الهدف: {ObjectiveTitle}
التصنيف: {Classification}
الوصف: {Truncated(ResultDescription, ≤200 chars)}
```

(If a field missing → omit the line.)

---

## 4. Output Validation Rules

1. Split response on first blank line → `MessageBody` + `AdviceBody`.
2. Reject & regenerate if:
   - Message > 32 words.
   - Contains date patterns (YYYY, أرقام + "يوم", "متبق") unless allowed.
   - More than one emoji.
   - Institution name appears without strategic context.
3. Trim whitespace; store as UTF‑8.

---

## 5. Example Records (Source Objectives)

Simplified examples derived from imported data:

| Employee                         | ObjectiveTitle                         | Classification                                 | ResultDescription (excerpt)                                             | WeightScore | Exceeds | Meets | Below |
| -------------------------------- | -------------------------------------- | ---------------------------------------------- | ----------------------------------------------------------------------- | ----------- | ------- | ----- | ----- |
| عبد الله بن جمعة بن سيف الهنداسي | تفعيل التحول الرقمي                    | هدف يساهم في تحقيق الخطة السنوية               | تطوير وتحسين برنامج إدارة الوثائق والمراسلات لزيادة كفاءة التحول الرقمي | 14.00       | 12.00   | 9.00  | 2.00  |
| عبد الله بن جمعة بن سيف الهنداسي | زيادة الكفاءة و الترشيد الإنفاق المالي | هدف يساهم في تحقيق الخطة السنوية               | تحسين التكامل الإلكتروني بين الأنظمة لتقليل التكاليف وزيادة الكفاءة     | 8.00        | 7.00    | 5.00  | 3.00  |
| عبد الله بن جمعة بن سيف الهنداسي | تحسين إدارة صلاحيات المستخدمين         | هدف يساهم في تحقيق الخطة السنوية               | تحديث منصة تجاوب لضمان منح صلاحيات دقيقة ومتوافقة مع الهيكل المؤسسي     | 8.00        | 8.00    | 6.00  | 2.00  |
| أحمد بن محمد بن حمد العجمي       | إدارة وصيانة Active Directory          | هدف يساهم في تحقيق المهام والاختصاصات الوظيفية | إدارة حسابات المستخدمين، الصلاحيات، متابعة الـ GPO وReplication         | (NULL)      | —       | —     | —     |
| أحمد بن محمد بن حمد العجمي       | تفعيل نظام الاتصالات الموحد 3CX        | توسعة وتطوير البنية التقنية في المحافظة        | تفعيل النظام لتقييم الأداء وملاءمته وتعزيز الاتصالات                    | (NULL)      | —       | —     | —     |

---

## 6. Approved Message + Advice Examples (High Quality Library)

Use these as **few-shot** inspiration (never copy verbatim too often; paraphrase if reusing patterns):

### Example 1 (Active Directory)

**💬 الرسالة:** مرحبًا أحمد العجمي، التحكم في بيئة Active Directory لا يظهر على السطح، لكنه يشكّل العمود الفقري لكل صلاحية وانسيابية داخل المؤسسة. دورك محوري… وثقتنا بك راسخة. 🛡️

**💡 النصيحة:** احرص على مراجعة الصلاحيات القديمة دوريًا، فالحسابات المهملة أو المتروكة بصلاحيات واسعة هي الثغرات التي لا تُرى إلا عند حدوث الخلل.

### Example 2 (3CX Project)

**💬 الرسالة:** مرحبًا أحمد العجمي، مشروع 3CX ليس إعدادًا تقنيًا فحسب؛ إنه إعادة تشكيل لمسارات التواصل بين الفرق، تقودها بخطوات واثقة. 📡

**💡 النصيحة:** ابدأ باختبار رحلة مكالمة كاملة كمستخدم عادي: اتصال، تحويل، إنهاء. سجّل نقاط الاحتكاك، ثم عالج الأسهل أثرًا أولًا.

### Example 3 (منصة الزواج – مجاز مشروع وطني)

**💬 الرسالة:** مرحبًا ماجد الشيزاوي، منصة الزواج التي تعمل عليها ليست مجرد كود؛ إنها قناة تُيسّر حياة الناس وتبني ثقة رقمية.

**💡 النصيحة:** راجع تدفق الطلب من منظور مواطن أول مرة يستخدم الخدمة، واكتب ثلاث اقتراحات تبسّط اللغة أو الخطوات.

### Example 4 (Document Management / التحول الرقمي)

**💬 الرسالة:** مرحبًا عبد الله الهنداسي، تحسين إدارة الوثائق والمراسلات خطوة جوهرية لتسريع التحول الرقمي وتمتين الحوكمة.

**💡 النصيحة:** حدّد أكثر نموذج يُستخدم تكرارًا، وابدأ بأتمتة حقوله القابلة للتعبئة المسبقة لتقليل الأخطاء اليدوية.

### Example 5 (Integration Efficiency)

**💬 الرسالة:** مرحبًا عبد الله الهنداسي، كل تحسين في تكامل الأنظمة يُعيد دقائق ثمينة للفرق ويقلل كلفة التكرار.

**💡 النصيحة:** ارسم خريطة تدفق بيانات مبسطة (5 عقد فقط). أي عقدة بلا مالك واضح → عالجها بتعيين مسؤول.

### Example 6 (Access Governance)

**💬 الرسالة:** مرحبًا عبد الله الهنداسي، دقة صلاحيات منصة تجاوب هي الحارس الصامت لجودة تجربة المستخدم وثقته.

**💡 النصيحة:** اختر مجموعة مستخدمين عشوائية، وراجع تداخل الأدوار. إزالة دور واحد غير مستعمل تُخفض مخاطر مستقبلية كبيرة.

---

## 7. Few-Shot Prompt Assembly (Illustrative)

```
SYSTEM:
أنت مساعد رسمي لمؤسسة حكومية... (النص الكامل أعلاه)

USER:
الاسم: أحمد بن محمد بن حمد العجمي
الهدف: إدارة وصيانة Active Directory
التصنيف: هدف يساهم في تحقيق المهام والاختصاصات الوظيفية
الوصف: إدارة حسابات المستخدمين، الصلاحيات، متابعة الـ GPO وReplication

<EXAMPLES START>
[مثال]
💬 الرسالة:
مرحبًا أحمد العجمي، ... 🛡️

💡 النصيحة:
احرص على مراجعة الصلاحيات ...
<EXAMPLES END>
```

The agent may embed 1–2 examples (rotated) before the dynamic user block to stabilize tone.

---

## 8. Pseudocode – Generation Workflow

```
Fetch Employee (id, full name)
Fetch Objective (title, classification, resultDescription)
Assemble system + user prompt
Call OpenAI Chat (temperature = 0.7, max_tokens ~ 180)
Validate output → if invalid regenerate (max 2 attempts)
Split message / advice on first blank line
Persist AiGeneratedMessages (mark previous IsActive = 0 for same objective)
Return to UI
```

---

## 9. Regeneration Policy

| Scenario                      | Action                                                |
| ----------------------------- | ----------------------------------------------------- |
| Output merged (no blank line) | Heuristic: split on first newline after \~25–35 words |
| Too long                      | Add system reminder + regenerate                      |
| Contains date / countdown     | Add rule hint: "لا تذكر تواريخ" + regenerate          |
| Contains >1 emoji             | Strip extras or regenerate                            |

---


