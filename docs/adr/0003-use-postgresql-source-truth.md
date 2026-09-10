```md
# ADR-0003 — PostgreSQL as the Source of Truth

## Status

Accepted

## Context

Billetterie may eventually use several storage or infrastructure technologies.

Possible examples include:

- PostgreSQL;
- Redis;
- message queues;
- search engines;
- caches;
- external payment providers.

Introducing several technologies can create ambiguity regarding which system contains the authoritative state.

This is particularly dangerous for ticketing because critical data includes:

- seat availability;
- reservations;
- orders;
- payment state recorded by Billetterie;
- issued tickets.

For example, Redis may later be used to improve reservation performance or handle temporary data.

However, relying on multiple independent sources of truth could lead to inconsistent states such as:

```text
Redis:
Seat A12 = Available

PostgreSQL:
Seat A12 = Sold
````

The application therefore needs a clearly defined authoritative datastore.

## Decision

PostgreSQL will be the **authoritative source of truth** for the core transactional state of Billetterie.

This includes:

* events;
* venues;
* seating configuration;
* reservations;
* orders;
* payment state stored by Billetterie;
* tickets.

Other technologies may store derived, temporary or cached representations of this data, but they must not become independently authoritative unless explicitly documented by a future ADR.

Examples:

```text
PostgreSQL
    ↓
Authoritative state
```

```text
Redis
    ↓
Temporary or cached state
```

```text
RabbitMQ
    ↓
Transport of events/messages
```

## Consequences

### Positive

* clear ownership of transactional data;
* simpler reasoning about consistency;
* easier debugging;
* recovery remains possible if temporary systems lose their state;
* caches can be rebuilt from PostgreSQL;
* critical decisions do not depend solely on volatile infrastructure.

### Negative

* PostgreSQL remains involved in critical transactional workflows;
* some optimizations may require synchronization between PostgreSQL and secondary systems;
* cache invalidation must be handled correctly;
* temporary systems may occasionally contain stale data.

## Example

Suppose a user attempts to reserve seat `A12`.

Redis may eventually indicate that the seat is temporarily unavailable for performance reasons.

However, the final decision regarding whether the reservation can be created must remain consistent with the authoritative state stored in PostgreSQL.

A cache entry alone must never be sufficient to permanently sell or issue a ticket.

## External Payment Providers

An external payment provider such as Stripe may remain authoritative regarding whether an external payment transaction actually succeeded.

Billetterie will store its own representation of the payment state in PostgreSQL.

The application must synchronize external payment results into its own transactional model without treating frontend responses as authoritative payment confirmation.

The detailed payment synchronization strategy will be defined in a separate ADR.

## Caching Principle

Cached data may improve performance but must be treated as disposable.

If Redis or another cache is completely cleared, the application should remain capable of reconstructing critical state from PostgreSQL.

## Messaging Principle

A message broker may transport events related to changes in the application.

The existence of a message does not replace the persistent business state stored in PostgreSQL.

## Review Conditions

This decision may be reconsidered if a future architecture introduces a business module with a deliberately independent datastore.

Such a change must clearly document:

* which module owns the data;
* which datastore becomes authoritative;
* how consistency is maintained;
* how other modules access that state.

```
```
