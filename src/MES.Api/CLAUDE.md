# MES.Api — WebAPI 层 Agent 指令

## 层级定位

API 层是系统入口，负责 HTTP 请求处理、认证授权、响应格式化。不包含业务逻辑。

## 依赖规则

| 规则 | 说明 |
|------|------|
| ✅ 允许 | 引用 `MES.Application`、`MES.AI.Application`、`MES.Infrastructure`、`MES.Integration` |
| ✅ 允许 | 间接通过 Application 层使用 `MES.Domain`、`MES.AI.Domain` |
| ✅ 允许 | 定义 Controller、Middleware、Hub、Filter |
| ❌ 禁止 | 直接引用 `MES.Domain`（通过 Application 层间接使用） |
| ❌ 禁止 | 在 Controller 中编写业务逻辑 |
| ❌ 禁止 | 在 Controller 中直接注入 `IRepository<T>` 或 `MesDbContext` |

## API 层规则

- **Controller 职责**：接收请求 → 调用 Application Service → 返回 `ApiResponse`
- **不要 try-catch**：异常由 `ExceptionMiddleware` 统一处理，Controller 只抛出异常
- **认证授权**：使用 `[Authorize(Roles = "xxx")]` 基于 JWT Claim 做角色授权
- **路由规范**：`api/v1/{controller-name}`，kebab-case（如 `api/v1/work-orders`）
- **响应格式**：统一使用 `ApiResponse.Ok(data)` / `ApiResponse.Fail(message)`，不直接返回 Entity

## 全局异常处理

- `DomainException` → HTTP 400（业务规则违反）
- `EntityNotFoundException` → HTTP 404
- `ValidationException` → HTTP 400（含字段级错误详情）
- `ForbiddenException` → HTTP 403
- 未处理异常 → HTTP 500（仅记录日志，不暴露堆栈）

## 文件组织

```
MES.Api/
├── Controllers/        # API 控制器
├── Middleware/          # ExceptionMiddleware, AuthMiddleware 等
├── Hubs/               # SignalR Hub
├── Filters/            # Action Filter
└── Program.cs          # DI 注册、中间件配置、Seed Data
```
