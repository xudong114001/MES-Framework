# MES Framework - Agent Instructions

## Agent Preferences

- Use **sub-agents** for parallel task execution when appropriate
- Clearly label each sub-agent with descriptive names (e.g., "分析后端代码")
- Summarize results from all sub-agents before presenting to user

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

- **Backend**: .NET 10, 4-layer Clean Architecture (Api → Application → Domain ← Infrastructure)
- **Frontend**: Vue 3 + Element Plus + Pinia + TypeScript
- **Database**: PostgreSQL with EF Core (snake_case naming)
- **Auth**: JWT (8-hour expiry)
- **Real-time**: SignalR (Hub at /hubs/mes)
- **Cache**: Redis (anti-duplicate submit, batch number generation)

### 架构分层详解

依赖方向严格单向，禁止反向引用：

- **Domain**：零外部依赖，定义 Entity / Enum / ValueObject / Domain Service / Domain Event / 聚合仓储接口
- **Application**：引用 Domain，定义 DTO / Application Service / Interface；禁止引用 Infrastructure / Api
- **Infrastructure**：引用 Domain + Application，实现仓储和基础设施接口；禁止引用 Api
- **Api**：引用 Application + Infrastructure + Integration；禁止直接引用 Domain

### AI 限界上下文

项目包含独立的 AI 限界上下文（`MES.AI.Domain`、`MES.AI.Application`），遵循同样的分层架构：

- **MES.AI.Domain**：AI 领域模型（Prompt、Conversation、Agent 等）
- **MES.AI.Application**：AI 应用服务（调用大模型、构建 Prompt 等）

其他层可以引用 AI 层，就像引用主领域一样：
- Domain 层可引用 `MES.AI.Domain`
- Application 层可引用 `MES.AI.Domain` + `MES.AI.Application`
- Infrastructure 层可实现 AI 层的接口
- Api 层可通过 Application 层间接使用 AI 领域模型

### DDD 重构规则

- 聚合根：WorkOrder / Routing / QcInspection / Bom / Equipment / Material / WorkReport / AndonEvent / Factory
- 充血模型：业务逻辑内聚在实体方法（如 `WorkOrder.Release()`），不用贫血模型
- Controller 不写 try-catch，异常由 ExceptionMiddleware 统一处理
- 异常层次：`DomainException` → 400 / `EntityNotFoundException` → 404 / `ForbiddenException` → 403

### 代码风格

- C#：4 空格缩进、file-scoped namespace、Allman 花括号、`_camelCase` 私有字段
- TypeScript：2 空格缩进、单引号、无分号、`<script setup lang="ts">`
- 详细规范见 `.editorconfig`

### 提交规范

- Git commit message 使用**中文**，主题行不超过 50 字符，祈使语气

### RBAC 角色

Admin / ProductionManager / QualityEngineer / EquipmentEngineer / Operator

Controller 使用 `[Authorize(Roles = "xxx")]` 基于 JWT Claim 做角色授权。

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

## 文档规范

### 文档目录结构

```
docs/
├── 01-项目概述/       # 项目概述、README
├── 02-技术设计/      # 技术方案（DDD方案、接口设计、数据库设计等）
├── 03-项目管理/      # 项目管理文档
│   └── 修复/         # 修复方案和执行计划
├── 04-质量保障/      # 测试计划、CICD 方案
└── 05-AI融合/        # AI 融合方案
```

### 文档创建规则

- **修复类文档**：执行修复任务时，在 `docs/03-项目管理/修复/` 下创建
  - 方案文件：`YYYY-MM-DD-XXX修复方案.md`
  - 计划文件：`YYYY-MM-DD-XXX修复执行计划.md`
- **技术方案**：放在 `docs/02-技术设计/` 下
- **执行计划**：放在 `docs/03-项目管理/` 下，如 `开发计划.md`、`执行计划.md`