---
project: "AI Finance Tracker"
researched_at: "2026-06-14"
recommended_platform: "Azure App Service + Azure SQL Database"
runner_up: "Render"
context_type: mvp-future-deployment-recommendation
tech_stack:
  language: "C#"
  framework: "ASP.NET Core Web API"
  runtime: ".NET 9.0"
  database: "Microsoft SQL Server"
---

# Infrastructure Recommendation

## Recommendation

**Use Azure App Service + Azure SQL Database as the first recommended platform if the project is deployed publicly in the future.**

This is a future-facing infrastructure recommendation and an educational deployment path only. The MVP remains local-first, runs locally, and must not require cloud deployment or store real personal finance data in Azure. Azure wins because it is the strongest fit for ASP.NET Core and SQL Server, matches existing platform familiarity, supports a one-region MVP, and currently offers useful free/low-cost entry points for experimentation.

## Decision Constraints

- MVP remains local-first and locally runnable.
- Cloud deployment is optional, educational, and not required for MVP success.
- No real financial data should be stored in Azure during the MVP stage.
- The future public deployment target is request/response only: no WebSockets, persistent workers, queues, cron, or background jobs are required.
- Cost is the top priority; free limits and low idle cost are preferred.
- One region is enough.
- External providers are acceptable if needed, but SQL Server compatibility matters.

## Platform Comparison

| Platform | Runtime fit | Cost fit | CLI-first | Managed/serverless | Agent-readable docs | Stable deploy API | MCP / integration | Result |
|---|---|---:|---|---|---|---|---|---|
| Azure App Service + Azure SQL | Pass | Pass | Pass | Pass | Pass | Pass | Partial | Recommended |
| Render | Partial | Pass | Partial | Pass | Partial | Pass | Partial | Runner-up |
| Fly.io | Pass | Partial | Pass | Partial | Pass | Pass | Partial | Third |
| Railway | Pass | Partial | Pass | Pass | Pass | Pass | Pass | Not selected: paid baseline risk |
| Vercel | Fail for backend | Pass | Pass | Pass | Pass | Pass | Pass | Dropped for ASP.NET Core API |
| Netlify | Fail for backend | Pass | Pass | Pass | Pass | Pass | Pass | Dropped for ASP.NET Core API |
| Cloudflare Workers/Pages | Fail for backend | Pass | Pass | Pass | Pass | Pass | Pass | Dropped for ASP.NET Core API |

### Azure App Service + Azure SQL

Azure is the best future target for this stack because ASP.NET Core and SQL Server are first-class Microsoft workloads. App Service provides managed hosting for web/API applications, while Azure SQL Database keeps the database choice aligned with the intended SQL Server stack. The important caveat is that Azure free/shared App Service tiers are for learning and experimentation, not production SLA-backed hosting. Azure SQL's free offer is useful for preview and education, but it must be treated as demo-data-only during MVP.

### Render

Render is attractive for a low-friction educational deploy because it has a free web service tier and simple web-service primitives. It is weaker for this project because SQL Server is not a native co-located database choice, so the data layer would need a separate provider or a stack change. That makes it less faithful to the current tech-stack contract.

### Fly.io

Fly.io is a strong technical fit for containerized ASP.NET Core and has excellent CLI-driven operations. It ranks third because the project does not need persistent processes, multi-region placement, or container-level control for MVP, and the Docker packaging path is more operational work than Azure App Service for a .NET + SQL Server application.

### Dropped Platforms

Cloudflare Workers/Pages, Vercel, and Netlify remain good future candidates for a static React frontend, but they are not the recommended host for the current ASP.NET Core Web API backend. Their strongest paths are JavaScript/TypeScript serverless or edge runtimes, which do not match the backend contract without changing architecture.

## Shortlisted Platforms

### 1. Azure App Service + Azure SQL Database (Recommended)

Best alignment with ASP.NET Core, SQL Server, one-region deployment, and existing Azure familiarity. It also keeps the future deployment story close to Microsoft documentation and tooling, which is helpful for an AI-assisted project.

### 2. Render

Good low-cost PaaS option for a simple web service, but SQL Server fit is weaker. Best considered if the database decision changes later or the deployment is limited to a demo API without the final data layer.

### 3. Fly.io

Good for containerized .NET and strong CLI operations, but more infrastructure-shaped than the MVP needs. Best considered if portability and container ownership become more important than Azure familiarity.

## Anti-Bias Cross-Check: Azure App Service + Azure SQL Database

### Devil's Advocate - Weaknesses

1. Azure can quietly become more complex than the MVP needs: App Service plan, Azure SQL server, firewall rules, connection strings, managed identity, logging, and cost alerts all become decisions before the product needs production hosting.
2. Free/shared App Service tiers are explicitly for learning, experimentation, and trial use, with no SLA. Treating them as production-ready would create false confidence.
3. Azure SQL free limits can pause or throttle the database when the monthly allowance is exhausted, which can look like an application bug during testing.
4. The biggest product risk is not technical deployment failure; it is accidentally moving real personal finance data into cloud infrastructure before the PRD allows that.
5. Azure's dashboard and IAM surface can lead to over-permissioned tokens if the agent is given broad access instead of scoped deployment-only credentials.

