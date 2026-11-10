# 0002. Use Application-owned DTOs at the layer boundary

Date: 2026-11-09
Status: Accepted

## Context

The Application layer coordinates use cases while depending on Domain. Use-case callers need request and response types, but Domain entities should not cross out of the Application layer. The external representation of an endpoint URL is a string, while the Domain model uses `Uri`.

Mapping is shared by multiple use cases. We considered keeping mapping in private use-case methods, but that would duplicate a common `MonitoredEndpoint` to `EndpointSummary` mapping as more use cases are added. A static factory on the DTO centralizes this mapping without introducing an interface, dependency injection, or a mapping library.

## Options considered

**Expose Domain entities as use-case inputs and outputs.** This avoids separate DTOs and mapping. It would allow Domain types to cross the Application boundary and expose `Uri` rather than the chosen string representation.

**Use Application-owned DTOs.** This keeps use-case contracts owned by Application and lets the boundary represent URL values as strings. The cost is defining DTO types and mapping between them and Domain entities.

**Keep mapping in private methods on each use case.** This is simple for a mapping used once, but duplicates mapping when multiple use cases need the same projection.

**Put shared mapping in a static DTO factory.** `EndpointSummary.From(MonitoredEndpoint)` provides one directly testable, reusable mapping without a mapper abstraction or library. Its cost is that the DTO factory depends on the corresponding Domain type.

## Decision

Use Application-owned request and response DTOs for use-case contracts. Domain entities do not cross the Application boundary. Represent endpoint URLs as strings in DTOs even though the Domain model uses `Uri`.

Place reusable Domain-to-DTO mapping in static factories on DTOs, such as `EndpointSummary.From(MonitoredEndpoint)` and `CheckResultSummary.From(CheckResult)`. Use cases return these DTOs, not Domain entities.

## Consequences

Makes easier: Application contracts remain independent of Domain entity representation; callers receive the intended URL representation; and shared mappings have one reusable, directly testable definition without adding mapping infrastructure.

Makes harder: DTOs duplicate some entity fields and require mapping code. Static DTO factories couple the DTO type to the Domain type and suit direct transformations rather than mappings requiring use-case-specific context or external lookups.

Revisit if DTO mappings need substantial context or lookups, or if use cases require meaningfully different projections that no longer belong in one shared factory.
