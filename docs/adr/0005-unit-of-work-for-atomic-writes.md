# 0005. Use a Unit of Work for atomic multi-repository writes

Date: 2026-11-09
Status: Accepted

## Context

`RecordCheckResult` updates a monitored endpoint and adds a check result through separate repository operations. If the second write fails after the first has been committed, the endpoint can appear checked even though no corresponding result was recorded. Application must not reference EF Core; persistence implementations belong in Infrastructure.

## Options considered

**Application-level Unit of Work.** Repositories stage changes and the use case calls one Application-owned commit operation. Infrastructure can implement the operation with EF Core's transactional `SaveChanges`. This requires repositories to share a unit of work and not commit independently.

**Explicit transaction abstraction.** The use case opens a transaction around both repository operations, and Infrastructure implements the transaction boundary. This preserves separately executing writes but adds transaction lifecycle complexity and requires both repositories to participate in the same transaction.

**One combined persistence operation.** A port can represent recording a result and updating its endpoint together, with Infrastructure implementing it atomically. This is direct for this use case but couples the port to a specific operation.

## Decision

Use an Application-owned `IUnitOfWork` with `SaveChangesAsync`. Repository write methods stage changes; `RecordCheckResult` stages the endpoint update and result addition, then calls the unit of work once. `RegisterEndpoint` likewise saves its staged addition through the unit of work. Infrastructure will implement the persistence boundary so that one save commits staged changes atomically, without adding EF Core references to Application.

## Consequences

Makes easier: use cases can commit changes across repositories through one explicit boundary while Application remains independent of EF Core. A failure during the single transactional save will not leave only one of the staged writes committed.

Makes harder: repositories must share the same unit of work and avoid independent commits. The atomicity guarantee depends on the Infrastructure implementation correctly using a transaction for the save.

Revisit if repositories need independent commit behavior or a use case needs an explicit transaction lifecycle or a specialized combined persistence operation.
