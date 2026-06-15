# Architecture

## Overview

The Claims Module follows Clean Architecture with a strict unidirectional dependency rule — inner layers never reference outer layers. This means the business logic in Domain and Application is completely isolated from infrastructure concerns like databases, HTTP, and file storage.

```
Domain ← Application ← Infrastructure
                    ← Persistence
                    ← API
```

---

## Backend Layers

### Domain
The innermost layer. Contains only business concepts — no framework dependencies, no NuGet packages beyond the .NET base library.

- **Entities** — `Claim`, `LossEvent`, `ClaimParty`, `ClaimRiskObject`, `ClaimReserveComponent`, `ReserveHistory`, `ClaimDocument`, `ClaimAuditLog`, `CauseOfLossCode`, `Policy`
- **Enumerations** — `ClaimStatus`, `PartyRole`, `AssetType`, `ReserveComponent`, `ApprovalStatus`, `UserRole`, and more
- **Exceptions** — `InvalidStatusTransitionException`, `SelfApprovalException`, `InsufficientAuthorityException`, `ClaimNotFoundException`
- **Events** — `ClaimCreatedEvent`, `ClaimStatusChangedEvent`, `ReserveApprovedEvent`

### Application
Orchestrates use cases. Defines interfaces that other layers implement. Never references EF Core, Hangfire, or any infrastructure concern directly.

**CQRS pattern via MediatR:**
- Commands (write operations): `CreateClaimCommand`, `TransitionClaimStatusCommand`, `CreateReserveCommand`, `ApproveReserveCommand`, `RejectReserveCommand`, `RetractReserveCommand`, `AddPartyCommand`, `RemovePartyCommand`, `AddRiskObjectCommand`, `RemoveRiskObjectCommand`, `UploadDocumentCommand`, `DeleteDocumentCommand`
- Queries (read operations): `GetClaimDetailQuery`, `ListClaimsQuery`, `GetAuditLogQuery`, `ValidateClaimQuery`

**Pipeline behaviours (MediatR middleware):**
- `ValidationBehaviour` — runs FluentValidation before every handler
- `LoggingBehaviour` — logs request start and completion

**Interfaces defined here, implemented elsewhere:**
- `IApplicationDbContext` — database access
- `ICurrentUserService` — current user identity and role
- `IAuditLogService` — writing audit log entries
- `IStorageService` — file upload/download/delete
- `IClaimNumberGenerator` — atomic claim number generation
- `ICorrelationIdService` — request correlation tracking
- `IBackgroundJobService` — scheduling background jobs

### Infrastructure
Implements interfaces for external services. Contains no business logic.

- `LocalFileSystemStorageService` / `AzureBlobStorageService` — implements `IStorageService`. Switched via `appsettings.json` `Storage:Provider` value
- `PostGLReserveChangeJob` — Hangfire fire-and-forget job triggered on reserve approval. Simulates GL posting with idempotency key enforcement
- `SlaMonitoringJob` — Hangfire recurring job (cron `*/15 * * * *`). Flags claims in Draft/Open status untouched for 48+ hours
- `HangfireBackgroundJobService` — implements `IBackgroundJobService`, wraps Hangfire's `IBackgroundJobClient`

### Persistence
All database concerns. EF Core with SQL Server.

- `ApplicationDbContext` — implements `IApplicationDbContext`. All `DbSet<T>` properties use expression-body syntax (`=> Set<T>()`)
- Entity configurations via `IEntityTypeConfiguration<T>` — all column types, constraints, indexes, and relationships defined in Fluent API (no data annotations)
- `AuditLogService` — implements `IAuditLogService`. Writes `ClaimAuditLog` entries
- `ClaimNumberGenerator` — implements `IClaimNumberGenerator`. Uses `NEXT VALUE FOR dbo.ClaimNumberSequence` for atomic, gap-free claim numbers in format `CLM-YYYY-0000001`

**EF Core conventions enforced:**
- All primary keys use `NEWSEQUENTIALID()` for sequential GUIDs reducing index fragmentation
- All monetary columns use `decimal(19,4)`
- All datetime columns use `datetimeoffset(7)`
- `Claim` and `ClaimReserveComponent` have `ROWVERSION` for optimistic concurrency
- Global query filters on all entities for soft delete (`IsDeleted = 0`) and organisation isolation
- `ClaimAuditLog` and `ReserveHistory` have no soft delete — they are append-only and immutable

### API
Entry point. Receives HTTP requests and delegates to MediatR. Contains no business logic.

- Controllers: `ClaimsController`, `ReservesController`, `PartiesController`, `RiskObjectsController`, `DocumentsController`, `ReferenceController`
- `ExceptionHandlingMiddleware` — maps domain exceptions to HTTP status codes (404, 422, 500) with spec-compliant error shape
- `CorrelationIdMiddleware` — generates or reads `X-Correlation-ID` header, stores in `HttpContext.Items`, echoed in response
- `CurrentUserService` — reads `X-User` header and returns the matching mock user with ID, name, and role

