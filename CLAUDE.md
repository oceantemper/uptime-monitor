# Uptime Monitor

## What this is
A self-hosted uptime monitor. It registers HTTP endpoints, polls them on a
schedule, stores the results over time, and raises an alert when a check fails.
A dashboard shows current status, latency and uptime percentage over a window.

## Why it exists
Two goals, in this order:

1. **Learn software architecture by building.** I want to understand the
   decisions, not just end up with working code. The reasoning matters more
   than the velocity.
2. **Portfolio piece.** This goes on my resume and gets deployed somewhere a
   hiring manager can click. It needs to be finished, documented, and live.

## Stack
- .NET 10 / ASP.NET Core (SDK 10.0.403)
- PostgreSQL via EF Core
- xUnit
- Docker for local dependencies and deployment

## Structure
Clean Architecture, four projects, dependencies point inward. See
docs/adr/0001-use-clean-architecture.md.

    src/UptimeMonitor.Domain           no references - entities, invariants
    src/UptimeMonitor.Application      -> Domain - use cases, interfaces
    src/UptimeMonitor.Infrastructure   -> Application - EF Core, HTTP clients
    src/UptimeMonitor.Web              -> Application, Infrastructure - API, DI
    tests/UptimeMonitor.Domain.Tests
    tests/UptimeMonitor.Application.Tests

Domain must never reference anything. If a change requires adding a reference
to Domain, that change is wrong.

## Roadmap
Milestones are deliberately small. Do not work ahead of the current one.

1. **Domain model.** Monitored endpoint and check result. Invariants, value
   objects, no persistence.
2. **Application layer.** Use cases (register an endpoint, record a result),
   repository interfaces defined here and implemented later.
3. **Persistence.** EF Core, PostgreSQL, migrations, repository
   implementations in Infrastructure.
4. **Polling.** A BackgroundService that checks due endpoints on a schedule.
   Overlap handling, timeouts, failure classification.
5. **API.** Register, list, and query history.
6. **Alerting.** Detect transitions to down, deliver notifications with retry
   and backoff, suppress flapping.
7. **Dashboard.** Status, latency history, uptime percentage over a window.
8. **Deployment.** Docker, config and secrets, migrations on deploy, health
   checks, CI.

## How to work with me
1. Explain before implementing. For anything non-trivial, propose the approach
   and the main tradeoff, then wait for my decision.
2. One milestone at a time. Do not run ahead.
3. Every service or domain behaviour gets tests. Delegate to the test-engineer
   subagent rather than writing tests inline.
4. When a choice has a real alternative, tell me what it was and why you did
   not pick it.
5. After an architectural decision, remind me to run /adr.
6. Never commit secrets. Connection strings come from environment variables or
   dotnet user-secrets, never appsettings.json.
7. If I ask for something that conflicts with the architecture, say so rather
   than quietly working around it.

## Conventions
- ADRs in docs/adr/NNNN-title.md, written with /adr
- Test projects mirror source project names with a .Tests suffix
- Tests named MethodName_Scenario_ExpectedOutcome
