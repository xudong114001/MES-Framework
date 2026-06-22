# MES Domain - 限界上下文命名空间演进计划

## 概述

当前 Domain 层采用扁平的按类型分组方式（Entities / Events / Enums / Repositories / Services），
未来将演进为按**业务限界上下文 (Bounded Context)** 分组，以更好地体现 DDD 设计理念。

## 目标命名空间结构

```
MES.Domain/
├── WorkOrderManagement/           # 工单管理
│   ├── Entities/                  #   WorkOrder, WorkOrderStep
│   │   └── WorkOrder.cs
│   │   └── WorkOrderStep.cs
│   └── Events/                    #   WorkOrderCreatedEvent, WorkOrderStatusChangedEvent
│       └── WorkOrderCreatedEvent.cs
│       └── WorkOrderStatusChangedEvent.cs
│
├── QualityManagement/             # 质量管理
│   ├── Entities/                  #   QcInspection, QcInspectionItem, QcCheckpoint
│   └── Events/                    #   QcInspectionCompletedEvent
│
├── EquipmentManagement/           # 设备管理
│   ├── Entities/                  #   Equipment, MaintenancePlan
│   └── Services/                  #   EquipmentDomainService
│
├── SchedulingManagement/          # 排程管理
│   ├── Entities/                  #   (预留)
│   └── Services/                  #   SchedulingDomainService
│
├── MaterialManagement/            # 物料管理
│   ├── Entities/                  #   Material, Bom, MaterialTrace
│   └── Events/                    #   MaterialInventoryUpdatedEvent
│
├── WorkshopManagement/            # 车间管理
│   ├── Entities/                  #   Factory, Workshop, ProductionLine, Workstation
│
├── Reporting/                     # 报工管理
│   ├── Entities/                  #   WorkReport
│   └── Events/                    #   WorkReportSubmittedEvent
│
├── AndonManagement/               # 安灯管理
│   └── Entities/                  #   AndonEvent
│
├── UserManagement/                # 用户管理
│   ├── Entities/                  #   User, Role, UserRole, RolePermission
│
├── RoutingManagement/             # 工艺路线管理
│   ├── Entities/                  #   Routing, RoutingStep
│   └── Repositories/              #   IRoutingRepository
│
├── Shared/                        # 共享内核 (保留在 Domain 根目录)
│   ├── AggregateRoots/            #   IAggregateRoot
│   ├── Entities/                  #   BaseEntity
│   ├── Events/                    #   DomainEvent
│   ├── Exceptions/                #   DomainException, EntityNotFoundException, etc.
│   ├── Interfaces/                #   ICacheService, IBatchNumberService
│   ├── ValueObjects/              #   Quantity
│   └── Enums/                     #   所有枚举
```

## 当前状态与目标对照

| 当前位置 | 目标命名空间 | 限界上下文 |
|---------|-------------|-----------|
| `Entities/WorkOrder.cs` | `WorkOrderManagement.Entities` | 工单管理 |
| `Entities/WorkOrderStep.cs` | `WorkOrderManagement.Entities` | 工单管理 |
| `Entities/QcInspection.cs` | `QualityManagement.Entities` | 质量管理 |
| `Entities/QcInspectionItem.cs` | `QualityManagement.Entities` | 质量管理 |
| `Entities/QcCheckpoint.cs` | `QualityManagement.Entities` | 质量管理 |
| `Entities/Equipment.cs` | `EquipmentManagement.Entities` | 设备管理 |
| `Entities/MaintenancePlan.cs` | `EquipmentManagement.Entities` | 设备管理 |
| `Entities/Material.cs` | `MaterialManagement.Entities` | 物料管理 |
| `Entities/Bom.cs` | `MaterialManagement.Entities` | 物料管理 |
| `Entities/MaterialTrace.cs` | `MaterialManagement.Entities` | 物料管理 |
| `Entities/Factory.cs` | `WorkshopManagement.Entities` | 车间管理 |
| `Entities/Workshop.cs` | `WorkshopManagement.Entities` | 车间管理 |
| `Entities/ProductionLine.cs` | `WorkshopManagement.Entities` | 车间管理 |
| `Entities/Workstation.cs` | `WorkshopManagement.Entities` | 车间管理 |
| `Entities/WorkReport.cs` | `Reporting.Entities` | 报工管理 |
| `Entities/AndonEvent.cs` | `AndonManagement.Entities` | 安灯管理 |
| `Entities/User.cs` | `UserManagement.Entities` | 用户管理 |
| `Entities/Role.cs` | `UserManagement.Entities` | 用户管理 |
| `Entities/UserRole.cs` | `UserManagement.Entities` | 用户管理 |
| `Entities/RolePermission.cs` | `UserManagement.Entities` | 用户管理 |
| `Entities/Routing.cs` | `RoutingManagement.Entities` | 工艺路线 |
| `Entities/RoutingStep.cs` | `RoutingManagement.Entities` | 工艺路线 |
| `Events/WorkOrderCreatedEvent.cs` | `WorkOrderManagement.Events` | 工单管理 |
| `Events/WorkOrderStatusChangedEvent.cs` | `WorkOrderManagement.Events` | 工单管理 |
| `Events/QcInspectionCompletedEvent.cs` | `QualityManagement.Events` | 质量管理 |
| `Events/MaterialInventoryUpdatedEvent.cs` | `MaterialManagement.Events` | 物料管理 |
| `Events/WorkReportSubmittedEvent.cs` | `Reporting.Events` | 报工管理 |
| `Services/EquipmentDomainService.cs` | `EquipmentManagement.Services` | 设备管理 |
| `Services/SchedulingDomainService.cs` | `SchedulingManagement.Services` | 排程管理 |
| `Services/WorkOrderDomainService.cs` | `WorkOrderManagement.Services` | 工单管理 |

## 迁移步骤（未来执行）

1. **逐个限界上下文迁移**：每次只迁移一个限界上下文，确保编译通过
2. **更新命名空间**：将 `namespace MES.Domain.Entities` 改为 `namespace MES.Domain.<BoundedContext>.Entities`
3. **更新所有引用**：使用 IDE 的重命名命名空间功能，更新 Application / Infrastructure / Api 层的 using 语句
4. **移动文件**：将文件从旧目录移动到新目录
5. **删除空目录**：确认旧目录为空后删除
6. **运行测试**：每迁移一个限界上下文后运行完整测试套件

## 注意事项

- Enums、Exceptions、ValueObjects 等共享概念保留在 `Shared/` 或 Domain 根目录
- 跨限界上下文的引用应通过接口或领域事件解耦
- 每个限界上下文内可包含 Entities / Events / Repositories / Services / Enums（上下文私有枚举）
- 迁移期间，旧目录和新目录可能暂时共存
