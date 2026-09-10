# ADR-0001 — Use a Modular Monolith

## Status

Accepted

## Context

Billetterie is an event ticketing platform that will initially be developed and maintained as a single application.

The system contains several business areas such as:

- events;
- venues;
- reservations;
- orders;
- payments;
- tickets;
- identity.

A decision is required regarding the overall backend architecture.

The main options considered were:

- a traditional monolith;
- a modular monolith;
- microservices.

Microservices could provide independent deployment and scaling of individual services, but they would also introduce additional complexity related to:

- distributed communication;
- deployment;
- monitoring;
- service discovery;
- data consistency;
- network failures;
- development and testing.

These concerns are not currently justified by the scope of the MVP.

A traditional monolith would be simpler, but without clear module boundaries it could become difficult to maintain as the application grows.

## Decision

Billetterie will use a **modular monolith** for the initial architecture.

The backend will be deployed as a single application while keeping clear boundaries between business modules.

Initial business modules include:

- Identity;
- Events;
- Venues;
- Reservations;
- Orders;
- Payments;
- Tickets;
- Notifications.

The application will also separate technical responsibilities using the following projects:

```text
Billetterie.Api
Billetterie.Application
Billetterie.Domain
Billetterie.Infrastructure