```md
# ADR-0002 — Use PostgreSQL as the Primary Database

## Status

Accepted

## Context

Billetterie requires persistent storage for highly relational business data.

Examples include:

- users;
- venues;
- sections;
- rows;
- seats;
- events;
- reservations;
- orders;
- payments;
- tickets.

These concepts have strong relationships with each other.

For example:

```text
Venue
  ↓
Section
  ↓
Row
  ↓
Seat
````

and:

```text
Customer
   ↓
Reservation
   ↓
Order
   ↓
Payment
   ↓
Ticket
```

The ticketing workflow also requires strong consistency for operations such as seat reservation and payment processing.

Important requirements include:

* relational integrity;
* transactions;
* constraints;
* concurrency control;
* indexing;
* reliable persistence.

Several database technologies were considered, including:

* PostgreSQL;
* Firebase Firestore;
* MongoDB;
* SQL Server.

## Decision

Billetterie will use **PostgreSQL** as its primary relational database.

Entity Framework Core will initially be used as the main persistence abstraction for the .NET backend.

The PostgreSQL provider will be used to connect Entity Framework Core to PostgreSQL.

## Consequences

### Positive

* strong relational model;
* ACID transactions;
* foreign key constraints;
* unique constraints;
* advanced indexing;
* strong concurrency features;
* mature SQL capabilities;
* good support from Entity Framework Core;
* suitable for reservation and purchasing workflows.

### Negative

* schema changes require migrations;
* relational modelling requires more upfront design than some document databases;
* developers must understand SQL and database behaviour instead of relying exclusively on an ORM;
* production deployment requires management of a PostgreSQL database.

## Alternatives Considered

### Firebase Firestore

Rejected as the primary database because the core domain is strongly relational and requires transactional consistency around reservations, orders and tickets.

Firestore may simplify certain application scenarios, but PostgreSQL better matches the requirements of the core ticketing domain.

### MongoDB

Rejected because the domain contains many important relationships and transactional rules that are naturally represented using a relational model.

### SQL Server

Considered a valid alternative.

PostgreSQL was selected because it provides the required relational and transactional capabilities while being open source and widely usable across different hosting environments.

## Implementation Guidelines

PostgreSQL should enforce important constraints whenever practical.

Examples include:

* primary keys;
* foreign keys;
* unique constraints;
* non-null constraints;
* appropriate indexes.

Entity Framework Core must not be treated as a replacement for understanding the underlying database behaviour.

Generated SQL and database performance should be inspected when relevant.

## Review Conditions

This decision may be reconsidered only if future requirements demonstrate that PostgreSQL is no longer suitable as the primary datastore.

Additional databases may be introduced for specialized workloads, but PostgreSQL should remain the primary transactional database unless a separate architectural decision replaces it.

````