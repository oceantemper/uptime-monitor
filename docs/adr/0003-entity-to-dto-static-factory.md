# 0003. Map entities to DTOs with static factories

Date: 2026-11-09
Status: Accepted

## Context

Multiple Application use cases may need the same mapping from a Domain entity to an Application-owned DTO. Keeping a private mapping method inside each use case would duplicate the mapping as additional use cases are introduced. The mapping is a direct transformation and does not require external lookups or contextual services.

## Options considered

**Private mapping methods in each use case.** This keeps each use case self-contained and is straightforward when a mapping is used once. Its cost is duplication when several use cases need the same entity-to-DTO mapping.

**A static factory on the DTO.** A method such as `EndpointSummary.From(MonitoredEndpoint)` gives the shared transformation one directly testable definition. It needs no interface, dependency injection, or mapping library. Its cost is that the DTO factory references the corresponding Domain entity.

## Decision

Put reusable, direct entity-to-DTO mappings in static factory methods on the Application DTOs. For example, `EndpointSummary.From(MonitoredEndpoint)` and `CheckResultSummary.From(CheckResult)`. Use cases call these factories when returning DTOs.

## Consequences

Makes easier: multiple use cases can reuse one mapping definition, and the mapping can be tested directly without setting up a use case or mapping infrastructure.

Makes harder: DTO factories depend on Domain types, and the approach is intended for direct transformations rather than mappings that require use-case-specific context or external lookups.

Revisit if mappings need context or external lookups, or if different use cases require meaningfully different projections.