---

## CQRS Request Flow

```
HTTP POST /api/claims
    ↓
ClaimsController.CreateClaim()
    ↓
IMediator.Send(CreateClaimCommand)
    ↓
LoggingBehaviour — logs "Handling CreateClaimCommand"
    ↓
ValidationBehaviour — runs CreateClaimCommandValidator
    → Invalid: throws ValidationException → 422
    → Valid: continues
        ↓
CreateClaimCommandHandler.Handle()
    → IClaimNumberGenerator.GenerateAsync()
    → builds Claim + LossEvent entities
    → IApplicationDbContext.SaveChangesAsync()
    → IAuditLogService.LogAsync(CLAIM_CREATED)
    → returns Guid (new claim ID)
        ↓
LoggingBehaviour — logs "Handled CreateClaimCommand"
    ↓
ClaimsController returns 201 Created with Guid
```

---

## Domain Events

Domain events announce that a significant business thing occurred. They are raised by handlers and consumed by other services.

| Event | Raised by | Handled by |
|---|---|---|
| `ClaimCreatedEvent` | `CreateClaimCommandHandler` | `AuditLogService` — writes `CLAIM_CREATED` |
| `ClaimStatusChangedEvent` | `TransitionClaimStatusHandler` | `AuditLogService` — writes `CLAIM_STATUS_CHANGED` |
| `ReserveApprovedEvent` | `ApproveReserveCommandHandler` | `HangfireBackgroundJobService` — enqueues GL posting job |

---

## Reserve Authority Model

Three-tier authority enforced in `CreateReserveCommandHandler` and `ApproveReserveCommandHandler`:

| Amount | Handler | Supervisor | Manager |
|---|---|---|---|
| ≤ $10,000 | Auto-approved | Auto-approved | Auto-approved |
| $10,001 – $100,000 | PendingApproval | Auto-approved | Auto-approved |
| > $100,000 | PendingApproval | PendingApproval | Auto-approved |

Additional rules:
- Self-approval is always prevented regardless of role
- Aggregate cap of $10,000,000 per claim unless manager sets `AggregateOverrideFlag`
- Reserve history is append-only — balances are updated on the `ClaimReserveComponent`, history rows are never modified

---

## Claim Status State Machine

```
Draft ──────────────────────────────────────► Withdrawn
  │
  ▼
Open ──────────────────────────────────────► Withdrawn
  │
  ├──► UnderInvestigation ◄──────────────────┐
  │         │                                │
  │         ▼                                │
  └──► PendingPayment ─────────────────────► Closed ──► Reopened
                                                            │
                                                            ▼
                                                           Open
```

Closure pre-flight checks (all must pass):
- CC-01: No reserve components in `PendingApproval` status
- CC-03: At least one party with role `Claimant` exists
- CC-04: If any reserve has balance > 0, requires `confirmCloseWithOpenReserves: true` and a written justification

---

## Hangfire Job Design

### PostGLReserveChangeJob
- **Trigger:** fire-and-forget, enqueued immediately on reserve approval
- **Idempotency:** each job carries a key in format `Reserve:{componentId}:Change:{sequence}`. If `PostingStatus` is already `Posted`, the job exits immediately without re-posting
- **Audit:** writes `GL_POSTING_SIMULATED` with message `DR Change in Outstanding Reserves / CR Outstanding Loss Reserves, Amount = {amount}`

### SlaMonitoringJob
- **Trigger:** recurring cron `*/15 * * * *` (every 15 minutes)
- **Logic:** queries all claims in `Draft` or `Open` status where `UpdatedAt < UtcNow - 48 hours`
- **Output:** writes `SLA_BREACH_FLAGGED` audit entry per breached claim
- **Deduplication:** relies on the audit log as the record — does not modify claim status

---

## Audit Log Design

`ClaimAuditLog` is intentionally excluded from soft delete and optimistic concurrency patterns. It has no `IsDeleted`, no `UpdatedAt`, no `RowVersion`. Every entry is final.

15+ event types as constants in `AuditEventTypes`:

```
CLAIM_CREATED, CLAIM_STATUS_CHANGED, CLAIM_UPDATED, CLAIM_CLOSED
PARTY_ADDED, PARTY_UPDATED, PARTY_REMOVED
RISK_OBJECT_ADDED, RISK_OBJECT_UPDATED, RISK_OBJECT_REMOVED
RESERVE_CREATED, RESERVE_AUTO_APPROVED, RESERVE_PENDING_APPROVAL
RESERVE_APPROVED, RESERVE_REJECTED, RESERVE_RETRACTED
DOCUMENT_UPLOADED, DOCUMENT_DELETED
GL_POSTING_SIMULATED, SLA_BREACH_FLAGGED, POLICY_UNKNOWN
```

