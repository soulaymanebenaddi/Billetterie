# AGENTS.md

## Project Overview

Billetterie is a modular .NET backend for event ticketing.

Architecture:

- Billetterie.Api
- Billetterie.Application
- Billetterie.Domain
- Billetterie.Infrastructure
- Billetterie.Web

## Architecture Rules

- Billetterie.Domain contains core business rules and entities.
- Billetterie.Application contains use cases and application orchestration.
- Billetterie.Infrastructure contains persistence and external integrations.
- Billetterie.Api exposes the backend through HTTP endpoints.
- Billetterie.Web contains the frontend/UI.

Dependency direction:

- Domain must not depend on Application, Infrastructure, Api, or Web.
- Application may depend on Domain.
- Infrastructure may depend on Application and Domain.
- Api may depend on Application and Infrastructure.
- Web should communicate with the backend through the Api and should not depend directly on Infrastructure.

## Domain Rules

- Domain entities should protect their state through encapsulation.
- Prefer private setters on entity properties.
- State changes should go through domain methods such as Publish(), Cancel(), etc.
- Constructors should prevent invalid entity states.
- Avoid putting EF Core-specific logic in Domain.

## Infrastructure Rules

- EF Core and PostgreSQL configuration belong in Infrastructure.
- BilletterieDbContext must remain infrastructure-specific.
- Do not leak database-specific concerns into Domain.

## Build and Verification

Run when relevant:

dotnet restore
dotnet build
dotnet test

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
9. Any other improvements related to:
   - code quality
   - readability
   - performance
   - security
   - scalability
   - maintainability

For every issue:
- explain why it is a problem;
- indicate the potential impact;
- suggest a concrete improvement when possible.

Avoid commenting on purely cosmetic preferences unless they affect readability, consistency, or maintainability.### Architecture

- Flag dependency-direction violations.
- Flag business logic placed in Api or Infrastructure when it belongs in Domain.
- Flag database-specific logic leaking into Domain.

### Domain Modeling

- Flag public setters that allow uncontrolled entity mutation.
- Flag constructors that allow invalid domain states.
- Flag state transitions that bypass domain methods.

### Correctness

- Flag nullability issues.
- Flag incorrect exception types.
- Flag logic bugs and regressions.
- Flag missing validation where invalid domain state could be created.

### Security

- Flag hardcoded secrets or credentials.
- Flag unsafe handling of sensitive configuration.
- Flag authorization or authentication issues when relevant.

### Review Style

- Focus on correctness, architecture, security, and maintainability.
- Do not comment on minor formatting unless it affects correctness.
