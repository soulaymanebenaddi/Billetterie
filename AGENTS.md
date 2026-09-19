# AGENTS.md

## Project Overview

Billetterie is an event ticketing platform built as a modular monolith.

The project is intended to support multiple types of ticketed events, including:

- concerts;
- theatre performances;
- sports events;
- comedy shows;
- conferences;
- similar ticketed events.

The application contains:

- `Billetterie.Api` — ASP.NET Core HTTP API.
- `Billetterie.Application` — application use cases and orchestration.
- `Billetterie.Domain` — core business entities, rules, and invariants.
- `Billetterie.Infrastructure` — persistence and external integrations.
- `Billetterie.Web` — React + TypeScript frontend.

The project should demonstrate production-oriented software engineering practices while avoiding unnecessary complexity and premature overengineering.

The goal is to build a strong portfolio project incrementally, starting with a complete MVP and progressively adding reliability, performance, real-time capabilities, infrastructure, and observability.


## Product Goal

The core product should allow customers to discover events, reserve seats, purchase tickets, and access digital tickets.

It should also allow organizers to manage events and staff to validate tickets.

The system should eventually support different venue structures and ticketing scenarios without becoming unnecessarily generic during the MVP.


## Architecture Style

Billetterie uses a modular monolith architecture.

Business capabilities may eventually be organized around modules such as:

- Events
- Venues
- Reservations
- Orders
- Payments
- Tickets
- Identity
- Notifications

Do not introduce microservices unless a future documented requirement clearly justifies them.


## Architecture Rules

### Billetterie.Domain

`Billetterie.Domain` contains:

- core business entities;
- domain rules;
- business invariants;
- domain behavior.

Domain must not depend on:

- Application;
- Infrastructure;
- Api;
- Web;
- EF Core;
- PostgreSQL;
- Stripe;
- HTTP concerns;
- UI concerns.

Domain should remain independent from technical infrastructure.


### Billetterie.Application

`Billetterie.Application` contains:

- use cases;
- application orchestration;
- abstractions required by use cases.

Application may depend on Domain.

Application must not depend on:

- Infrastructure;
- Api;
- Web.

Application should coordinate business operations but should not contain infrastructure-specific implementation details.


### Billetterie.Infrastructure

`Billetterie.Infrastructure` contains technical implementations such as:

- Entity Framework Core;
- PostgreSQL persistence;
- repositories when needed;
- external service integrations;
- Stripe integration;
- infrastructure configuration.

Infrastructure may depend on:

- Application;
- Domain.

Infrastructure must not depend on Api.


### Billetterie.Api

`Billetterie.Api` is the backend entry point.

It may depend on:

- Application;
- Infrastructure.

Its responsibilities include:

- HTTP endpoints;
- request/response handling;
- dependency injection composition;
- middleware;
- API configuration.

Avoid placing business rules directly in controllers or endpoints.


### Billetterie.Web

`Billetterie.Web` contains the frontend built with React and TypeScript.

The frontend communicates with the backend through the HTTP API.

It must not:

- access PostgreSQL directly;
- depend directly on Infrastructure;
- duplicate critical business rules that must be enforced by the backend.


## Dependency Direction

The intended project dependency direction is:

Application → Domain

Infrastructure → Application
Infrastructure → Domain

Api → Application
Api → Infrastructure

Domain must remain independent.

Forbidden dependencies include:

Domain → Application
Domain → Infrastructure
Domain → Api
Domain → Web

Application → Infrastructure
Application → Api
Application → Web

Infrastructure → Api


## Domain Modeling Rules

Domain entities should protect their own state.

Prefer:

- private setters;
- constructors that prevent invalid states;
- domain methods for meaningful state transitions;
- explicit business invariants.

For example:

- `Publish()`
- `Cancel()`
- `Confirm()`
- `Expire()`

Do not expose public setters when doing so would allow callers to bypass business rules.

Do not add domain methods only for the sake of having methods.

Add behavior when a real business rule requires the entity to control a state change.

Avoid putting EF Core attributes, database configuration, HTTP concerns, or external-service logic inside Domain.


