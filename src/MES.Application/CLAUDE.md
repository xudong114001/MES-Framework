# MES.Application — 应用服务层 Agent 指令

## 层级定位

应用服务层编排业务用例，协调领域对象完成业务流程。是 API 层与领域层之间的桥梁。

## 依赖规则

| 规则 | 说明 |
|------|------|
| ✅ 允许 | 引用 `MES.Domain`、`MES.AI.Domain` |
| ✅ 允许 | 引用 `MES.AI.Application`（AI 限界上下文的应用服务） |
| ✅ 允许 | 定义 DTO、Application Service、Application Interface |
| ✅ 允许 | 注入 Domain 层的聚合仓储接口（`IWorkOrderRepository` 等） |
| ✅ 允许 | 注入领域服务（`WorkOrderDomainService` 等） |
| ❌ 禁止 | 引用 `MES.Infrastructure`、`MES.Api` |
| ❌ 禁止 | DTO 直接暴露 Entity（必须手动映射） |
| ❌ 禁止 | 在 Service 中直接操作 `MesDbContext` |
| ❌ 禁止 | 引用 EF Core、Redis、RabbitMQ 等基础设施包 |

## 应用服务规则

- **应用服务**只做编排：校验输入 → 调用领域方法/领域服务 → 持久化 → 发布事件
- **业务规则**不在 Application 层实现，应下沉到 Domain 层
- **基础设施抽象**：需要缓存、批次号生成等功能时，定义接口（`ICacheService`、`IBatchNumberService`），实现在 Infrastructure 层
- **异常处理**：业务校验失败抛出 `DomainException` 或 `BusinessException`，不要 try-catch 后返回错误码
- **领域事件发布**：通过 `IEventBus` 接口发布，不要直接依赖 RabbitMQ

## DTO 映射

- Entity → DTO：在 Application 层手动映射或使用 Mapster，不要返回 Entity 给 API 层
- DTO → Entity：通过聚合根工厂方法或领域服务创建，不要直接 set 属性
