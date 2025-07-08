Employee of the Month System
System‑Requirements Specification (SRS) & Relational Database Design

1  Purpose
To automate the end‑to‑end process of identifying, evaluating and recognising the “Employee of the Month” (EOM) for every department, while laying a foundation that can later accommodate additional programmes such as “Creative Employee” and “Initiative Employee”.

2  Scope
Internal web application, delivered on the organisation’s on‑premise network.

Technology stack: ASP.NET Core MVC 8.0 (or current LTS), SQL Server 2022, Entity Framework Core, Windows/IIS hosting.

Active Directory (AD) will provide single‑sign‑on, user identity and management hierarchy data.

Re‑use of existing HR/Attendance tables (Employees, Departments, EmployeeManagers) to avoid duplication.

3  Stakeholders & User Roles
Role	Authentication	Main Responsibilities
System Administrator	AD group EOM‑Admin	Configure award cycles, quotas, criteria; manage committee list; publish results
Department Manager	AD; must be registered as a manager in HR DB	Nominate up to department quota of employees; enter scores & notes; upload supporting document (optional)
Committee Member (8 users)	AD group EOM‑Committee	Score each nominee on the four criteria
HR Viewer / Auditor	AD group EOM‑View	Read‑only dashboards, reports
Nominee (Employee)	AD	View own nomination status & feedback

4  Functional Requirements (abbreviated)
Ref	Requirement
F‑01	The system shall open a nomination window automatically on the 1st calendar day of every month and close it at 23:59 of the 7th day (dates configurable).
F‑02	The system shall retrieve each manager’s direct reports from AD/HR data nightly and cache the hierarchy.
F‑03	A manager shall be allowed to nominate up to the pre‑defined quota for his/her department (configurable per department).
F‑04	During nomination, the manager shall assign a score (0–25, 0–30, 0–25, 0–20 respectively) for the four criteria shown on page 1 of the attached form 
 and provide a textual justification (max 500 characters) for each criterion.
F‑05	The manager may upload one optional PDF/Word file (≤ 5 MB).
F‑06	Exactly 8 committee members shall each evaluate every nominee with the same four‑criteria scoring grid.
F‑07	Final score for a nominee = (manager total + Σ committee totals) ÷ 9. System shall store both the weighted total and raw component scores.
F‑08	After evaluation closes, the system shall rank nominees by final score (desc) per department and overall. Ties are broken by highest “Quality of Service & Productivity” score, then by earliest hire date.
F‑09	Results publication triggers e‑mail/Teams notification to winners, their managers and HR.
F‑10	All create/update/delete actions are audit‑logged with AD user, timestamp and IP.

5  Non‑Functional Requirements (summary)
Category	Requirement
Security	Windows / Kerberos authentication; role‑based authorisation; HTTPS; OWASP compliance; file‑upload virus scan.
Performance	Page response ≤ 2 s at 200 concurrent users; batch score calculations run < 10 s.
Usability	Fully responsive (Bootstrap 5); bilingual UI EN/AR in future; WCAG AA accessible.
Maintainability	Clean hexagonal architecture; EF Core migrations; 80 % unit‑test coverage.
Extensibility	Additional award categories can be configured without DB redesign.
Compliance	Data retained 7 years (HR policy); stored in‑country only.

Design Notes

Normalisation: Scores are in separate detail tables to allow expandable criteria and avoid column explosion.

Weighting: Criteria.WeightPercent stores the percentages from the form (25 %, 30 %, 25 %, 20 %). These can change without code.

Computed View: Create a view vw_EOM_FinalScores that aggregates ManagerScores + average of EvaluationScores and returns final score and rank per cycle.

Extensibility: Introducing a new award is as simple as inserting into AwardTypes, defining criteria and quotas; no table changes.

Security: All FK links use the organisation’s canonical EmployeeID (already present in HR DB). AD SID or UPN is stored for committee members to remain robust if usernames change.

7  Key Use‑Case Flow (EOM cycle)
Admin creates AwardCycle → system status = “Nomination”.

Manager opens “Nominate” page → selects employees (limited by quota) → enters four scores & notes → uploads optional file → submits.

On NominationEnd, status auto‑switches to “Evaluating”; committee members receive task list.

Each Committee member evaluates every nominee (UI displays manager notes & attached file).

On EvaluationEnd, system locks edits, runs final‑score stored‑procedure, writes to FinalScores table, status = “Closed”.

Admin reviews and hits “Publish”, status = “Published”; notifications dispatched, results visible on dashboard.


title: Employee-of-the-Month (EOM) Internal System
version: 1.1
last_updated: 2025-09-28
author: Majid + ChatGPT