## Venue Domain Model

The current venue hierarchy is:

Venue
→ VenueSpace
→ Section
→ Row
→ Seat

A `Venue` represents a physical location or complex.

Examples:

- theatre;
- arena;
- stadium;
- convention centre.

A `VenueSpace` represents the specific space inside a venue where an event takes place.

Examples:

- theatre hall;
- arena;
- auditorium;
- stage;
- room.

An `Event` should be associated with a `VenueSpace`, not directly with a `Venue`.

This allows a venue to contain multiple spaces.

Do not prematurely introduce seating layouts, general admission models, or multiple venue configurations unless a concrete use case requires them.


## Core Business Invariants

The system should preserve the following business rules.

- A seat must not ultimately be sold twice for the same event.
- An expired reservation cannot be completed.
- Seats associated with an expired or cancelled reservation should become available again unless they were sold.
- A purchase is confirmed only after a successful payment.
- A ticket can only be generated for a confirmed purchase.
- Duplicate tickets must not be generated accidentally.
- A successfully validated ticket must not be successfully validated again.
- Customers may purchase tickets only for events that are published and currently available for sale.

Some invariants will receive stronger concurrency and reliability guarantees in later milestones.

Do not prematurely introduce advanced infrastructure solely to solve future-scale scenarios.


## Infrastructure Rules

Entity Framework Core and PostgreSQL configuration belong in Infrastructure.

`BilletterieDbContext` must remain infrastructure-specific.

Do not leak:

- database-specific configuration;
- EF Core-specific concerns;
- PostgreSQL-specific concerns;

into Domain.

PostgreSQL is the primary source of truth for Billetterie's core transactional state.

External providers may remain authoritative for their own domain.

For example, Stripe is authoritative for the actual payment transaction processed by Stripe.

Caches and messaging systems introduced later must not become the authoritative source of core transactional data unless a new architectural decision explicitly changes this rule.


## MVP Scope — v0.1

The first milestone is a portfolio-ready MVP.

Target effort is approximately 50–60 hours.

The goal is to deliver a complete end-to-end ticket purchasing flow.

Core MVP capabilities:

- Browse published events.
- View event details.
- View available seats.
- Select a seat.
- Create a customer account.
- Authenticate.
- Temporarily reserve a seat.
- Expire or cancel reservations and release seats.
- Create an order.
- Complete payment through Stripe in test mode.
- Generate a digital ticket after successful payment.
- Include a QR code on the digital ticket.
- Allow customers to view their tickets.
- Allow organizers to create and manage basic events.
- Allow staff to validate tickets.
- Add essential tests.
- Provide a basic deployable version of the application.

The MVP should prioritize a complete working product over advanced scalability or infrastructure.

Once v0.1 is complete, the project should already be suitable for inclusion in a professional portfolio.


## MVP Non-Goals

Do not introduce these technologies during v0.1 by default:

- microservices;
- Kubernetes;
- RabbitMQ;
- Redis;
- distributed caching;
- event-driven architecture;
- SignalR;
- complex infrastructure-as-code;
- advanced distributed-system patterns.

They may be introduced in later milestones when a concrete requirement justifies them.

Do not add:

- abstractions;
- interfaces;
- repositories;
- services;
- patterns;
- infrastructure;

solely because they might theoretically be useful in the future.

Prefer the simplest design that correctly supports the current use case.


## Project Roadmap

### v0.1 — Portfolio MVP

Target: approximately 50–60 hours.

Focus on:

- React;
- TypeScript;
- ASP.NET Core;
- PostgreSQL;
- Entity Framework Core;
- events;
- venues and seating;
- event browsing;
- event details;
- seat selection;
- reservations;
- authentication;
- Stripe test payments;
- digital tickets;
- QR codes;
- essential testing;
- basic deployment.


### v0.2 — Concurrency and Reliability

Target: approximately 8–12 additional hours.

Focus on:

- robust prevention of double seat reservations;
- PostgreSQL transactions;
- appropriate concurrency-control mechanisms;
- testing approximately 50–100 concurrent requests attempting to reserve the same seat;
- Stripe webhook idempotency;
- reservation reliability;
- payment workflow reliability.

