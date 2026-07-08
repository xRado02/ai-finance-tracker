---
starter_id: dotnet
package_manager: dotnet
project_name: ai-finance-tracker
hints:
  language_family: dotnet
  team_size: solo
  deployment_target: self-host
  ci_provider: github-actions
  ci_default_flow: manual-promotion
  bootstrapper_confidence: verified
  path_taken: standard
  quality_override: false
  self_check_answers: null
  has_auth: false
  has_payments: false
  has_realtime: false
  has_ai: false
  has_background_jobs: false
---

## Why this stack

AI Finance Tracker is a solo, local-first web-app MVP with a 3-week after-hours target, no server authentication, no payments, no realtime, no AI in scope, and no background jobs. The recommended starter for this product shape in the .NET family is ASP.NET Core Web API, which clears the agent-friendly gates through strong typing, conventions, mature documentation, and broad ecosystem familiarity. Bootstrapper confidence is verified, so scaffolding should be smooth. Deployment is set to self-host because the MVP runs locally rather than in the cloud; GitHub Actions with manual promotion keeps CI focused on build and test quality. React and Microsoft SQL Server remain the intended frontend and database context for the bootstrap/configuration step.
