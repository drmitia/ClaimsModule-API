# AI Workflow

This document describes how AI assistance (Claude by Anthropic) was used throughout the development of the Claims Module.

---

## AI Tools Used

**Claude Sonnet** by Anthropic, accessed via claude.ai chat interface — used for the entire project from requirements analysis through deployment.

---

## Workflow Structure

### Phase 1 — Requirements Analysis
The two specification documents (FRS and Assessment brief) were uploaded to Claude at the start. Claude extracted all technical requirements, identified the 20-step build sequence ordered by layer dependency, and flagged the most complex areas (atomic claim number generation, reserve event sourcing, state machine enforcement, Hangfire idempotency).

### Phase 2 — Architecture & Domain Design
Claude proposed the Clean Architecture structure with 5 projects, explained the dependency rules, designed the 10-entity data model, and defined the CQRS command/query split. All entities, enumerations, exceptions, and domain events were generated with explanations before implementation.

### Phase 3 — Backend Implementation
Claude generated code layer by layer — Domain → Application → Persistence → Infrastructure → API. For each layer it explained what the layer does, why it exists, and how it connects to adjacent layers before writing any code.

### Phase 4 — Frontend Implementation
Claude generated the Angular project structure, all TypeScript models, core services, the HTTP interceptor, and all three feature screens with their components, templates, and styles.

### Phase 5 — Debugging & Refinement
Several bugs were identified and fixed through iterative debugging — Claude analysed error messages and stack traces to identify root causes.

### Phase 6 — Azure Deployment
Claude guided the complete Azure resource provisioning, GitHub Actions pipeline setup, and deployment troubleshooting step by step.

### Phase 7 — Documentation
Claude generated all three documentation files based on the actual implemented system.

---

## Representative Prompts

### Prompt 1 — Architecture Design
> "I have two requirement documents for a Claims Module. Review them and send me a plan of action and the key features."

**Purpose:** Load full context and produce a structured build plan before writing any code. This ensured architecture decisions were made upfront rather than discovered mid-implementation.

**Result:** A 9-phase build plan with correct dependency ordering — Domain before Application before Persistence, etc. This prevented the common mistake of building infrastructure before the domain model is stable.

---

### Prompt 2 — Layer Explanation
> "Explain me everything in this layer — why and for what we have these folders and role of classes. The idea of this layer and all in simple words."

**Purpose:** Used after each layer was built to verify understanding before moving to the next. Asked repeatedly for Domain, Application, Persistence, Infrastructure, and API layers.

**Result:** Plain-language explanations that confirmed the architecture was correctly understood — not just copied. This became the basis for the live review answers.

---

### Prompt 3 — Debugging
> "ClaimsModule.API.Middleware.ExceptionHandlingMiddleware[0] Unhandled exception Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException: The database operation was expected to affect 1 row(s), but actually affected 0 row(s)"

**Purpose:** Paste the exact error and stack trace to diagnose a runtime bug.

**Result:** Claude identified that `ROWVERSION` optimistic concurrency was firing because the `ClaimReserveComponent` was being created and updated in the same `SaveChangesAsync` call — before the database had assigned a real `RowVer` value. The fix was splitting into two separate saves. This was a non-obvious EF Core behaviour that would have taken significant time to debug manually.

---

### Prompt 4 — Architecture Violation Catch
> "we don't have Hangfire in this project installed" (referring to Application layer)

**Purpose:** Flagging that a generated handler was importing Hangfire directly in the Application layer.

**Result:** Claude identified this as an architecture violation — Application must not reference Infrastructure libraries. It proposed the `IBackgroundJobService` abstraction implemented in Infrastructure, keeping the dependency rule intact.

---

### Prompt 5 — Deployment Debugging
> "System.UnauthorizedAccessException: Access to the path 'C:\uploads' is denied."

**Purpose:** Diagnose an Azure App Service startup failure.

**Result:** Claude identified that the local filesystem uploads path was being created at startup even in production where Azure Blob Storage is used. The fix was wrapping the directory creation in `if (!app.Environment.IsProduction())`.

---

## Examples Where AI Output Was Incorrect

### Correction 1 — Inline HTML templates
Claude initially generated Angular components with inline HTML templates inside the `.ts` file using the `template:` property. This is poor practice for anything beyond trivial components.

**What AI generated:**
```typescript
@Component({
  template: `<mat-toolbar>...</mat-toolbar>`
})
```

