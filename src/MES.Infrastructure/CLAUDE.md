# MES.Infrastructure — 基础设施层 Agent 指令

## 层级定位

基础设施层实现技术关注点：数据库访问、缓存、消息队列、外部服务集成。是"怎么做"的实现层。

## 依赖规则

| 规则 | 说明 |
|------|------|
| ✅ 允许 | 引用 `MES.Domain`、`MES.Application` |
| ✅ 允许 | 引用 `MES.AI.Domain`、`MES.AI.Application`（实现 AI 层的接口） |
| ✅ 允许 | 引用 EF Core、Redis、RabbitMQ 等技术包 |
| ✅ 允许 | 实现 Application 层定义的接口（`ICacheService`、`IBatchNumberService` 等） |
| ✅ 允许 | 实现聚合仓储接口（`IWorkOrderRepository` 等） |
| ❌ 禁止 | 引用 `MES.Api` |
| ❌ 禁止 | 定义业务规则或领域逻辑 |
| ❌ 禁止 | 在 Repository 实现中包含业务校验逻辑 |

## 基础设施规则

- **聚合仓储**：每个聚合根一个仓储实现，内部使用 `.Include()` 加载聚合内部实体
- **DbContext**：表名使用 `mes_` 前缀 + snake_case，列名使用 snake_case
- **DI 注册**：在 `ServiceCollectionExtensions.cs` 中统一注册，按接口→实现配对
- **数据库迁移**：新增实体时需更新 `MesDbContext` 的 `DbSet` 和 `OnModelCreating`

## 文件组织

```
MES.Infrastructure/
├── Data/               # MesDbContext, DbSet 配置
├── Repositories/       # 仓储实现（聚合仓储 + 通用仓储）
├── Extensions/         # ServiceCollectionExtensions
├── Services/           # ICacheService 等接口实现
└── Migrations/         # EF Core 迁移文件
```
