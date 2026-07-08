---
project: "AI Finance Tracker"
context_type: greenfield
created: 2026-06-14
updated: 2026-06-14
checkpoint:
  current_phase: 8
  phases_completed: [1, 2, 3, 4, 5, 6, 7]
  gray_areas_resolved:
    - topic: "context type"
      decision: "greenfield - new project from scratch"
    - topic: "primary persona scope"
      decision: "single named user - the creator managing personal finances"
    - topic: "product insight"
      decision: "simple one-place finance management with dashboard and local data control"
    - topic: "access control"
      decision: "local profile without server authentication; full user account system outside MVP"
  frs_drafted: 10
  quality_check_status: accepted
product_type: web-app
target_scale:
  users: small
  qps: low
  data_volume: small
timeline_budget:
  mvp_weeks: 3
  hard_deadline: null
  after_hours_only: true
---

# AI Finance Tracker Shape Notes

## Seed Idea

AI Finance Tracker is a web application for personal finance management. The MVP focuses on tracking income, expenses, categories, transaction history, dashboard statistics, financial goals, and goal progress. AI analysis, AI chat, bank integrations, automatic transaction import, notifications, reminders, and mobile app support are outside the first version.

## Forward: tech-stack

- The user prefers .NET for backend, React for frontend, and Microsoft SQL Server for database.
- First version will be developed and run locally.
- Cloud deployment and AI features may be considered later.

## Vision & Problem Statement

Pain: It is hard for the user to easily control personal finances, track expenses, analyze cost structure, and monitor progress toward financial goals in one place.

Moment: The pain appears when the user wants to add an income or expense, check where money is going, review monthly financial structure, or see whether a financial goal is progressing.

Cost today: Without this application, the user must manage finances manually or in scattered tools, which makes it harder to quickly see spending structure and savings progress.

Insight: Compared with a spreadsheet, the MVP should provide a simple and fast way to manage finances in one place, with a dashboard showing the most important statistics and progress toward financial goals. The user also wants full control over personal data by running the application locally.

Scale note: The domain rule remains per user even if the product later reaches more users. The MVP does not plan shared budgets, user comparisons, or family roles.

## User & Persona

Primary persona: The creator of the application, acting as a single user managing personal finances and trying to reach a financial goal.

Context: The MVP is designed primarily for one person who wants to control their own finances, track spending, analyze cost categories, and monitor financial goal progress.

## Access Control

Single local profile without server authentication.

The MVP is a local application built primarily for the creator. It does not need full email/password registration or sign-in. One local user profile owns the user's transactions, categories, financial goals, and statistics.

Full multi-user account management is outside the MVP scope.

## Success Criteria

### Primary

- The user can run the application locally, create or select a local profile, add income and expenses, assign transactions to categories, create a financial goal, and see balance, spending structure, and goal progress on the dashboard.
- The user can review transaction history and quickly identify where the most money was spent and which expenses may be reduced.

### Secondary

- The user can quickly see which category generates the largest expenses.
- The user uses the application several times per week to monitor finances.
- The dashboard helps the user understand their financial situation faster than a spreadsheet.

### Guardrails

- All financial data is stored locally in the user's database.
- No financial data is sent to external services or cloud services as part of the MVP.
- Adding income or expense should take no more than 2 minutes.
- Every transaction must be assigned to a category.
- The dashboard should be readable and allow the user to quickly understand the financial situation.
- The application must not lose saved data during normal use.

## User Stories

### US-01: First local finance overview

- **Given** the user runs the application locally with a default local profile
- **When** the user adds income and expenses, assigns each transaction to a category, creates a financial goal, and opens the dashboard
- **Then** the user sees total income, expenses, balance, spending structure by category, and progress toward the financial goal

#### Acceptance Criteria

- The user can complete the first useful finance overview without connecting to external services.
- Each transaction included in the overview has a category.
- The dashboard shows enough information for the user to understand current finances and savings progress in one place.

## Functional Requirements

### Local Profile

- FR-001: User can use a default local profile. Priority: must-have
  > Socrates: Counter-argument considered: a separate local profile may be unnecessary for a single-user MVP. Resolution: changed to a default local profile; separate profile management is not needed in MVP.

