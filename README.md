# AIMBYS Paper Generation Platform

The current `src/` tree is a Vite + React 18 + Tailwind UI/UX reference, exported
from Figma Make ([original design](https://www.figma.com/design/Al5DYMAGO5w6c2da5A2tBM/Quiz---Q-A-Platform-UI-UX)).
It is being migrated to **ASP.NET Core MVC + Bootstrap 5** (target framework
`net10.0`, SQL Server + EF Core, TinyMCE editor, provider-agnostic generation
abstraction with an OpenAI Responses-style adapter first). The React reference
will be retained until the MVC port reaches feature parity — do not delete it.

## Running the React reference

```sh
npm i
npm run dev      # vite dev server
npm run build    # vite build
```

There are no tests and no CI in this repo today.

---

## React → ASP.NET Core MVC Migration Map

This section is documentation only. No production code is changed by it. Its
purpose is to give every subsequent migration chunk an unambiguous target:
which controller, which action, which view, which view model, which auth
requirement, which forms.

### 1. Audit summary of the React UI

- **Entry**: `src/main.tsx` → `src/app/App.tsx`. The `App` component holds two
  pieces of in-memory state — `role: AppRole | null` and `view: AppView` —
  and conditionally renders one of five components: `LandingPage`,
  `SuperAdminShell`, `InstituteShell`, `TeacherShell`, `StudentShell`.
- **No router**. Despite `react-router` being a dependency, no router is wired.
  Navigation is an internal `setView('xxx')` call. The MVC port replaces this
  with real URLs.
- **Roles** (`AppRole`): `superadmin`, `institute`, `teacher`, `student`.
- **Views** (`AppView`) are prefixed by role: `sa-*`, `inst-*`, `tchr-*`,
  `std-*`, plus `landing`.
- **Exam mode**: when `examActive` is true and `view === 'std-exam'`,
  `ExamInterface` takes over the entire viewport (no shell, no sidebar).
- **Active screen tree** (the only tree to migrate):
  `src/app/components/aimbys/**`.
- **Legacy / unused** (not in scope unless explicitly reinstated by a later
  chunk):
  - `src/app/components/admin/*` (`AdminShell`, `AdminDashboard`,
    `AdminAnalytics`, `QuizManagement`, `QuizUpload`, `UserManagement`).
  - `src/app/components/AuthScreen.tsx`, `src/app/components/Onboarding.tsx`.
  - `src/app/components/figma/ImageWithFallback.tsx` (asset helper).
  - shadcn/ui primitives in `src/app/components/ui/*`.

### 2. URL strategy

Use **MVC Areas** to keep authorization, routing, and view discovery aligned to
roles:

| Area              | URL prefix      | Role required           |
| ----------------- | --------------- | ----------------------- |
| (root)            | `/`             | Anonymous (marketing/auth) |
| `Account`         | `/Account/*`    | Anonymous → authenticated |
| `SuperAdmin`      | `/SuperAdmin/*` | `SuperAdmin`            |
| `Institute`       | `/Institute/*`  | `InstituteAdmin`        |
| `Teacher`         | `/Teacher/*`    | `Teacher` or `Evaluator` |
| `Student`         | `/Student/*`    | `Student`               |

Identity roles to seed: `SuperAdmin`, `InstituteAdmin`, `Teacher`, `Evaluator`,
`Proctor`, `Student`. (`Evaluator` and `Proctor` appear in
`InstituteUsers.tsx` as separate roles.) Apply
`[Authorize(Roles = "...")]` at the controller; layer ownership checks for
teacher-owned questions, student-owned attempts, etc.

### 3. Route-by-route mapping table

`AppView` is the React state value. `Component` is the file under
`src/app/components/aimbys/`. `MVC Route` is the canonical URL. `Controller /
Action` and `View path` are the proposed MVC targets. `Auth` is the role
requirement. `Notes` lists the data the screen reads and any forms / POSTs
discovered in the React source.

#### 3.1 Marketing & authentication (Anonymous)

| AppView   | Component        | MVC Route             | Controller / Action            | View path                              | Auth | Notes |
| --------- | ---------------- | --------------------- | ------------------------------ | -------------------------------------- | ---- | ----- |
| `landing` | `LandingPage`    | `GET /`               | `HomeController.Index`         | `Views/Home/Index.cshtml`              | Anon | Hero, stats, feature list, ISO/CERT-In/TLS footer. Read-only marketing copy. |
| `landing` | `LandingPage` (login form half) | `GET /Account/Login`  | `AccountController.Login`      | `Views/Account/Login.cshtml`           | Anon | Role selector + username + password + "remember device". Submit → `POST /Account/Login` (anti-forgery). On success, redirect to role-specific dashboard. |
| —         | `LandingPage` (login form half) | `POST /Account/Login` | `AccountController.Login`      | redirects                              | Anon | `[ValidateAntiForgeryToken]`. ASP.NET Identity sign-in. Server-side rate limiting + audit log entry per attempt. |
| —         | (new)            | `GET /Account/ForgotPassword` | `AccountController.ForgotPassword` | `Views/Account/ForgotPassword.cshtml` | Anon | Stub. React has a "Forgot password?" link with no handler; reproduce as a placeholder page in chunk 1. |
| —         | (n/a)            | `POST /Account/Logout` | `AccountController.Logout`    | redirects to `/`                       | User | Anti-forgery. Logs audit event. Replaces every shell's sidebar `LogOut` button. |

#### 3.2 Super Admin (`Roles = "SuperAdmin"`, area `SuperAdmin`)

| AppView          | Component              | MVC Route                       | Controller / Action                     | View path                                              | Notes (data + forms) |
| ---------------- | ---------------------- | ------------------------------- | --------------------------------------- | ------------------------------------------------------ | -------------------- |
| `sa-dashboard`   | `SuperAdminDashboard`  | `GET /SuperAdmin`               | `DashboardController.Index`             | `Areas/SuperAdmin/Views/Dashboard/Index.cshtml`        | KPI tiles, platform-activity area chart (24h), regional table, license pie, service status, alerts feed, pending-approvals table with `Approve` / `Review` row actions → `POST /SuperAdmin/Approvals/Approve/{id}`. |
| `sa-institutes`  | `InstituteManagement`  | `GET /SuperAdmin/Institutes`    | `InstitutesController.Index`            | `Areas/SuperAdmin/Views/Institutes/Index.cshtml`       | Search + status + type filters, paginated list. Row actions: view (`GET /SuperAdmin/Institutes/Details/{id}`), edit (`GET /SuperAdmin/Institutes/Edit/{id}` + `POST`), more (suspend, change-license). Toolbar: `Onboard Institute` (`GET /SuperAdmin/Institutes/Create` + `POST`), `Export` (`GET /SuperAdmin/Institutes/Export`). |
| `sa-analytics`   | `GlobalAnalytics`      | `GET /SuperAdmin/Analytics`     | `AnalyticsController.Index`             | `Areas/SuperAdmin/Views/Analytics/Index.cshtml`        | Monthly trend, subject breakdown, question-type radar, pass/fail trend, KPI strip. `Export Report` button → `GET /SuperAdmin/Analytics/Export?format=...`. |
| `sa-licenses`    | (placeholder in shell) | `GET /SuperAdmin/Licenses`      | `LicensesController.Index`              | `Areas/SuperAdmin/Views/Licenses/Index.cshtml`         | Stub for chunk 8 — React shell renders fallback dashboard for this view. |
| `sa-security`    | (placeholder in shell) | `GET /SuperAdmin/Security`      | `SecurityController.Index`              | `Areas/SuperAdmin/Views/Security/Index.cshtml`         | Same — stub. Chunk 8. |
| `sa-system`      | `SystemHealth`         | `GET /SuperAdmin/System`        | `SystemHealthController.Index`          | `Areas/SuperAdmin/Views/SystemHealth/Index.cshtml`     | Infra metrics tiles, CPU/memory + req/error charts (24h), service registry table. Read-only. |
| `sa-audit`       | `AuditLogs`            | `GET /SuperAdmin/Audit`         | `AuditController.Index`                 | `Areas/SuperAdmin/Views/Audit/Index.cshtml`            | Severity + module + free-text filter; paginated table. `Export Logs` → `GET /SuperAdmin/Audit/Export`. Read-only (audit rows are append-only). |
| `sa-broadcasts`  | (placeholder in shell) | `GET /SuperAdmin/Broadcasts`    | `BroadcastsController.Index` / `Create` | `Areas/SuperAdmin/Views/Broadcasts/Index.cshtml`       | Stub. Chunk 8. POST will be `POST /SuperAdmin/Broadcasts/Create` with anti-forgery. |
| —                | derived from dashboard | `POST /SuperAdmin/Approvals/Approve/{id}` / `Reject/{id}` | `ApprovalsController.Approve` / `Reject` | redirects                                            | Anti-forgery; ownership check (the request must belong to a tenant the SuperAdmin governs). |

#### 3.3 Institute Admin (`Roles = "InstituteAdmin"`, area `Institute`)

| AppView              | Component             | MVC Route                              | Controller / Action                | View path                                                | Notes |
| -------------------- | --------------------- | -------------------------------------- | ---------------------------------- | -------------------------------------------------------- | ----- |
| `inst-dashboard`     | `InstituteDashboard`  | `GET /Institute`                       | `DashboardController.Index`        | `Areas/Institute/Views/Dashboard/Index.cshtml`           | KPI tiles, weekly activity area chart, alerts feed, subject performance bars, top contributors table. |
| `inst-users`         | `InstituteUsers`      | `GET /Institute/Users`                 | `UsersController.Index`            | `Areas/Institute/Views/Users/Index.cshtml`               | Role + status + search filters, paginated table. Row actions: view, edit, more. Toolbar `Invite User` → `GET /Institute/Users/Invite` + `POST` (anti-forgery, server validation: email, role, department). |
| `inst-papers`        | (placeholder in shell)| `GET /Institute/Papers`                | `PapersController.Index`           | `Areas/Institute/Views/Papers/Index.cshtml`              | Stub for chunk 4. List of all papers across teachers in the institute with approve/return workflow. |
| `inst-questionbank`  | `QuestionBank`        | `GET /Institute/Questions`             | `QuestionsController.Index`        | `Areas/Institute/Views/Questions/Index.cshtml`           | Type/subject/difficulty filters; cards. Row actions: view (`Details/{id}`), edit (`Edit/{id}` + POST), delete (`POST Delete/{id}`). Toolbar `Add Question` → `Create` (TinyMCE-backed); `Import from Excel` → `POST /Institute/Questions/Import` (`<form enctype="multipart/form-data">`, anti-forgery, file ≤ 10 MB, server validates schema). |
| `inst-calendar`      | `ExamCalendarPage` (inline in shell) | `GET /Institute/Calendar`     | `CalendarController.Index`         | `Areas/Institute/Views/Calendar/Index.cshtml`            | Read-only table; later chunks may add `POST Calendar/Schedule`. |
| `inst-approvals`     | (placeholder in shell)| `GET /Institute/Approvals`             | `ApprovalsController.Index`        | `Areas/Institute/Views/Approvals/Index.cshtml`           | Stub. Workflow inbox for paper approvals from teachers. POSTs: `Approve/{id}`, `Reject/{id}` (anti-forgery, requires comment on reject). |
| `inst-analytics`     | `InstituteAnalytics`  | `GET /Institute/Analytics`             | `AnalyticsController.Index`        | `Areas/Institute/Views/Analytics/Index.cshtml`           | Monthly trend, class-wise bars, subject trend lines, summary table. `Export Report` → `GET /Institute/Analytics/Export`. |
| `inst-settings`      | (placeholder in shell)| `GET /Institute/Settings`              | `SettingsController.Index`         | `Areas/Institute/Views/Settings/Index.cshtml`            | Stub. Branding, academic calendar, integrations. POST per section. |

#### 3.4 Teacher / Examiner (`Roles = "Teacher,Evaluator"`, area `Teacher`)

| AppView              | Component                      | MVC Route                              | Controller / Action                  | View path                                                  | Notes |
| -------------------- | ------------------------------ | -------------------------------------- | ------------------------------------ | ---------------------------------------------------------- | ----- |
| `tchr-dashboard`     | `TeacherDashboard`             | `GET /Teacher`                         | `DashboardController.Index`          | `Areas/Teacher/Views/Dashboard/Index.cshtml`               | KPI tiles, class-avg bar chart, pending evaluations list (deep-link to `/Teacher/Evaluation/{submissionId}`), recent papers table. |
| `tchr-papergen`      | `PaperGeneration`              | `GET /Teacher/Papers`                  | `PapersController.Index`             | `Areas/Teacher/Views/Papers/Index.cshtml`                  | List of teacher's papers. |
| `tchr-papergen` (editor) | `PaperGeneration`          | `GET /Teacher/Papers/Edit/{id}`        | `PapersController.Edit`              | `Areas/Teacher/Views/Papers/Edit.cshtml`                   | Two-pane editor: bank browser (search + multi-select) + paper editor (drag-reorder, remove, totals). Buttons: `Preview` (`GET .../Preview/{id}`), `Save Draft` (`POST /Teacher/Papers/SaveDraft` — anti-forgery, JSON of selected QIDs + ordering + title), `Submit for Approval` (`POST /Teacher/Papers/Submit/{id}`). Title edit is part of the same form. |
| `tchr-blueprint`     | `PaperGeneration` (blueprint tab) | `GET /Teacher/Papers/Blueprint/{id}` | `PapersController.Blueprint`         | `Areas/Teacher/Views/Papers/Blueprint.cshtml`              | Total marks, duration, type-mix, difficulty-mix form. Submit → `POST /Teacher/Papers/GenerateFromBlueprint/{id}` (calls the provider-agnostic generator service). |
| `tchr-questionbank`  | `TeacherQuestionBank`          | `GET /Teacher/Questions`               | `QuestionsController.Index`          | `Areas/Teacher/Views/Questions/Index.cshtml`               | "My Questions" view. CRUD scoped by ownership: server enforces `q.AuthorId == currentUserId` on every Edit/Delete. TinyMCE on Create/Edit. |
| `tchr-evaluation`    | `EvaluationDesk`               | `GET /Teacher/Evaluation`              | `EvaluationController.Index`         | `Areas/Teacher/Views/Evaluation/Index.cshtml`              | Inbox of submissions assigned to this evaluator. |
| `tchr-evaluation` (open submission) | `EvaluationDesk` | `GET /Teacher/Evaluation/{submissionId}` | `EvaluationController.Open`         | `Areas/Teacher/Views/Evaluation/Open.cshtml`               | Split view: student answer + rubric scoring + feedback textarea. Each rubric save: `POST /Teacher/Evaluation/SaveScore` (anti-forgery, JSON: `{submissionId, answerIdx, rubricIdx, points, feedback}`) — autosave per criterion. Final: `POST /Teacher/Evaluation/Submit/{submissionId}`; redirects to inbox. |
| `tchr-reports`       | `TeacherReports` (inline)      | `GET /Teacher/Reports`                 | `ReportsController.Index`            | `Areas/Teacher/Views/Reports/Index.cshtml`                 | Read-only class summary table. |
| `tchr-ide`           | `CodingIDEPage` (inline)       | `GET /Teacher/CodingIDE`               | `CodingIdeController.Index`          | `Areas/Teacher/Views/CodingIde/Index.cshtml`               | Editor + test runner UI. POSTs deferred to a later chunk: `Run` → `POST /Teacher/CodingIDE/Run` (returns test results JSON), `Save to Bank` → `POST /Teacher/CodingIDE/Save` (creates a Coding question owned by current user). Anti-forgery on both. |

#### 3.5 Student / Candidate (`Roles = "Student"`, area `Student`)

| AppView           | Component                    | MVC Route                                                    | Controller / Action                  | View path                                              | Notes |
| ----------------- | ---------------------------- | ------------------------------------------------------------ | ------------------------------------ | ------------------------------------------------------ | ----- |
| `std-dashboard`   | `StudentDashboard`           | `GET /Student`                                               | `DashboardController.Index`          | `Areas/Student/Views/Dashboard/Index.cshtml`           | KPI tiles, next-exam banner with "Start Exam Now" CTA, recent results, subject progress bars. CTA links to `/Student/Exams/Start/{examId}` (POST). |
| `std-exam`        | `ExamLobby` (inline)         | `GET /Student/Exams`                                         | `ExamsController.Index`              | `Areas/Student/Views/Exams/Index.cshtml`               | Upcoming + completed lists. |
| `std-exam` (active) | `ExamInterface`            | `GET /Student/Exams/Take/{attemptId}`                        | `ExamsController.Take`               | `Areas/Student/Views/Exams/Take.cshtml`                | Full-bleed layout (no role shell). Strict auth: attempt must belong to current user and be in window. Form POSTs (all anti-forgery): `POST /Student/Exams/Start/{examId}` → creates attempt and redirects to `Take/{attemptId}`. `POST /Student/Exams/SaveAnswer` (autosave per question — debounced 2s) `{attemptId, questionId, response}`. `POST /Student/Exams/Flag` `{attemptId, questionId, flagged}`. `POST /Student/Exams/Submit/{attemptId}` → finalises, redirects to results. Server enforces server-side timer (do not trust client clock). |
| `std-results`     | `StudentResults`             | `GET /Student/Results`                                       | `ResultsController.Index`            | `Areas/Student/Views/Results/Index.cshtml`             | Latest result card + topic breakdown + all-results table. Read-only; `[Authorize]` + ownership check (`attempt.StudentId == currentUserId`). |
| `std-results` (one) | `StudentResults`           | `GET /Student/Results/{attemptId}`                           | `ResultsController.Details`          | `Areas/Student/Views/Results/Details.cshtml`           | Same data with deep link. |
| `std-analytics`   | `StudentAnalytics` (inline)  | `GET /Student/Analytics`                                     | `AnalyticsController.Index`          | `Areas/Student/Views/Analytics/Index.cshtml`           | Subject score trend bars + KPI cards. Read-only. |
| `std-leaderboard` | (placeholder in shell)       | `GET /Student/Leaderboard`                                   | `LeaderboardController.Index`        | `Areas/Student/Views/Leaderboard/Index.cshtml`         | Stub; React shell renders fallback dashboard for this view. |

### 4. Shared layout mapping

The React app does **not** share a `_Layout` between roles — every shell is its
own full-page component. That duplication should be collapsed in MVC:

| React element                                   | MVC equivalent                                               |
| ----------------------------------------------- | ------------------------------------------------------------ |
| Top-level theme wrapper (`dark` class on root)  | Root `Views/Shared/_Layout.cshtml` with theme cookie + `data-bs-theme` on `<body>`. |
| `LandingPage` full-bleed split layout           | `Views/Shared/_LayoutAnonymous.cshtml` (used by `Home`, `Account`). |
| Each role `Shell` (sidebar + topbar + main + mobile bottom nav) | Per-area `Areas/<Role>/Views/Shared/_RoleLayout.cshtml`. Each `_RoleLayout` `@RenderSection`s the page body and pulls common bits via partials. |
| Sidebar `NAV` array + brand block + user card   | `_Sidebar.cshtml` partial, populated via a `RoleNavViewComponent` that returns the nav model for the current role; active link computed from `ViewContext.RouteData`. |
| Top bar (search, dark toggle, bell, role badge) | `_TopBar.cshtml` partial + `NotificationsViewComponent` (badge count) + `UserMenuViewComponent`. Search box posts to a global `SearchController.Suggest` JSON endpoint. |
| Mobile bottom nav (`<nav class="lg:hidden ...">`) | `_MobileBottomNav.cshtml` partial, same nav model as sidebar, surfaced under `d-lg-none` Bootstrap classes. |
| `ExamInterface` header + sidebar (no shell)     | Distinct `_LayoutExam.cshtml`: minimal head, no role shell, includes server-driven countdown timer and a hidden anti-forgery form for the autosave endpoint. |

Reusable view components / partials to extract:

- `KpiCardViewComponent(label, value, sub, trend, iconKey, accentKey)`.
- `ChartCardViewComponent(title, dataUrl, type)` — chart bodies render
  client-side from a JSON endpoint; **Chart.js** replaces Recharts under the
  Bootstrap-only constraint.
- `DataTablePartial` (`_DataTable.cshtml`) for the search + filter + paginated
  table pattern that recurs in `InstituteManagement`, `InstituteUsers`,
  `QuestionBank`, `AuditLogs`, etc.
- `FilterBarPartial` (search input + selects).
- `PaginationPartial` (server-side pagination, query-string driven).
- `RoleBadgePartial`.

### 5. Forms, validation, anti-forgery, and POST endpoints

Every form below MUST include `@Html.AntiForgeryToken()` (or the implicit
`asp-antiforgery` from the tag helper) and the action MUST be decorated with
`[ValidateAntiForgeryToken]`. `[AutoValidateAntiforgeryToken]` is recommended
as a global filter so a missed attribute doesn't silently disable protection.
Validation lives in the view-model with DataAnnotations and is mirrored
client-side via `jquery-validation-unobtrusive`.

| Form / action                                  | Method + URL                                              | View model fields (proposed)                                                                 | Auth                  |
| ---------------------------------------------- | --------------------------------------------------------- | -------------------------------------------------------------------------------------------- | --------------------- |
| Login                                          | `POST /Account/Login`                                     | `Username (Required, MaxLen)`, `Password (Required)`, `Role`, `RememberDevice (bool)`, `ReturnUrl` | Anon, rate-limited    |
| Logout                                         | `POST /Account/Logout`                                    | (empty)                                                                                      | User                  |
| Onboard institute                              | `POST /SuperAdmin/Institutes/Create`                      | `Name`, `Type`, `City`, `State`, `LicenseTier`, `AdminEmail` (`EmailAddress`, required)      | SuperAdmin            |
| Edit / suspend institute                       | `POST /SuperAdmin/Institutes/Edit/{id}` / `Suspend/{id}`  | id-bound, plus reason on suspend                                                             | SuperAdmin            |
| Approve / reject platform request              | `POST /SuperAdmin/Approvals/Approve/{id}` / `Reject/{id}` | `RequestId`, optional `Comment`                                                              | SuperAdmin            |
| Broadcast message                              | `POST /SuperAdmin/Broadcasts/Create`                      | `Subject`, `Body (HTML, sanitized)`, `Audience[]`, `ScheduledAt?`                            | SuperAdmin            |
| Invite user                                    | `POST /Institute/Users/Invite`                            | `Name`, `Email (EmailAddress)`, `Role`, `Department`                                         | InstituteAdmin        |
| Update user role / status                      | `POST /Institute/Users/Update/{id}`                       | `Role`, `Status`                                                                             | InstituteAdmin        |
| Approve / reject paper                         | `POST /Institute/Approvals/Approve/{id}` / `Reject/{id}`  | `PaperId`, `Comment` (required on reject)                                                    | InstituteAdmin        |
| Create / edit question                         | `POST /Teacher/Questions/Create` / `Edit/{id}`            | `Type`, `Subject`, `Topic`, `Class`, `Difficulty`, `Bloom`, `Marks`, `BodyHtml` (TinyMCE, sanitized via HtmlSanitizer), `Choices[]` (MCQ), `CorrectAnswer`, `RubricCriteria[]` (Descriptive), `TestCases[]` (Coding) | Teacher (own)         |
| Delete question                                | `POST /Teacher/Questions/Delete/{id}`                     | id-bound; ownership check                                                                    | Teacher (own)         |
| Excel import                                   | `POST /Institute/Questions/Import`                        | `IFormFile File` (`.xlsx` only, ≤ 10 MB, MIME + extension validated)                         | InstituteAdmin        |
| Save paper draft                               | `POST /Teacher/Papers/SaveDraft`                          | `PaperId?`, `Title`, `QuestionIds[]` (ordered), `BlueprintId?`                               | Teacher (own paper)   |
| Submit paper for approval                      | `POST /Teacher/Papers/Submit/{id}`                        | `PaperId`                                                                                    | Teacher (own paper)   |
| Generate from blueprint                        | `POST /Teacher/Papers/GenerateFromBlueprint/{id}`         | `TotalMarks`, `DurationMinutes`, `TypeMix{ }`, `DifficultyMix{ }`, `Subjects[]`              | Teacher (own paper)   |
| Save evaluation rubric score (autosave)        | `POST /Teacher/Evaluation/SaveScore`                      | `SubmissionId`, `AnswerIndex`, `RubricIndex`, `Points`, `Feedback?`                           | Evaluator (assigned)  |
| Submit evaluation                              | `POST /Teacher/Evaluation/Submit/{submissionId}`          | `SubmissionId`                                                                                | Evaluator (assigned)  |
| Coding IDE: run code                           | `POST /Teacher/CodingIDE/Run`                             | `Language`, `Source`, `TestCases[]`                                                          | Teacher               |
| Coding IDE: save question                      | `POST /Teacher/CodingIDE/Save`                            | full Coding question payload                                                                 | Teacher               |
| Start exam attempt                             | `POST /Student/Exams/Start/{examId}`                      | `ExamId`                                                                                     | Student (eligible)    |
| Save answer (autosave)                         | `POST /Student/Exams/SaveAnswer`                          | `AttemptId`, `QuestionId`, `Response`                                                        | Student (own attempt) |
| Flag question                                  | `POST /Student/Exams/Flag`                                | `AttemptId`, `QuestionId`, `Flagged (bool)`                                                  | Student (own attempt) |
| Submit attempt                                 | `POST /Student/Exams/Submit/{attemptId}`                  | `AttemptId`                                                                                  | Student (own attempt) |
| Theme / dark-mode toggle                       | `POST /Profile/Theme`                                     | `Theme ("light"|"dark")`                                                                     | User                  |
| Export endpoints (institutes, audit, reports)  | `GET /<Area>/<Resource>/Export?format=csv|xlsx&...`       | filter query string                                                                          | role-gated            |

### 6. Recommended conversion order

This sequencing matches the upcoming chunk plan. Each step builds on the
previous, and each can ship independently behind feature flags / route
authorization.

1. **Solution scaffold + `global.json` pin to `net10.0`** (no UI yet).
2. **Identity + Account area** — login, logout, forgot-password stub,
   redirect-after-login by role. Replaces `LandingPage` login half.
3. **Marketing home** at `/` — replaces `LandingPage` left panel.
4. **Shared layouts + role shells + read-only dashboards** — `_Layout`,
   `_LayoutAnonymous`, `_RoleLayout` per area, partials, view components,
   four dashboard pages with seed data.
5. **Question Bank** — institute-wide and teacher "My Questions". TinyMCE
   integrated for Create/Edit. Excel import endpoint.
6. **Paper Generation** — list, two-pane editor, blueprint, preview.
   Introduce `IPaperGenerationProvider` + `OpenAIResponsesProvider` as the
   first adapter.
7. **Exam runtime** — lobby, attempt page with server-driven timer, autosave,
   flag, submit. Integrity (no shell, anti-cheat hooks left as stubs).
8. **Evaluation Desk** — inbox + per-submission scoring with rubric autosave.
9. **Results + analytics** — student results, institute analytics, global
   analytics. Chart.js + JSON endpoints.
10. **Admin tooling** — institute management, user management, approvals
    workflows, audit log viewer, system health, broadcasts, licenses,
    security monitor.
11. **Hardening** — CSP, output caching where safe, full anti-forgery filter,
    rate-limit policies on auth and export endpoints.

### 7. Definition of Done (per screen)

A migrated screen is "done" only when **all** of these hold. This is the
checklist every chunk PR must self-attest.

- [ ] Route registered, reachable from the corresponding sidebar / nav link.
- [ ] Controller has `[Authorize(Roles = "...")]`; ownership / tenancy checked
      where required (e.g. teacher's questions, student's attempt, institute
      scope).
- [ ] View renders against a dedicated ViewModel (no EF entities leaked into
      the view).
- [ ] All forms include `@Html.AntiForgeryToken()`; all POST actions have
      `[ValidateAntiForgeryToken]`. POST endpoints reject `GET`.
- [ ] Server-side validation via DataAnnotations on the ViewModel; client-side
      validation enabled (`jquery-validation-unobtrusive`).
- [ ] User-supplied HTML (TinyMCE bodies, feedback) is sanitized on the
      server before persistence.
- [ ] Bootstrap 5 responsive: layout verified at sm / md / lg / xl
      breakpoints; mobile bottom nav works for role pages; tables wrap or
      scroll, never overflow.
- [ ] Empty state, loading state, and error state are visibly designed (not
      blank). 404 / 403 routes through shared error view.
- [ ] Accessibility: form fields have `<label>`, interactive icons have
      `aria-label`, modal/dropdown traps focus, color contrast ≥ AA.
- [ ] No secrets, connection strings, or API keys committed; configuration is
      read from `IConfiguration` / user-secrets.
- [ ] Audit log entry written for every state-changing action (login,
      paper publish, evaluation submit, institute suspend, etc.).
- [ ] Existing build passes (`dotnet build`) and any tests added for the
      chunk pass (`dotnet test`).

### 8. Open questions

These are intentionally left for the relevant chunks; the safest defaults
above are picked for now so migration is unblocked.

1. Should `Evaluator` be a distinct Identity role with its own area, or is it
   a permission flag on the `Teacher` role? Defaulting to: same area
   (`/Teacher/Evaluation`), separate role name for `[Authorize]`.
2. Should marketing (`/`) and login (`/Account/Login`) be the same page
   (matching the React `LandingPage` 50/50 split) or separate URLs?
   Defaulting to: separate pages, same anonymous layout, "Sign in" CTA on
   `/` links to `/Account/Login`.
3. Are tenancy boundaries strict (an InstituteAdmin can never see another
   institute's data, ever) or are there shared "platform-level" reads?
   Defaulting to: strict tenancy enforced by an `IInstituteScope` filter on
   every query.