phases:
  # ────────────────────────────
  - id: dev-bootstrap
    name: Development Bootstrap (NOW)
    goals:
      - Ship a working EOM prototype that managers and committee can use.
    tech_stack:
      backend: ASP.NET Core MVC 8.0
      orm: Entity Framework Core 8 (code-first)
      db_provider: MySQL 8 / MariaDB 10.x
      auth:
        scheme: ASP.NET Core Identity (local username + password)
        roles:
          - EOM-Admin
          - EOM-Committee
          - EOM-View
      third_party: Bootstrap 5, FluentValidation
    db_connection:
      env_key: ConnectionStrings__MySql
      provider_package: Pomelo.EntityFrameworkCore.MySql
    extra_notes:
      - Point to the **existing Attendance DB** tables `employees` and `employee_managers` for master data; *do not* duplicate that data.
      - Seed 2 test managers + 8 test committee users in Identity tables.
      - Keep all provider-specific SQL out of migrations—let EF generate it.
  # ────────────────────────────
  
award_type_seed:
  - id: EmployeeOfMonth
    name: موظف الشهر
    criteria:
      - id: 1
        name: الالتزام والانضباط
        weight_percent: 25
        subcriteria:
          - { id: 1.1, name: الالتزام بالحضور والانصراف, max: 8 }
          - { id: 1.2, name: الانضباط في المواعيد,          max: 6 }
          - { id: 1.3, name: التمثيل المؤسسي والسلوك المهني, max: 5 }
          - { id: 1.4, name: احترام السياسات وقوانين العمل,  max: 6 }
      - id: 2
        name: جودة الخدمة والإنتاجية "رضا المستفيد"
        weight_percent: 30
        subcriteria:
          - { id: 2.1, name: دقة تنفيذ المهام ومطابقتها للمتطلبات,          max: 10 }
          - { id: 2.2, name: سرعة الإنجاز,                                max: 10 }
          - { id: 2.3, name: المساهمة في تحقيق أهداف الفريق/المؤسسة,      max: 10 }
      - id: 3
        name: التعاون والعمل الجماعي
        weight_percent: 25
        subcriteria:
          - { id: 3.1, name: الدعم والمبادرة,                max: 6 }
          - { id: 3.2, name: العمل بروح الفريق,              max: 8 }
          - { id: 3.3, name: التواصل الإيجابي,               max: 6 }
          - { id: 3.4, name: تبادل الخبرات ونقل المعرفة,      max: 5 }
      - id: 4
        name: الأداء العام
        weight_percent: 20
        subcriteria:
          - { id: 4.1, name: تحمل المسؤولية,                 max: 5 }
          - { id: 4.2, name: التعامل المهني مع المواقف الصعبة, max: 5 }
          - { id: 4.3, name: القدرة على اتخاذ القرار,        max: 5 }
          - { id: 4.4, name: رأي المسؤول المباشر,            max: 5 }

schema_new_tables:
  - AwardTypes(AwardTypeId PK, Name, Description, IsActive)
  - AwardCycles(CycleId PK, AwardTypeId FK, Month, Year,
                NominationStart, NominationEnd,
                EvaluationStart, EvaluationEnd, Status)
  - Criteria(CriterionId PK, AwardTypeId FK, Name, WeightPercent)
  - DepartmentQuotas(DepartmentId PK/FK, AwardTypeId PK/FK, Quota)
  - Nominations(NominationId PK, CycleId FK, EmployeeId FK,
                ManagerId FK, SupportingDocPath, CreatedAt)
  - ManagerScores(NominationId PK/FK, CriterionId PK/FK,
                  Score tinyint, Note nvarchar(500))
  - CommitteeMembers(MemberUserId PK, StartDate, EndDate)
  - Evaluations(EvaluationId PK, NominationId FK,
                MemberUserId FK, CreatedAt)
  - EvaluationScores(EvaluationId PK/FK, CriterionId PK/FK,
                     Score tinyint, Note nvarchar(500))

use_cases:
  - id: NominateEmployee
    actors: Manager
    flow:
      1: GET /Nominate
      2: Manager selects up to quota employees
      3: Enters 4× scores + notes, uploads optional PDF ≤ 5 MB
      4: POST → /Nominate
  - id: EvaluateNominee
    actors: CommitteeMember
    precondition: Cycle.Status == Evaluating
    flow:
      1: GET /Evaluate/{NomineeId}
      2: Member inputs 4× scores + notes
      3: POST → /Evaluate
  - id: PublishResults
    actors: EOM-Admin
    flow:
      1: Click “Publish” when all evaluations complete
      2: System calculates final_score = (MgrTotal + ΣCommitteeTotals) / 9
      3: Writes to vw_EOM_FinalScores ; sends notifications

ci_cd:
  repo: Azure DevOps
  stages:
    - build
    - test
    - deploy-dev (MySQL, local login)
    - deploy-prod (SQL Server, AD)  # gate until prod-cutover phase

open_issues:
  - Confirm bilingual UI scope (EN/AR) for MVP.
  - Finalise AD group names for role-mapping.
  - Decide document retention length >7 yrs?
  - Provide department quota source (HR vs manual).



