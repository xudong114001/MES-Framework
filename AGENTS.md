# MES Framework - Agent Instructions

## Agent Preferences

- Use **sub-agents** for parallel task execution when appropriate
- Clearly label each sub-agent with descriptive names (e.g., "分析后端代码")
- Summarize results from all sub-agents before presenting to user
- Git commit messages should be in **Chinese** (中文)

## Quick Start

| Service | URL | Credentials |
|---------|-----|-------------|
| Backend API | http://localhost:5180 | admin / Admin@2026! |
| Swagger | http://localhost:5180/swagger | - |
| Frontend Dev | http://localhost:5173 | - |
| RabbitMQ UI | http://localhost:15672 | mes_user / Mes@2026! |

## Run Commands

```powershell
# Start infrastructure (PostgreSQL, Redis, RabbitMQ)
docker-compose up -d

# Start backend (runs on port 5180)
cd src/MES.Api; dotnet run

# Start frontend
cd src/mes-web; npm run dev

# Run E2E tests
.\tests\e2e-test.ps1
```

## Architecture

- **Backend**: .NET 10, 4-layer Clean Architecture (Api → Application → Domain → Infrastructure)
- **Frontend**: Vue 3 + Element Plus + Pinia + TypeScript
- **Database**: PostgreSQL with EF Core (snake_case naming)
- **Auth**: JWT (8-hour expiry)
- **Real-time**: SignalR (Hub at /hubs/mes)
- **Cache**: Redis (anti-duplicate submit, batch number generation)

## Key Patterns

- **WorkOrder Status Flow**: PENDING → RELEASED → SCHEDULED → IN_PROGRESS → COMPLETED → CLOSED
- **Status can also go to**: CANCELLED, ON_HOLD (pause)
- Navigation properties must be nullable (e.g., `Material?`) for EF Core serialization
- All DbSets use `BaseEntity` for auto `CreatedAt`/`UpdatedAt` timestamps

## Important Files

- `src/MES.Api/Program.cs` - DI, middleware, seed data
- `src/MES.Infrastructure/Data/MesDbContext.cs` - EF Core context
- `src/MES.Infrastructure/Extensions/ServiceCollectionExtensions.cs` - Infrastructure DI
- `database/init.sql` - Database schema + seed data
- `.env` - Docker environment variables

## Testing

- Unit tests: `src/MES.Tests/` (xUnit + Moq)
- E2E tests: `tests/e2e-test.ps1` (PowerShell, tests all API endpoints)

## Seed Data

- Admin user: `admin / Admin@2026!` (SHA256 hashed, auto-created on startup)
- Operator: `operator / 123456`
- 2 Factories, 2 Workshops, 4 Production Lines, 12 Workstations
- 4 Materials (1 finished good + 3 components)
- 1 BOM, 1 Routing (7 steps), 2 Work Orders