This milestone should harden the critical transactional paths introduced during the MVP.


### v0.3 — Testing and Performance

Target: approximately 6–10 additional hours.

Focus on:

- Testcontainers with a real PostgreSQL instance;
- broader integration test coverage;
- selected Playwright end-to-end tests;
- k6 load testing;
- measuring P95 and P99 latency;
- documenting useful performance results in the README.

Testing should demonstrate system behavior rather than merely increase test count.


### v0.4 — Real-Time Updates

Target: approximately 4–7 additional hours.

Introduce SignalR where it provides meaningful user value.

Example:

When one customer reserves seat `A12`, other connected customers viewing the same event should see the seat become unavailable without manually refreshing the page.


### v0.5 — Redis

Target: approximately 5–8 additional hours.

Introduce Redis only when there is a concrete use case.

Possible uses include:

- caching;
- temporary data;
- TTL-based reservation support.

Redis should not be added solely to increase the number of technologies listed in the project.

Where possible, measure and document the benefit produced by introducing Redis.


### v0.6 — Messaging

Target: approximately 6–10 additional hours.

Introduce RabbitMQ when asynchronous workflow decoupling provides a concrete benefit.

Possible flow:

Payment confirmed
→ Ticket generation
→ Email delivery
→ Other downstream processing

Explore:

- retries;
- failure handling;
- dead-letter queues;
- message processing reliability.

Do not introduce messaging before the synchronous MVP flow works correctly.


### v0.7 — Observability

Target: approximately 5–8 additional hours.

Introduce production-oriented observability.

Possible work includes:

- OpenTelemetry;
- structured logging;
- traces;
- metrics;
- endpoint response-time measurements;
- error monitoring;
- visibility into critical reservation and payment workflows.


### v0.8 — Advanced CI/CD and Infrastructure

Target: approximately 5–10 additional hours.

Improve delivery and deployment infrastructure.

Possible work includes:

- comprehensive GitHub Actions pipelines;
- automated tests in CI;
- Docker-based deployment workflows;
- automated deployments;
- environment configuration;
- potentially infrastructure-as-code.

Docker may already be used earlier for local development.

This milestone focuses on production-oriented automation and infrastructure rather than introducing Docker for the first time.


## Implementation Strategy

Prefer incremental vertical progress.

A typical feature should progress through the necessary layers rather than building large amounts of unused infrastructure in advance.

Typical flow:

Domain
→ Persistence
→ Application use case
→ API
→ Frontend

Only include the layers needed by the specific feature.

Prefer a working vertical slice over several disconnected abstractions.


## Planning Rules

When asked to propose new issues or determine the next implementation steps:

1. Inspect the current repository before proposing work.
2. Read this `AGENTS.md`.
3. Read relevant architecture documentation.
4. Read accepted ADRs.
5. Inspect the current implementation.
6. Inspect relevant existing issues and recent pull requests when available.
7. Treat repository documentation and accepted ADRs as the source of truth.
8. Do not propose work that has already been implemented.
9. Respect dependencies between tasks.
10. Prioritize the shortest logical path toward the current milestone.
11. Keep MVP work focused on v0.1.
12. Do not pull later-roadmap technologies into v0.1 unless required for correctness.
13. Prefer small issues that can normally be completed in one branch and one pull request.
14. Avoid speculative abstractions and premature generalization.
15. Identify meaningful architectural decisions that may require an ADR.
16. Do not create ADRs for trivial implementation details.
17. If repository documentation and implementation appear inconsistent, flag the inconsistency rather than silently changing the architecture.
18. If a requirement is unclear, prefer identifying the ambiguity rather than inventing product behavior.

When asked to propose the next issues, provide them in dependency order.

For each proposed issue include:

- title;
- short description;
- tasks;
- acceptance criteria;
- dependencies when applicable.

Do not implement the issues unless explicitly asked to do so.


## ADR Rules

Create an ADR when a decision:

- materially affects the architecture;
- has meaningful alternatives;
- introduces an important long-term constraint;
- would reasonably cause a future developer to ask why that approach was chosen.

