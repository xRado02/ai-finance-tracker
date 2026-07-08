---
project: "AI Finance Tracker"
created: 2026-06-14
status: plan-only
source_contracts:
  - context/foundation/tech-stack.md
  - context/foundation/infrastructure.md
recommended_platform: "Azure App Service + Azure SQL Database"
deployment_type: "future educational preview"
---

# Deployment Plan

## Scope Boundary

This document is a plan only. It does not authorize or perform a deployment.

The MVP remains local-first and locally runnable. Azure App Service + Azure SQL Database is the recommended future educational preview path if the project is deployed publicly later, but cloud deployment is not required for MVP completion.

No real personal finance data may be stored in Azure during the MVP stage. Any future Azure deployment must use demo or seed data only unless the PRD is updated first.

## Current Stack

- Backend: ASP.NET Core Web API targeting .NET 9.0
- Future frontend: React
- Intended database: Microsoft SQL Server
- Auth model: default local profile, no email/password account system
- Runtime shape: request/response only
- Explicitly out of MVP scope: AI, bank integrations, realtime, WebSockets, background jobs, queues, cron, mandatory cloud deployment

## Planned Target

- Platform: Azure App Service for the ASP.NET Core Web API
- Database: Azure SQL Database for demo/preview data only
- Region: one region is enough
- Cost posture: prefer free or lowest-cost educational limits
- Access posture: human-approved setup, scoped credentials, no broad subscription-owner token for agents

## Non-Execution Notice

Do not run these actions now:

- Do not create an Azure resource group.
- Do not create an App Service plan or web app.
- Do not create an Azure SQL server or database.
- Do not configure production secrets.
- Do not deploy the application.
- Do not upload real financial data.

This plan is the audit trail for a future deployment conversation.

## Human Gates

The following decisions require explicit human approval before any command is run:

1. Confirm that a cloud preview is still needed.
2. Confirm that the preview will use demo data only.
3. Confirm the Azure subscription and budget limit.
4. Confirm the target region.
5. Confirm whether the preview should be temporary or kept for ongoing demos.
6. Confirm the exact deployment source: local CLI publish, GitHub integration, or later CI/CD workflow.
7. Confirm deletion/cleanup policy for all Azure resources created for the preview.

## Future Preparation Checklist

Before creating Azure resources, complete these local checks:

1. Verify the app builds locally:

   ```powershell
   dotnet restore .\ai-finance-tracker.csproj
   dotnet build .\ai-finance-tracker.csproj --no-restore
   ```

2. Replace the generated weather sample endpoint with real finance endpoints before any public preview.
3. Ensure local persistence is implemented and verified locally first.
4. Prepare a demo-only dataset with no real personal finance data.
5. Decide how configuration differs between local and cloud preview.
6. Document required environment variables and connection strings without committing secret values.

## Future Azure Preview Plan

These steps describe the intended order for a later, explicitly approved educational preview.

### 1. Azure Account And Budget Guardrails

- Create or choose a dedicated Azure subscription for learning/demo use.
- Set a strict budget alert before creating resources.
- Use a dedicated resource group for this project, for example `rg-ai-finance-tracker-preview`.
- Keep all preview resources in one region.

### 2. Database Preview Setup

- Create Azure SQL Database only for demo data.
- Prefer the Azure SQL free offer if it is still available and still fits the current terms.
- Configure the database to pause or stop accepting usage before unexpected billing, where available.
- Do not import local real financial data.
- Store only seed/demo transactions, categories, dashboard examples, and goal examples.

### 3. App Service Preview Setup

- Create an App Service plan suitable for learning/demo use.
- Create an App Service web app configured for the .NET runtime supported by Azure App Service at deployment time.
- Configure application settings through Azure App Service configuration, not repository files.
- Store the database connection string as an App Service setting or future secret store entry.

### 4. Deployment Method

Preferred first preview path: human-approved Azure CLI or GitHub integration, kept simple and reversible.

The future deployment commands must be checked against current Azure CLI and .NET hosting docs before execution. The exact commands are intentionally not executed as part of this plan.

Expected command families for a future run:

```powershell
az login
az group create
az appservice plan create
az webapp create
az sql server create
az sql db create
az webapp config appsettings set
dotnet publish
az webapp deploy
```

These are placeholders for planning only. A future deploy run must fill in exact names, region, SKU, runtime, and budget settings.

### 5. Preview Verification

After a future deployment, verify:

- The local app still runs without Azure.
- The cloud preview uses demo data only.
- No real financial data exists in Azure SQL.
- The API health endpoint or equivalent responds.
- Finance endpoints, once implemented, return only demo data.
- App Service logs can be read without granting destructive permissions.
- Azure SQL usage remains within the configured free/low-cost limit.

### 6. Rollback And Cleanup

- For code rollback, redeploy the previous known-good artifact or commit.
- For database changes, do not assume automatic rollback. Review migrations manually.
- For temporary preview environments, delete the whole dedicated resource group after the demo.
- A human must approve deletion, secret rotation, or database reset.

## Agent Operating Rules

- The agent may prepare documentation and local configuration examples.
- The agent may run local restore/build/test commands.
- The agent must not create Azure resources without a fresh explicit approval.
- The agent must not deploy to Azure without a fresh explicit approval.
- The agent must not handle real personal finance data in cloud workflows.
- The agent must preserve `context/` and keep PRD, tech-stack, infrastructure, and deployment plan aligned.

## Risks And Mitigations

| Risk | Impact | Mitigation |
|---|---:|---|
| Cloud preview becomes treated as MVP requirement | High | Keep this document marked plan-only and local-first; PRD remains the product contract. |
| Real financial data is uploaded to Azure | High | Use demo data only; require human confirmation before any database import. |
| Azure free limits change or are exceeded | Medium | Re-check pricing before deployment and set budget alerts first. |
| App Service runtime support differs at deployment time | Medium | Verify current Azure App Service .NET runtime support before creating the web app. |
| Broad credentials are given to an agent | High | Use scoped credentials only; destructive actions remain human-only. |
| Cloud config drifts from local behavior | Medium | Treat local run as the primary acceptance path and document environment differences. |

## Completion Criteria For This Plan

This deployment planning step is complete when:

- `context/deployment/deploy-plan.md` exists.
- The plan states that MVP remains local-first.
- The plan states that Azure is a future educational preview path.
- The plan prohibits real financial data in cloud during MVP.
- The plan states that no deploy is being executed now.
- No Azure resources have been created.
- No application features have been implemented as part of this step.
