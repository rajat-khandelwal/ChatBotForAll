# MVP Scope (Day 1)

## Product Vision
Build a multi-tenant RAG chatbot platform where each client (tenant) can:
- Upload knowledge documents from an admin panel
- Embed a chatbot on their website
- Keep users, data, and conversations isolated per tenant

## In Scope (MVP)
1. Multi-tenant isolation
2. Authentication (`login` / `logout`)
3. Tenant roles (`PlatformAdmin`, `TenantAdmin`, `TenantUser`)
4. Admin panel (Blazor) for document upload and document list
5. Document ingestion v1 (`.pdf`, `.txt`, `.md`)
6. Chunking + embeddings + vector retrieval
7. Chat API with RAG answer generation
8. Embeddable chatbot widget for external websites
9. Answer citations (source document reference)

## Out of Scope (Post-MVP)
- Billing/subscription management
- SSO/SAML/OAuth enterprise flows
- Advanced analytics dashboards
- Human handoff/live chat support
- Multi-language UI

## Primary Users
- `PlatformAdmin`: manages all tenants
- `TenantAdmin`: manages own tenant users, docs, and settings
- `TenantUser`: uses chatbot with tenant permissions
- `Website Visitor`: asks questions via embedded widget

## Success Criteria
- Tenant data isolation is enforced in API and database queries
- Tenant admin can upload a document and see it processed
- Chatbot can answer based on uploaded content with citation
- Chat widget can be embedded in a third-party website
- Authenticated users can login/logout successfully

## Functional Requirements (MVP)
- Create and manage tenants
- Create and manage tenant users
- Upload and track document processing status
- Ask questions and receive RAG-based responses
- Store conversation history per tenant/user

## Non-Functional Targets (MVP)
- Average chat response target: `< 4s` for typical questions
- Max upload size per file: `20 MB`
- Allowed document types: `.pdf`, `.txt`, `.md`
- All core entities include audit fields (`CreatedAtUtc`, `UpdatedAtUtc`, `CreatedBy`)

## Assumptions
- Stack: `.NET 10`, `Blazor`, `ASP.NET Core API`, `PostgreSQL`
- Vector storage: `pgvector` (or equivalent abstraction)
- LLM/Embedding provider selected in implementation phase

## Risks
- Poor retrieval quality due to weak chunking strategy
- Cross-tenant leakage if filters are missed
- Cost spikes from token/embedding usage

## Day 1 Exit Criteria
- Scope approved
- Roles and boundaries approved
- Architecture and domain model documented
- Ready to start implementation on Day 2