### Transactions & Categories

- FR-002: User can add income. Priority: must-have
  > Socrates: Counter-argument considered: a starting balance might be enough if income is stable. Resolution: kept; the user wants real balance and finance history. Future recurring income or monthly salary handling may be considered later.
- FR-003: User can add expense. Priority: must-have
  > Socrates: Counter-argument considered: no strong counter-argument; without expenses the product does not solve the core problem. Resolution: kept as core functionality.
- FR-004: User can assign every transaction to a category. Priority: must-have
  > Socrates: Counter-argument considered: mandatory categories could slow down transaction entry. Resolution: kept, with a default "Other" category so adding a transaction is not blocked.
- FR-005: User can review transaction history. Priority: must-have
  > Socrates: Counter-argument considered: a dashboard with recent transactions might be enough. Resolution: kept; transaction history is key to finance analysis.

### Dashboard & Analysis

- FR-006: User can see a dashboard with total income, expenses, and balance. Priority: must-have
  > Socrates: Counter-argument considered: a list plus simple summary might be enough. Resolution: kept; the dashboard is one of the main reasons to use the application.
- FR-007: User can see spending structure by category. Priority: must-have
  > Socrates: Counter-argument considered: category structure might matter only after more transactions exist. Resolution: kept; category-based expense analysis is a core product value.
- FR-008: User can quickly check which categories had the highest spending. Priority: must-have
  > Socrates: Counter-argument considered: suggesting savings could expand into future AI functionality. Resolution: kept only as data presentation; the application shows the largest expense categories without suggesting savings and without AI.

### Financial Goals

- FR-009: User can create a financial goal with a target amount. Priority: must-have
  > Socrates: Counter-argument considered: goals could wait until expense tracking works. Resolution: kept; financial goals are one of the main functions the user wants from the start.
- FR-010: User can track progress toward a financial goal. Priority: must-have
  > Socrates: Counter-argument considered: goal progress may be misleading if it is not tied clearly to saved money. Resolution: kept; progress tracking is necessary for goals to make sense.

## Forward: technical-roadmap

- Financial charts are removed from the MVP and can be added later as an additional presentation layer.
- Recurring income or monthly salary handling may be considered after manual income entry works.

## Business Logic

The application classifies transactions into categories and calculates balance, spending structure, and financial-goal progress so the user can quickly understand their financial situation and identify the largest spending areas.

The rule consumes user-entered income, expenses, categories, and financial goals. Its outputs are the current balance, category-based spending structure, largest spending categories, and progress toward the user's financial goal.

The user must stay in control of categorization. If a transaction is assigned to the wrong category, the user can correct it; categorization must help analysis without becoming a hidden or irreversible decision.

## Non-Functional Requirements

- Adding income or expense takes no more than 2 minutes.
- Financial data remains local in the user's database and is not sent to external services.
- The dashboard displays the basic finance summary in under 2 seconds during typical local use.
- The application preserves saved data after closing and reopening.
- The interface is readable and comfortable to use on a desktop computer or laptop.
- The user can easily find transaction history and financial-goal information.

## Non-Goals

- No AI-based expense analysis in the MVP; AI features are reserved for a future version.
- No AI chat answering questions about the user's finances in the MVP.
- No bank integrations in the MVP.
- No automatic transaction import in the MVP.
- No notifications or reminders in the MVP.
- No mobile application in the MVP.
- No cloud deployment or public availability in the MVP; the first version runs locally.
- No full user account system in the MVP; the product uses a default local profile.
- No shared budgets, user comparisons, or family roles in the MVP or nearest planned stages.
- No savings recommendations in the MVP; the product presents data and largest spending categories without advising what to cut.
- Financial charts are not required for MVP success; small charts may be added later as presentation improvements.

## Quality cross-check

- Access Control: present.
- Business Logic: present.
- Project artifacts: present.
- Timeline-cost acknowledgement: present; MVP target is 3 weeks.
- Non-Goals: present.
- Preserved behavior: n/a for greenfield.
