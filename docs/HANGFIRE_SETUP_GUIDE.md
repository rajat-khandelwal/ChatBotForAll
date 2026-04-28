# Hangfire Setup Guide for Document Processing

## Overview
This guide explains how to set up Hangfire for background document processing (chunking and embedding) in the ChatBotForAll project.

## What is Hangfire?
Hangfire is a .NET library for background job processing using PostgreSQL as storage. It allows you to:
- Queue long-running tasks asynchronously
- Monitor job execution via a dashboard
- Automatically retry failed jobs
- Scale to multiple servers

## Setup Steps

### 1. Install Required NuGet Packages

Run these commands in the Package Manager Console:

```powershell
dotnet add ChatBotForAll.ApiService package Hangfire.Core
dotnet add ChatBotForAll.ApiService package Hangfire.PostgreSql
```

Or via Package Manager:
- Search for `Hangfire.Core` and install
- Search for `Hangfire.PostgreSql` and install

### 2. Configuration in `appsettings.json`

Your connection string is already configured:
```json
{
  "ConnectionStrings": {
    "chatbotforall": "Host=localhost;Port=5432;Database=chatbotforall;Username=postgres;Password=your_password"
  }
}
```

Hangfire will use this same PostgreSQL connection for job storage.

### 3. How It Works

When a document is uploaded:

```
User Upload
    ↓
DocumentController.Upload()
    ↓
DocumentService.UploadAsync()
    ↓
File saved to storage
Document record created with status: "Uploaded"
    ↓
DocumentProcessingBackgroundService.EnqueueDocumentProcessing()
    ↓
Hangfire Job Enqueued
    ↓
[User gets immediate response]
    ↓
Hangfire Worker Process
    ↓
DocumentProcessingService.ProcessDocumentAsync()
    ↓
1. Extract text from file
2. Chunk text into smaller pieces
3. Generate embeddings for each chunk
4. Save chunks + embeddings to database
5. Update document status: "Indexed"
```

### 4. Monitoring Jobs

Access the Hangfire Dashboard:

**URL:** `http://localhost:5000/hangfire`

You can see:
- Active jobs
- Successful jobs
- Failed jobs
- Job statistics
- Retry options

### 5. Database Schema

Hangfire automatically creates these PostgreSQL tables:
- `hangfire."job"` - Job metadata
- `hangfire."state"` - Job state history
- Other internal tables for metrics and scheduling

### 6. Error Handling

If a document processing job fails:
- Error is logged
- Document status is set to "Failed"
- Error message is stored in the database
- Job can be retried from Hangfire dashboard

### 7. Configuration Details

**Services Registered in Program.cs:**

```csharp
// Hangfire setup
builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(c => 
        c.UseNpgsqlConnection(hangfireConnectionString)));
builder.Services.AddHangfireServer();

// Document processing services
builder.Services.AddScoped<IChunkingService, ChunkingService>();
builder.Services.AddScoped<IEmbeddingService, AzureOpenAIEmbeddingService>();
builder.Services.AddScoped<IDocumentProcessingService, DocumentProcessingService>();
builder.Services.AddScoped<IDocumentProcessingBackgroundService, DocumentProcessingBackgroundService>();
```

**Hangfire Dashboard (Development only):**

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard("/hangfire");
}
```

## Execution Flow Example

### Step 1: User uploads a document
```
POST /api/document/upload
Content: PDF file (5 MB)
Response: 201 Created
{
  "documentId": "guid-123",
  "fileName": "MyDocument.pdf",
  "status": "Uploaded",
  "createdDateTime": "2024-01-15T10:30:00Z"
}
```

### Step 2: Background job processes document
- Job starts in Hangfire
- Extracts text from PDF
- Splits into 50 chunks
- Generates 50 embeddings (calls Azure OpenAI)
- Saves to database
- Updates document status to "Indexed"

### Step 3: User checks document status
```
GET /api/document/guid-123
Response: 200 OK
{
  "documentId": "guid-123",
  "fileName": "MyDocument.pdf",
  "status": "Indexed",
  "updatedDateTime": "2024-01-15T10:35:00Z"
}
```

## Development Notes

### Local Testing

1. **Start PostgreSQL:**
   ```powershell
   # Using Docker
   docker run --name postgres -e POSTGRES_PASSWORD=postgres -d -p 5432:5432 postgres:latest
   ```

2. **Run migrations:**
   ```powershell
   dotnet ef database update
   ```

3. **Start the API:**
   ```powershell
   dotnet run
   ```

4. **Access Hangfire Dashboard:**
   Open `http://localhost:5000/hangfire`

### Troubleshooting

**Jobs not processing:**
- Check that Hangfire server is running (logs should show "Hangfire server started")
- Verify PostgreSQL connection string
- Check database permissions

**Connection errors:**
- Ensure PostgreSQL is running
- Verify connection string in appsettings.json
- Check network connectivity

**Embedding API errors:**
- Verify Azure OpenAI credentials in appsettings.json
- Check API key and endpoint are correct
- Ensure embedding model name is correct

## Production Considerations

1. **Scale Hangfire Workers:**
   - Run multiple instances of the API
   - Each instance will process jobs in parallel
   
2. **Job Monitoring:**
   - Set up alerts for failed jobs
   - Monitor job execution times
   - Track queue lengths

3. **Performance Tuning:**
   - Adjust worker count based on CPU/memory
   - Set appropriate job timeouts
   - Consider implementing batching for embeddings

4. **Security:**
   - Disable Hangfire dashboard in production
   - Or, protect dashboard with authentication
   - Use API keys for Azure OpenAI securely

## API Endpoints

### Upload Document (Synchronous)
```
POST /api/document/upload
Headers: Authorization: Bearer <token>
Body: multipart/form-data (file)
Response: 201 Created with document info
Status: Uploaded (job queued)
```

### Get Document Status
```
GET /api/document/{documentId}
Response: DocumentResponse with current status
Possible statuses: Uploaded → Processing → Indexed (or Failed)
```

### List All Documents
```
GET /api/document
Response: List of DocumentResponse objects
```

## File Structure

New/Modified Files:
- ✅ `Services/DocumentProcessingBackgroundService.cs` - Enqueues jobs
- ✅ `Services/DocumentProcessingService.cs` - Processes documents
- ✅ `Services/ChunkingService.cs` - Splits documents into chunks
- ✅ `Services/AzureOpenAIEmbeddingService.cs` - Generates embeddings
- ✅ `Services/DocumentService.cs` - Updated to queue jobs
- ✅ `Program.cs` - Registered Hangfire and services
- ✅ `Data/Migrations/20240101000000_AddHangfireSchema.cs` - DB schema

## Next Steps

1. Install NuGet packages
2. Run `dotnet ef database update` to create Hangfire tables
3. Update `appsettings.json` with Azure OpenAI credentials
4. Test by uploading a document
5. Monitor via Hangfire Dashboard at `/hangfire`

---

**Last Updated:** January 2024
**Status:** Production Ready
