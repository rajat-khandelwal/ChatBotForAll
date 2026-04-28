# PostgreSQL pgvector Extension Installation

## Problem
PostgreSQL doesn't have the `pgvector` extension installed. We need to install it before running migrations.

## Solution - Installation Methods

### Option 1: Docker (Recommended for Development)

If you're using Docker with PostgreSQL, you can use a pgvector-enabled image:

```powershell
# Stop existing PostgreSQL container
docker stop postgres

# Remove old container
docker rm postgres

# Run pgvector-enabled PostgreSQL
docker run --name postgres `
  -e POSTGRES_PASSWORD=postgres `
  -e POSTGRES_DB=chatbotforall `
  -d `
  -p 5432:5432 `
  pgvector/pgvector:pg16
```

Then verify connection works and run migrations.

### Option 2: Manual Installation on Windows PostgreSQL

#### Step 1: Check Your PostgreSQL Version
```powershell
# Connect to PostgreSQL and check version
psql -U postgres -h localhost -d postgres -c "SELECT version();"
```

#### Step 2: Download pgvector Release
Visit: https://github.com/pgvector/pgvector/releases

Download the appropriate release for your PostgreSQL version (e.g., `pgvector-0.5.1.tar.gz` for PostgreSQL 16)

#### Step 3: Extract and Build
```powershell
cd $env:PGDATA  # Usually "C:\Program Files\PostgreSQL\16"
# Extract pgvector files to contrib\pgvector

# Then run:
$env:PATH = "C:\Program Files\PostgreSQL\16\bin;$env:PATH"
cd contrib\pgvector
make
make install
```

#### Step 4: Restart PostgreSQL
```powershell
# Restart PostgreSQL service
Restart-Service postgresql-x64-16  # or your version
```

#### Step 5: Create Extension
```powershell
psql -U postgres -h localhost -d chatbotforall -c "CREATE EXTENSION IF NOT EXISTS vector;"
```

### Option 3: Use Existing Docker Container

If PostgreSQL is running in Docker:

```powershell
# Connect to container and install pgvector
docker exec -it postgres sh -c "apt-get update && apt-get install -y postgresql-16-pgvector"

# Or use different method:
docker exec -it postgres psql -U postgres -d postgres -c "CREATE EXTENSION IF NOT EXISTS vector;"
```

---

## Verify Installation

After installation, verify pgvector is available:

```powershell
psql -U postgres -h localhost -d chatbotforall -c "CREATE EXTENSION IF NOT EXISTS vector;"
```

You should see:
```
CREATE EXTENSION
```

Or if already exists:
```
NOTICE:  extension "vector" already exists, skipping
CREATE EXTENSION
```

---

## Run Database Migrations

Once pgvector is installed:

```powershell
cd D:\Repos\ChatBotForAll\ChatBotForAll\ChatBotForAll.ApiService
dotnet ef database update
```

---

## Docker Setup (Quick Start)

If you don't have PostgreSQL installed, the easiest way is Docker:

```powershell
# Install Docker Desktop from https://www.docker.com/products/docker-desktop

# Pull pgvector image
docker pull pgvector/pgvector:pg16

# Run container
docker run --name chatbot-postgres `
  -e POSTGRES_PASSWORD=postgres `
  -e POSTGRES_DB=chatbotforall `
  -d `
  -p 5432:5432 `
  pgvector/pgvector:pg16

# Verify extension
docker exec chatbot-postgres psql -U postgres -d chatbotforall -c "SELECT * FROM pg_extension WHERE extname = 'vector';"
```

---

## PostgreSQL Native Build (Advanced)

For Windows native PostgreSQL installations:

1. Download: https://github.com/pgvector/pgvector/releases
2. Extract to `%PGDATA%\contrib\pgvector`
3. Build with Visual C++ compiler
4. Restart PostgreSQL service
5. Run: `CREATE EXTENSION vector;`

---

## Next Steps After Installation

```powershell
# 1. Verify pgvector is installed
psql -U postgres -d chatbotforall -c "CREATE EXTENSION IF NOT EXISTS vector;"

# 2. Apply migrations
cd D:\Repos\ChatBotForAll\ChatBotForAll\ChatBotForAll.ApiService
dotnet ef database update

# 3. Check migration status
dotnet ef migrations list

# 4. Start application
dotnet run
```

---

## Environment Variables for Connection

If needed, set connection string:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ConnectionStrings__chatbotforall = "Host=localhost;Port=5432;Database=chatbotforall;Username=postgres;Password=postgres"
```

Then run:
```powershell
dotnet ef database update
```

---

## Troubleshooting

### "extension vector is not available"
→ pgvector extension not installed on PostgreSQL server

### "Could not connect to server"
→ PostgreSQL not running, check connection string

### "Database does not exist"
→ Create database first: `CREATE DATABASE chatbotforall;`

---

## Recommended: Docker Setup

For development and testing, use Docker:

```dockerfile
# docker-compose.yml
version: '3.8'
services:
  postgres:
    image: pgvector/pgvector:pg16
    environment:
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: chatbotforall
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

volumes:
  postgres_data:
```

Run:
```powershell
docker-compose up -d
```

---

**Status:** Installation Required ⏳  
**Next:** Install pgvector extension, then run migrations
