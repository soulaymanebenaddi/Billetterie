# System Architecture

```mermaid
flowchart LR
    WEB[Billetterie.Web]
    API[Billetterie.Api]
    APP[Billetterie.Application]
    DOMAIN[Billetterie.Domain]
    INFRA[Billetterie.Infrastructure]
    DB[(PostgreSQL)]

    WEB --> API
    API --> APP
    APP --> DOMAIN
    INFRA --> APP
    INFRA --> DOMAIN
    INFRA --> DB