Every audit entry carries a `CorrelationId` matching the HTTP request's `X-Correlation-ID` header, enabling reconstruction of all events from a single request.

---

## Data Model

```
Policies                CauseOfLossCodes
    │
    │
Claims ──────────────────────────────────────────────────────────────────┐
    │                                                                     │
    ├── LossEvents (1:1)                                                  │
    ├── ClaimParties (1:N)                                                │
    ├── ClaimRiskObjects (1:N)                                            │
    ├── ClaimDocuments (1:N)                                              │
    ├── ClaimAuditLog (1:N) ← append only, immutable                     │
    └── ClaimReserveComponents (1:N)                                      │
             └── ReserveHistory (1:N) ← append only, event sourced       │
                                                                          │
                          All entities scoped by OrganisationId ─────────┘
```

---

## Azure Architecture

```
                    ┌─────────────────────────────────────────┐
                    │           Azure (Poland Central)         │
                    │                                          │
  User Browser ────►│  Azure Static Web App                   │
                    │  (claims-module-ui)                      │
                    │  Angular 18 SPA                          │
                    │           │                              │
                    │           │ HTTPS API calls              │
                    │           ▼                              │
                    │  Azure App Service (Free F1)             │
                    │  (claims-module-api)                     │
                    │  .NET 9 ASP.NET Core API                 │
                    │           │                              │
                    │     ┌─────┴──────┐                      │
                    │     │            │                       │
                    │     ▼            ▼                       │
                    │  Azure SQL    Azure Blob Storage          │
                    │  Database     (claimsmoduleua1)          │
                    │  (Free tier)  claim-documents/           │
                    │               {orgId}/{claimId}/         │
                    └─────────────────────────────────────────┘
```

### Azure Services Used

| Service | Tier | Purpose |
|---|---|---|
| Azure App Service | Free F1 | Hosts the .NET 9 API on Windows |
| Azure SQL Database | Free tier | Stores all application data |
| Azure Blob Storage | LRS Standard | Stores uploaded claim documents |
| Azure Static Web App | Free | Hosts the Angular SPA with global CDN |

### How Services Interact

1. **User** accesses the Angular app via the Static Web App CDN URL
2. **Angular** makes API calls to the App Service via HTTPS — CORS policy restricts to the Static Web App origin
3. **App Service** reads/writes data to Azure SQL via EF Core with `EnableRetryOnFailure` for transient fault handling
4. **App Service** stores uploaded documents in Azure Blob Storage under `claim-documents/{organisationId}/{claimId}/{filename}`
5. **App Service** generates 1-hour SAS URLs for document downloads — clients download directly from Blob Storage, not through the API
6. **Hangfire** uses Azure SQL as its job store — background jobs (GL posting, SLA monitoring) run within the App Service process
7. **GitHub Actions** deploys both apps — backend via publish profile, frontend via Static Web App deployment token

### Key Design Decisions

**Why Windows App Service instead of Linux?**
The publish profile deployment method works more reliably on Windows for .NET apps. Linux would require Docker or zip deployment configuration.

**Why Free F1 App Service?**
Sufficient for assessment purposes. The limitation is no custom domain and the app sleeps after 20 minutes of inactivity — first request after sleep takes ~10 seconds. Scale up to B1 for production.

**Why local filesystem fallback for storage?**
The `IStorageService` interface abstracts the storage provider, switchable via a single config value. This allows local development without Azure credentials and clean production deployment without code changes.

**Why separate repos for backend and frontend?**
Independent deployment pipelines — the frontend can be deployed without touching the backend and vice versa. Cleaner separation of concerns for a team environment.

---

## Frontend Architecture

Angular 18 standalone components with lazy-loaded feature modules.

```
AppComponent (shell + toolbar + role switcher)
    │
    ├── /claims          → ClaimsListModule (lazy)
    │       └── ClaimsListComponent
    │
    ├── /claims/:id      → ClaimDetailModule (lazy)
    │       └── ClaimDetailComponent (6 tabs)
    │
    └── /fnol            → FnolModule (lazy)
            └── FnolComponent (3 steps)
```

**Core layer (singleton, loaded once):**
- `AuthService` — signal-based current user state, mock users, role switching
- `ClaimsService`, `ReservesService`, `DocumentsService`, `ReferenceService` — typed HTTP service layer
- `authInterceptor` — functional interceptor, adds `Authorization`, `X-User`, `X-Correlation-ID` headers

**Shared layer:**
- `StatusBadgeComponent` — color-coded claim status chip
- `ConfirmDialogComponent` — reusable confirmation dialog
- `RejectReserveDialogComponent` — rejection reason dialog
- `JustificationDialogComponent` — closure justification dialog
- `MaterialModule` — single import point for all Angular Material modules