# Architecture (Day 1)

## Overview
The system is a multi-tenant RAG SaaS with two frontends:
1. `Blazor Admin Panel` for tenant administration
2. `Embeddable Chat Widget` for client websites

Both frontends call the same backend API with tenant-aware authorization.

## High-Level Components
- `ChatBotForAll.Web` (Blazor)
  - Admin login/logout
  - Document upload/list/status
  - User and tenant settings (MVP subset)

- `ChatBotForAll.ApiService` (ASP.NET Core)
  - Auth endpoints
  - Tenant/user/document endpoints
  - RAG chat endpoint
  - Background ingestion orchestration

- `PostgreSQL`
  - Relational data (tenants/users/docs/conversations)
  - Vector index (via `pgvector`) for chunk similarity

- `Storage` (abstracted)
  - Document file storage per tenant path

- `LLM + Embedding Provider` (abstracted)
  - Embedding generation
  - Final answer generation

## Request Flows

### 1) Admin Document Upload
1. Tenant admin authenticates
2. Blazor uploads file to API
3. API stores file and creates `Document` record (`Uploaded`)
4. Ingestion pipeline extracts text -> chunks -> embeddings
5. Chunks and vectors are stored with `TenantId`
6. Document status becomes `Indexed` or `Failed`

### 2) Chat Question (RAG)
1. User/visitor sends question via widget/API
2. API resolves tenant context
3. Query embedding is generated
4. Similar chunks are retrieved (tenant-filtered)
5. Prompt is built from top chunks + system instructions
6. LLM generates answer
7. API returns answer + citations
8. Conversation/message saved with `TenantId`

## Multi-Tenancy Strategy
- Every business entity contains `TenantId`
- Every query includes tenant filter
- Authorization policies validate tenant access
- Files stored under tenant-specific directories
- No shared conversation/document data across tenants

## Security Model
- Auth: Identity + JWT/Cookie (implementation choice)
- Role-based policies:
  - `PlatformAdmin`
  - `TenantAdmin`
  - `TenantUser`
- Input validation for uploads and chat inputs
- Audit trail for key actions

## Suggested Project Boundaries
- `ApiService`
  - `Controllers`/`Endpoints`
  - `Application` services (ingestion, retrieval, chat)
  - `Infrastructure` (storage, providers)
- `Web` (Blazor)
  - `Pages` and `Components`
  - Auth state and role guards

## Operational Considerations
- Log ingestion failures with reasons
- Track latency and token usage per tenant
- Add retry mechanism for failed ingestion jobs
- Add health endpoints for DB/provider dependencies

## Day 1 Decisions Captured
- Build MVP as multi-tenant first (not single-tenant retrofit)
- Keep provider integrations behind interfaces
- Start with document types: `pdf`, `txt`, `md`