### Pre-Mortem - How This Could Fail

Six months later, the Azure choice failed because the team treated an educational deployment path as a production commitment. The app was still meant to be local-first, but the cloud preview slowly became the default environment for manual testing. Real transactions were added to Azure SQL because it was convenient, and nobody updated the PRD or access-control assumptions. The free database allowance was exceeded during repeated tests, causing auto-pause behavior that looked like intermittent backend failures. To debug quickly, a broad Azure token was created and reused by the agent, making the environment harder to reason about and violating least-privilege discipline. Meanwhile, App Service configuration drifted from local settings, so bugs appeared only in cloud preview. The actual mistake was not choosing Azure; it was failing to preserve the boundary between local-first MVP, demo-data preview, and future public deployment.

### Unknown Unknowns

- Azure SQL's free offer is useful, but it has behavior choices when free limits are reached: auto-pause until next month or continue with paid usage.
- Free/shared App Service plans have no production SLA and should not be presented as a reliable public launch tier.
- Azure firewall and connection settings can block the app even when both services are correctly deployed.
- Local-first behavior must be tested locally first; a cloud preview can hide file path, local database, and environment differences.
- A future React frontend may be hosted separately from the API, so CORS and environment-specific API URLs need explicit handling.

## Operational Story

- **Preview deploys**: Use a demo-data-only Azure App Service deployment created from GitHub or Azure CLI. Preview environments are optional and must not become the required MVP run path.
- **Secrets**: Store connection strings and deployment credentials in Azure App Service configuration or GitHub Secrets. Agent access should use scoped credentials only; no broad subscription owner token.
- **Rollback**: Use App Service deployment slots if a paid tier is chosen later, or redeploy the last known good artifact from GitHub. Database migrations do not automatically roll back and require manual review.
- **Approval**: A human must approve any production publish, database creation, real-data import, primary secret rotation, or deletion of Azure resources.
- **Logs**: Use read-only Azure CLI access for App Service logs, for example `az webapp log tail`, and Azure portal metrics for SQL free allowance monitoring.

## Risk Register

| Risk | Source | Likelihood | Impact | Mitigation |
|---|---|---:|---:|---|
| Real financial data is accidentally stored in Azure during MVP | Devil's advocate | Medium | High | Mark cloud preview as demo-data-only and keep MVP storage local until PRD changes. |
| Free tier is mistaken for production-grade hosting | Pre-mortem | Medium | Medium | Document that free/shared tiers are educational only and require reassessment before public launch. |
| Azure SQL free allowance is exhausted and the database pauses | Unknown unknowns | Medium | Medium | Configure alerts on remaining free vCore seconds and use demo data with low test volume. |
| Agent receives over-broad Azure permissions | Devil's advocate | Medium | High | Use scoped deployment credentials; destructive actions remain human-only. |
| Cloud preview drifts from local-first behavior | Pre-mortem | Medium | Medium | Treat local run as the acceptance path and verify local persistence before cloud preview. |
| SQL Server fit is weakened if switching to Render/Fly without managed SQL Server | Research finding | Medium | Medium | Keep Azure as first recommendation unless tech-stack.md changes database choice. |

## Getting Started

These steps are for a future educational/demo deployment, not MVP completion:

1. Keep the app fully runnable locally with local data storage first.
2. Create an Azure subscription/resource group dedicated to this demo project.
3. Create an Azure App Service app for the ASP.NET Core Web API using the .NET runtime supported by App Service at deployment time.
4. Create an Azure SQL Database free-offer database for demo data only, with auto-pause on free-limit exhaustion.
5. Store the database connection string in App Service configuration, not in the repository.
6. Deploy with Azure CLI or GitHub integration after a human approves the preview environment.

## Out of Scope

The following were not evaluated or authorized by this infrastructure recommendation:

- Mandatory cloud deployment for MVP
- Storing real personal finance data in Azure during MVP
- Docker image configuration
- CI/CD pipeline setup
- Production-scale architecture, multi-region HA, disaster recovery, or SLA planning
- Bank integrations, AI features, background jobs, queues, cron, or realtime infrastructure

## Sources Checked

- Azure App Service pricing and free/shared plan notes: https://azure.microsoft.com/en-us/pricing/details/app-service/windows/
- Azure SQL Database free offer: https://learn.microsoft.com/en-us/azure/azure-sql/database/free-offer?view=azuresql
- Azure App Service ASP.NET quickstart: https://learn.microsoft.com/en-us/azure/app-service/quickstart-dotnetcore
- Render pricing and free web service tier: https://render.com/pricing
- Fly.io .NET deployment guide: https://fly.io/docs/languages-and-frameworks/dotnet/
- Railway pricing and ASP.NET Core support references: https://docs.railway.com/pricing/plans
- Vercel function runtimes: https://vercel.com/docs/functions/runtimes
- Netlify functions overview: https://docs.netlify.com/build/functions/overview/
- Cloudflare Workers docs and agent-readable docs index: https://developers.cloudflare.com/workers/platform/pricing/
