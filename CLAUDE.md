# Uptime Monitor

## What this is
Self-hosted uptime monitor: registers HTTP endpoints, polls them on a schedule,
stores results over time, alerts when a check fails.

## Stack
.NET 10 / ASP.NET Core · PostgreSQL via EF Core · xUnit · Docker

## How to work with me
I am building this to learn architecture, not just to ship it.

1. Explain before implementing. For anything non-trivial, propose the approach
   and the main tradeoff, then wait for my decision.
2. One feature at a time. Do not run ahead.
3. Every service or domain behaviour gets tests. Delegate to the test-engineer
   subagent rather than writing tests inline.
4. When a choice has a real alternative, tell me what it was and why you did
   not pick it.
5. After an architectural decision, remind me to run /adr.
6. Never commit secrets. Connection strings come from environment variables or
   dotnet user-secrets, never appsettings.json.

## Conventions
- ADRs in docs/adr/NNNN-title.md
- Test projects mirror source project names with a .Tests suffix