**What was corrected:**
All components were split into separate `.ts`, `.html`, and `.scss` files using `templateUrl` and `styleUrl`. Claude agreed this was the correct approach when challenged.

**Lesson:** AI defaults to the quickest working solution, not always the best-practice one. Reviewing output critically and challenging conventions matters.

---

### Correction 2 — Wrong HTTP status code for warnings
Claude initially returned `422 Unprocessable Entity` for both blocking issues AND warnings in the claim closure flow. This meant the Angular `error` handler caught warnings instead of the `next` handler, making it impossible for the frontend to show the warning dialog.

**What AI generated:**
```csharp
if (!result.Succeeded)
    return UnprocessableEntity(result); // returned 422 for warnings too
```

**What was corrected:**
```csharp
// Only blocking issues get 422
if (!result.Succeeded && result.BlockingIssues.Any())
    return UnprocessableEntity(result);

// Warnings return 200 so Angular next handler receives them
return Ok(result);
```

**Lesson:** AI didn't reason about how the frontend would handle the different response types. The business requirement (warn but allow proceeding) required a 200 response, not a 4xx error. This required understanding both layers simultaneously.

---

### Correction 3 — YAML multiline commands
Claude generated multiline bash commands using `\` line continuation in GitHub Actions YAML:

```yaml
run: dotnet publish ClaimsModule.API/ClaimsModule.API.csproj \
  --configuration Release \
  --output ./publish
```

This caused `MSBuild: error MSB1008: Only one project can be specified` because the shell was treating `\` differently than expected on the GitHub Actions Linux runner.

**What was corrected:**
```yaml
run: dotnet publish ClaimsModule.API/ClaimsModule.API.csproj --configuration Release --output ./publish
```

Single-line commands. Simple fix but required recognising that YAML + shell line continuation behaves differently across environments.

---

### Correction 4 — Missing src/ folder assumption
Claude assumed the solution had a `src/` folder structure based on the initial architecture plan:

```yaml
--project src/ClaimsModule.Persistence/ClaimsModule.Persistence.csproj
```

But since the projects were created directly through Rider's UI without a `src/` subfolder, the correct path was:

```yaml
--project ClaimsModule.Persistence/ClaimsModule.Persistence.csproj
```

**Lesson:** AI makes assumptions about project structure based on what it has seen most often. Always verify generated paths against the actual filesystem.

---

## Parts That Benefited Most From AI Assistance

**Highest value:**
- EF Core entity configurations — all 10 configurations with correct `decimal(19,4)`, `datetimeoffset(7)`, `NEWSEQUENTIALID()`, `ROWVERSION` conventions would have taken hours to write manually
- MediatR pipeline wiring — the `ValidationBehaviour` and `LoggingBehaviour` pattern is boilerplate that AI handles perfectly
- Angular component scaffolding — generating the HTML templates, SCSS, and TypeScript for 3 complex screens simultaneously
- Debugging — paste a stack trace, get a root cause analysis in seconds

**Lowest value (required most human judgment):**
- Architecture decisions — AI suggested patterns but the decision to use them required understanding the tradeoffs
- Business rule correctness — AI generated the reserve authority logic but it required careful review against the spec to ensure $10K/$100K thresholds and self-approval prevention were correctly implemented
- UI/UX decisions — layout, component structure, what to show/hide for each role
- Deployment troubleshooting — each Azure error required human judgment about which fix to apply

---

## AI Interaction History

This entire conversation with Claude is available at:
**https://claude.ai/share/4f49f1a4-4ea3-4ef2-96cf-033e93a6f573**

The conversation covers the complete development journey from requirements analysis through Azure deployment — approximately 200+ exchanges covering architecture, implementation, debugging, and deployment.

---

## Honest Assessment

AI assistance compressed what would have been a 3-4 week project into a focused development effort. The most significant acceleration was in boilerplate generation — entities, configurations, handlers, and Angular components that follow predictable patterns.

However, every piece of generated code required review. AI made incorrect assumptions about project structure, violated architecture rules when not explicitly reminded, and occasionally generated code that worked locally but failed in production (the uploads path issue, the YAML multiline commands).

The correct mental model: AI is an extremely fast developer who knows every syntax and pattern but needs direction on architecture, business rules, and production concerns. The engineering judgment — what to build, how layers should interact, why a specific design decision is correct — remains entirely human.