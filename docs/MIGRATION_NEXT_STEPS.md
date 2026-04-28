# Database Migration Setup - Final Steps

## ✅ What's Complete
- ✅ All code implemented and building successfully
- ✅ Hangfire configured
- ✅ pgvector EF Core integration added
- ✅ Migrations created
- ✅ All services registered

## ⏳ What's Needed Now

### Step 1: Install pgvector on PostgreSQL

The error shows: **"extension vector is not available"**

This means PostgreSQL doesn't have pgvector installed yet.

#### Quick Option - Docker (Recommended)

```powershell
# Pull pgvector-enabled PostgreSQL image
docker pull pgvector/pgvector:pg16

# Stop old PostgreSQL
docker stop postgres
docker rm postgres

# Run new PostgreSQL with pgvector
docker run --name postgres `
  -e POSTGRES_PASSWORD=postgres `
  -e POSTGRES_DB=chatbotforall `
  -d `
  -p 5432:5432 `
  pgvector/pgvector:pg16

# Wait 10 seconds for startup
Start-Sleep -Seconds 10

# Verify pgvector is available
docker exec postgres psql -U postgres -d chatbotforall -c "CREATE EXTENSION IF NOT EXISTS vector;"
```

#### OR - Install on Existing PostgreSQL

See `docs/PGVECTOR_INSTALLATION.md` for detailed installation steps

### Step 2: Apply Migrations

Once pgvector is installed:

```powershell
cd D:\Repos\ChatBotForAll\ChatBotForAll\ChatBotForAll.ApiService

dotnet ef database update
```

This will:
- Create Hangfire tables
- Create DocumentChunk table
- Create ChunkEmbedding table with pgvector support
- Create vector indexes for similarity search

### Step 3: Verify Migration

```powershell
# Check if tables were created
psql -U postgres -d chatbotforall -c "\dt"

# Verify pgvector extension
psql -U postgres -d chatbotforall -c "SELECT * FROM pg_extension WHERE extname = 'vector';"

# Check ChunkEmbedding table structure
psql -U postgres -d chatbotforall -c "\d \"ChunkEmbedding\""
```

### Step 4: Configure Azure OpenAI

Edit `ChatBotForAll.ApiService/appsettings.Development.json`:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://<your-resource>.openai.azure.com/",
    "ApiKey": "your-api-key-here",
    "EmbeddingDeploymentName": "text-embedding-3-small"
  }
}
```

### Step 5: Run Application

```powershell
cd D:\Repos\ChatBotForAll\ChatBotForAll\ChatBotForAll.ApiService
dotnet run
```

---

## 📊 What Will Happen After Migration

### Tables Created
```sql
-- Hangfire tables
hangfire.job
hangfire.state

-- Document processing
"Document" (updated with new fields)
"DocumentChunk" (new table)
"ChunkEmbedding" (new table with pgvector)
```

### Indexes Created
```sql
-- Vector similarity search index
CREATE INDEX "IX_ChunkEmbedding_Vector" 
ON "ChunkEmbedding" USING ivfflat ("Vector" vector_cosine_ops);
```

---

## 🚀 Final Deployment Commands

```powershell
# 1. Install pgvector (if not using Docker image)
# See PGVECTOR_INSTALLATION.md

# 2. Apply migrations
cd D:\Repos\ChatBotForAll\ChatBotForAll\ChatBotForAll.ApiService
dotnet ef database update

# 3. Check migration status
dotnet ef migrations list
# Should show: 
#   20260411171118_InitialCreate (Applied)
#   20xxxxxxxxxxxxxx_AddPgvectorSupport (Applied)

# 4. Configure credentials
# Edit appsettings.Development.json with Azure OpenAI details

# 5. Start application
dotnet run

# 6. Access services
# - API: http://localhost:5000
# - Hangfire Dashboard: http://localhost:5000/hangfire
# - Scalar API: http://localhost:5000/scalar
```

---

## ✅ Verification Checklist

After running migrations:

- [ ] pgvector extension installed on PostgreSQL
- [ ] `dotnet ef database update` completes successfully
- [ ] All tables visible in database
- [ ] Vector column exists in ChunkEmbedding table
- [ ] IVF-Flat index created
- [ ] Application starts without errors
- [ ] Hangfire dashboard accessible
- [ ] Document upload endpoint works
- [ ] Azure OpenAI API credentials configured

---

## 📝 Key Files Modified

- ✅ `Program.cs` - Added pgvector EF Core configuration
- ✅ `ChatBotDbContext.cs` - Added OnConfiguring with UseVector()
- ✅ New migrations added

---

## 🔗 Related Documentation

- `docs/PGVECTOR_INSTALLATION.md` - Detailed pgvector setup
- `docs/PGVECTOR_IMPLEMENTATION.md` - Full implementation details
- `docs/QUICK_START.md` - Quick reference guide
- `docs/HANGFIRE_SETUP_GUIDE.md` - Hangfire configuration

---

## ⚠️ Common Issues

| Error | Solution |
|-------|----------|
| "extension vector is not available" | Install pgvector on PostgreSQL (see step 1) |
| "Could not connect to server" | Verify PostgreSQL running, check connection string |
| "The model for context has pending changes" | Run `dotnet ef migrations add` (already done) |
| "Migration already exists" | Migration files already created, just run update |

---

**Current Status:** Ready for Migration ✅  
**Next Action:** Install pgvector, then run `dotnet ef database update`  
**ETA to Complete:** ~15 minutes