Examples of ADR-worthy decisions include:

- modular monolith vs microservices;
- database technology;
- source-of-truth strategy;
- major concurrency strategy;
- major messaging architecture;
- major authentication architecture.

Do not create ADRs for every library, command, implementation detail, or local-development convenience.


## Local Development

Docker Compose may be used to provide reproducible local infrastructure such as PostgreSQL.

Local-development containerization alone does not require an ADR.

Do not commit real secrets or credentials.

Use appropriate mechanisms such as:

- .NET User Secrets;
- local `.env` files;
- environment variables;
- deployment secret stores.

Commit example configuration when useful, such as `.env.example`, without real credentials.


## Build and Verification

Run the relevant checks before considering work complete.

Backend:

```bash
dotnet restore
dotnet build
dotnet test
````

Frontend checks should also be run when frontend code is modified.

Use the existing frontend scripts defined in `Billetterie.Web/package.json`.

Do not claim an issue is complete when the application fails to build or start because of changes introduced by that issue.

## Code Review Rules

Review pull requests as a senior engineer.

Specifically look for the following and provide concrete improvement suggestions with reasoning:

1. Logical mistakes that could cause bugs or runtime errors.
2. Unhandled or poorly handled edge cases.
3. Poor or inconsistent naming conventions or styling that reduce readability.
4. Performance issues or meaningful optimization opportunities.
5. Security vulnerabilities, unsafe patterns, or security concerns.
6. Ambiguous or hard-to-understand code that would benefit from clarification or documentation.
7. Debugging code, temporary code, or development-only code that should not reach production.
8. Architectural violations or misplaced responsibilities.
9. Unnecessary complexity or premature abstractions.
10. Any other meaningful improvements related to:

    * code quality;
    * readability;
    * performance;
    * security;
    * scalability;
    * maintainability.

Do not recommend scalability-oriented complexity unless there is a concrete requirement for it.

For every reported issue:

* explain why it is a problem;
* indicate the potential impact;
* suggest a concrete improvement when possible.

Avoid commenting on purely cosmetic preferences unless they affect readability, consistency, correctness, or maintainability.

### Architecture Review

Flag:

* dependency-direction violations;
* business logic placed in Api or Infrastructure when it belongs in Domain;
* database-specific logic leaking into Domain;
* infrastructure concerns leaking into Application;
* frontend code bypassing the API;
* unnecessary coupling between modules;
* premature distributed-system complexity.

### Domain Modeling Review

Flag:

* public setters that allow uncontrolled entity mutation;
* constructors that allow invalid domain states;
* state transitions that bypass required domain behavior;
* business invariants that can easily be bypassed;
* infrastructure concerns inside domain entities;
* domain behavior implemented only in controllers or persistence code.

### Correctness Review

Flag:

* nullability issues;
* incorrect exception usage;
* logical bugs;
* regressions;
* missing validation where invalid states could be created;
* incorrect handling of dates or time zones;
* inconsistent transactional behavior when relevant.

### Security Review

Flag:

* hardcoded secrets or credentials;
* committed `.env` files containing secrets;
* unsafe handling of sensitive configuration;
* missing authentication when required;
* missing authorization checks;
* insecure direct object access;
* trust placed in client-controlled values that should be validated server-side;
* unsafe payment handling;
* sensitive information exposed through logs or API responses;
* injection risks when relevant.

### Performance Review

Flag meaningful performance issues such as:

* unnecessary database queries;
* N+1 query patterns;
* loading excessive data;
* blocking operations in asynchronous workflows;
* inefficient queries;
* avoidable repeated external calls.

Do not suggest caches, Redis, messaging, or distributed infrastructure solely to optimize hypothetical future workloads.

### Review Style

Focus primarily on:

* correctness;
* architecture;
* security;
* maintainability;
* meaningful performance concerns.

Prioritize findings by impact.

Do not manufacture issues merely to produce review comments.

If the implementation is correct and no meaningful issue exists, say so.

Do not comment on minor formatting unless it affects correctness, readability, or consistency.

````
