# 0001. Use Clean Architecture for project layout

Date: 2026-11-09
Status: Accepted

## Context

Starting a self-hosted uptime monitor in .NET 10. The app is small at the
outset: two entities (a monitored endpoint and a check result), one background
poller, one read-oriented dashboard.

A primary goal of this project is learning software architecture, not only
shipping a working tool. That goal weighs against choosing purely by what is
fastest to stand up.

## Options considered

**Single project.** All controllers, services, EF context and models in one
assembly. Fastest to start and no ceremony. Nothing enforces dependency
direction, so the design degrades quietly as the app grows, and it teaches
nothing about boundaries.

**Clean Architecture.** Four projects - Domain, Application, Infrastructure,
Web - with dependencies pointing inward and Domain depending on nothing.
Boundaries are enforced by the compiler rather than by discipline. Cost is
real ceremony: four projects and a set of interfaces for an app with two
entities.

**Vertical slices.** Organize by feature rather than by layer, each slice
holding its own request, handler and data access. Less indirection and each
feature readable in one place. Less widely recognized in interviews, and
slices tend to duplicate one another without active care.

## Decision

Clean Architecture.

## Consequences

Makes easier: dependency direction is enforced at compile time, so an attempt
to reach EF Core from Domain fails to build rather than passing review. Domain
and Application are testable without a database or a web host. The structure
matches the vocabulary used in interviews.

Makes harder: more projects and more indirection than this app needs today.
Adding a feature touches several projects. Early on, some interfaces will have
exactly one implementation and will look like overhead.

Revisit if: the ceremony starts costing more than the boundaries return - most
likely signal is interfaces created only to satisfy the layout rather than to
describe a real seam.
