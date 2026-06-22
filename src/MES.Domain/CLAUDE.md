# MES.Domain — 领域层 Agent 指令

## 层级定位

领域层是系统核心，包含所有业务规则和领域逻辑。不依赖任何其他项目层。

## 依赖规则

| 规则 | 说明 |
|------|------|
| ✅ 允许 | 定义 Entity、Enum、ValueObject、Domain Service、Domain Event |
| ✅ 允许 | 定义聚合根（实现 `IAggregateRoot`）和聚合仓储接口 |
| ✅ 允许 | 定义领域异常（`DomainException` 及其子类） |
| ❌ 禁止 | 引用 `MES.Application`、`MES.Infrastructure`、`MES.Api` |
| ❌ 禁止 | 引用任何 ORM 框架（EF Core 等） |
| ❌ 禁止 | 引用任何基础设施关注点（Redis、RabbitMQ、HttpClient 等） |
| ⚠️ 注意 | 可引用 `MES.AI.Domain`（AI 限界上下文的领域模型） |

## 聚合根列表

- **聚合根**：`WorkOrder`、`Routing`、`QcInspection`、`Bom`、`Equipment`、`Material`、`WorkReport`、`AndonEvent`、`Factory`
- **聚合内部实体**只能通过聚合根访问，不能有独立仓储
- **充血模型**：业务逻辑内聚在实体方法中（如 `WorkOrder.Release()`、`WorkOrder.Start()`），不使用贫血模型 + Service
- **值对象**：使用 `Quantity`、`Money` 等值对象替代原始类型（`decimal`、`string`）
- **领域事件**：定义在 `Domain/Events/` 下，事件名使用过去时态（如 `WorkOrderCreatedEvent`）
- **领域异常**：使用 `DomainException` 及其子类，不使用 `InvalidOperationException`

## 命名空间

- 当前阶段使用扁平命名空间 `MES.Domain.Entities`、`MES.Domain.Enums` 等
- 长期演进方向：按限界上下文分组（如 `MES.Domain.WorkOrderManagement`），见 DDD 重构方案 Task #9

## 文件组织

```
MES.Domain/
├── AggregateRoots/     # IAggregateRoot 标记接口
├── Entities/           # 实体（聚合根 + 内部实体）
├── Enums/              # 枚举
├── Events/             # 领域事件
├── Exceptions/         # DomainException 层次
├── Repositories/       # 聚合仓储接口
├── Services/           # 领域服务
└── ValueObjects/       # 值对象
```
