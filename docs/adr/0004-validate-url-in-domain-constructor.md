# 0004. Validate endpoint URLs in the Domain constructor

Date: 2026-11-09
Status: Accepted

## Context

A monitored endpoint represents an HTTP(S) target that the monitor can check. Allowing an endpoint to be created without a valid URL would permit invalid domain state and defer an essential failure until a later operation. The Domain project has no dependencies, so validation must use framework types available there.

## Options considered

**Allow an endpoint to exist without a valid URL and validate later.** This allows incomplete endpoint data during creation, but invalid state can persist until a later layer or operation detects it.

**Require a valid URL at endpoint construction.** This prevents invalid endpoint state immediately, but makes the Domain responsible for URL syntax and the supported scheme constraint.

## Decision

`MonitoredEndpoint` construction requires an absolute URL using HTTP or HTTPS and rejects values that do not satisfy that rule. Other entry points may also validate input for clearer caller-facing errors, but the Domain remains the invariant-enforcing boundary.

## Consequences

Makes easier: every constructed monitored endpoint has a usable URL, and callers cannot bypass this invariant by skipping API-level validation.

Makes harder: URL syntax and the HTTP/HTTPS restriction are Domain concerns, so changing the accepted URL rules changes the Domain model.

Revisit if the concept of a monitored target changes beyond absolute HTTP(S) URLs or if the domain invariant itself changes. Caller-facing validation can be added independently without replacing constructor validation.
