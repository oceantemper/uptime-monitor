# 0006. Store durations as PostgreSQL interval

Date: 2026-11-10
Status: Accepted

## Context

The persistence layer needs to store `CheckResult.Duration` and `MonitoredEndpoint.CheckInterval`, both represented as .NET `TimeSpan`. Check durations are measured data that will be aggregated in SQL, including average latency, slowest checks, and percentiles over a time window. The stored values should also be readable when inspecting the database.

The initial alternative of storing `TimeSpan` ticks in `bigint` preserves .NET's 100-nanosecond precision. That precision does not decide this case: HTTP timing jitter is measured in milliseconds, and the important requirements are useful SQL aggregation and readable values.

## Options considered

**Store durations as ticks in `bigint`.** This preserves the full .NET `TimeSpan` precision and provides an exact fixed-duration representation. Its cost is that aggregation queries require manual arithmetic and the stored values are not readily understandable when inspecting the table.

**Store durations as PostgreSQL `interval`.** This provides a readable duration representation and supports SQL operations such as `AVG`, `MIN`/`MAX`, and `percentile_cont` without per-query tick conversion. PostgreSQL intervals can represent calendar months, unlike a fixed elapsed .NET `TimeSpan`, and have microsecond rather than 100-nanosecond precision. This application stores elapsed durations, not calendar-month values, and the precision difference is immaterial for HTTP checks.

## Decision

Map both `CheckResult.Duration` and `MonitoredEndpoint.CheckInterval` to PostgreSQL `interval`. The SQL aggregation and table-readability needs for measured check durations outweigh the tick representation's finer precision. Using `interval` for the non-aggregated check interval as well keeps duration persistence consistent.

## Consequences

SQL can aggregate and inspect check durations directly, supporting latency summaries without custom tick arithmetic. Both duration fields use a database type intended for time spans. PostgreSQL's interval semantics and microsecond precision should be kept in mind; these values represent elapsed time, not calendar periods. Revisit the mapping if requirements change to depend on finer-than-microsecond precision or calendar-relative duration semantics.
