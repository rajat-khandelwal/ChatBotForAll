# Domain Model (Day 1)

## Core Entities

## 1) Tenant
Represents a client organization.

Fields:
- `Id` (Guid)
- `Name` (string)
- `Slug` (string, unique)
- `IsActive` (bool)
- `CreatedAtUtc` (DateTime)
- `UpdatedAtUtc` (DateTime?)

## 2) AppUser
Authenticated user in the platform.

Fields:
- `Id` (Guid/string based on Identity config)
- `TenantId` (Guid, nullable for `PlatformAdmin` if needed)
- `Email` (string, unique per tenant or global by policy)
- `DisplayName` (string)
- `Role` (enum/string: `PlatformAdmin`, `TenantAdmin`, `TenantUser`)
- `IsActive` (bool)
- `CreatedAtUtc` / `UpdatedAtUtc`

## 3) Document
Uploaded file metadata and processing lifecycle.

Fields:
- `Id` (Guid)
- `TenantId` (Guid)
- `UploadedByUserId` (Guid/string)
- `FileName` (string)
- `ContentType` (string)
- `StoragePath` (string)
- `SizeBytes` (long)
- `Status` (enum: `Uploaded`, `Processing`, `Indexed`, `Failed`)
- `ErrorMessage` (string?)
- `CreatedAtUtc` / `UpdatedAtUtc`

## 4) DocumentChunk
Chunked text from a document used for retrieval.

Fields:
- `Id` (Guid)
- `TenantId` (Guid)
- `DocumentId` (Guid)
- `ChunkIndex` (int)
- `Text` (string)
- `TokenCount` (int)
- `SourcePage` (int?)
- `MetadataJson` (string/json)
- `CreatedAtUtc`

## 5) ChunkEmbedding
Vector representation of a chunk.

Fields:
- `Id` (Guid)
- `TenantId` (Guid)
- `DocumentChunkId` (Guid)
- `Model` (string)
- `Vector` (vector)
- `CreatedAtUtc`

Note: Can be merged into `DocumentChunk` if vector DB design supports it.

## 6) Conversation
Chat session scope.

Fields:
- `Id` (Guid)
- `TenantId` (Guid)
- `UserId` (Guid/string, nullable for anonymous visitor mode)
- `ExternalSessionId` (string?)
- `Title` (string?)
- `CreatedAtUtc` / `UpdatedAtUtc`

## 7) Message
Individual chat message in a conversation.

Fields:
- `Id` (Guid)
- `TenantId` (Guid)
- `ConversationId` (Guid)
- `Role` (enum: `User`, `Assistant`, `System`)
- `Content` (string)
- `CitationsJson` (string/json?)
- `PromptTokens` (int?)
- `CompletionTokens` (int?)
- `LatencyMs` (int?)
- `CreatedAtUtc`

## Relationships
- `Tenant` 1-* `AppUser`
- `Tenant` 1-* `Document`
- `Document` 1-* `DocumentChunk`
- `DocumentChunk` 1-1/* `ChunkEmbedding`
- `Tenant` 1-* `Conversation`
- `Conversation` 1-* `Message`

## Indexing Recommendations
- Unique: `Tenant.Slug`
- Index: `Document(TenantId, Status, CreatedAtUtc)`
- Index: `DocumentChunk(TenantId, DocumentId, ChunkIndex)`
- Index: `Conversation(TenantId, UserId, UpdatedAtUtc)`
- Vector index on `ChunkEmbedding.Vector`

## Multi-Tenant Guard Rules
- All reads/writes require tenant context
- Cross-tenant IDs are rejected at API boundary
- Background jobs must carry and enforce `TenantId`

## Initial Enums
- `DocumentStatus`: Uploaded, Processing, Indexed, Failed
- `MessageRole`: User, Assistant, System
- `UserRole`: PlatformAdmin, TenantAdmin, TenantUser
