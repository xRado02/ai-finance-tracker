<!-- IMPL-REVIEW-REPORT -->
# Implementation Review: Transaction Entry And History

- **Plan**: `context/changes/transaction-entry-history/plan.md`
- **Scope**: Phases 1-4 of 4
- **Date**: 2026-07-09
- **Verdict**: APPROVED
- **Findings**: 0 critical, 2 warnings, 0 observations

## Verdicts

| Dimension | Verdict |
|-----------|---------|
| Plan Adherence | PASS |
| Scope Discipline | PASS |
| Safety & Quality | PASS |
| Architecture | WARNING |
| Pattern Consistency | PASS |
| Success Criteria | WARNING |

## Findings

### F1 - Transaction type is likely a numeric JSON enum

- **Severity**: WARNING
- **Impact**: MEDIUM - real tradeoff; pause to reason through it
- **Dimension**: Architecture
- **Location**: `Contracts/FinanceContracts.cs:7`
- **Detail**: The public DTO exposes `TransactionType` directly, and `Program.cs` does not configure a JSON string enum converter. ASP.NET Core's default `System.Text.Json` behavior serializes enums as numbers unless configured otherwise, so the wire contract is likely `0`/`1` rather than the plan language of `Income` / `Expense`. The endpoint tests post typed C# DTOs and read typed C# DTOs, so they do not catch the actual JSON representation future React code will depend on.
- **Fix A Recommended**: Configure string enum serialization for HTTP JSON and update endpoint tests to assert raw or string-based JSON for `type`.
  - Strength: Makes the public API self-describing and matches the plan wording.
  - Tradeoff: This is a contract choice; if clients already used numeric enum values, they would need adjustment.
  - Confidence: HIGH - current code has no enum converter and tests use typed DTO serialization.
  - Blind spot: I did not inspect generated OpenAPI output, which may further confirm the numeric schema.
- **Fix B**: Keep numeric enum values but document that the API contract uses numeric enum values.
  - Strength: Lowest code churn.
  - Tradeoff: Less readable for the future React client and easier to misuse.
  - Confidence: MEDIUM - acceptable only if numeric enums are a deliberate contract decision.
  - Blind spot: No frontend consumer exists yet to validate preference.
- **Decision**: FIXED via Fix A - configured string enum serialization for HTTP JSON and added test coverage for the raw `type` JSON value.

### F2 - Validation tests only assert status codes, not ProblemDetails shape

- **Severity**: WARNING
- **Impact**: LOW - quick decision; fix is obvious and narrowly scoped
- **Dimension**: Success Criteria
- **Location**: `tests/AiFinanceTracker.Tests/Endpoints/FinanceEndpointsTests.cs:103`
- **Detail**: The plan selected idiomatic `ProblemDetails` / `ValidationProblem` responses and Phase 3 asks for validation/problem responses. The tests for invalid limit, invalid request, missing category, and category/type mismatch assert only `400` or `404`, so a future change could return plain text or a different error body while the test suite still passes.
- **Fix**: Deserialize `ValidationProblemDetails` or `ProblemDetails` in the failing-path tests and assert key fields such as `status`, `errors`, and the expected error keys.
- **Decision**: PENDING
