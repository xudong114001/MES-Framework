# MES Framework - 制造执行系统框架

[![.NET](https://img.shields.io/badge/.NET-10.0-blue)](https://dotnet.microsoft.com/)
[![Vue 3](https://img.shields.io/badge/Vue-3.5+-42b883)](https://vuejs.org/)
[![License](https://img.shields.io/badge/license-Apache_2.0-blue.svg)](LICENSE)

通用、可扩展的 MES 系统基础框架，面向中小型制造企业生产管理。

## 技术栈

| 层级 | 技术选型 |
|------|----------|
| 后端 | .NET 10 + C# 14 + Entity Framework Core 10 |
| 前端 | Vue 3 + TypeScript + Element Plus |
| 数据库 | PostgreSQL 16 + Redis + RabbitMQ |
| 架构 | Clean Architecture (DDD) |

## 目录结构

```
MES-Framework/
├── README.md                   # 项目说明
├── AGENTS.md                   # Agent 开发指引
├── CLAUDE.md                   # Claude Code 开发指引
├── docs/                       # 项目文档
│   ├── 01-项目概述/             # 项目背景、目标、功能模块
│   ├── 02-技术设计/             # 数据库设计、接口规范、架构设计
│   ├── 03-项目管理/             # 开发计划、执行计划、里程碑
│   ├── 04-质量保障/             # 测试策略、用例与质量门禁
│   └── 05-AI融合/              # AI 融合方案
├── src/                        # 源代码
│   ├── MES.Api/                # WebAPI 层（Controller）
│   ├── MES.Application/        # 应用服务层（Service + DTO + Interface + Settings）
│   ├── MES.Domain/             # 领域层（Entity + Enum + ValueObject + DomainEvent）
│   ├── MES.Infrastructure/     # 基础设施层（Repository + 外部服务 + Data/Migrations）
│   ├── MES.Integration/        # 集成层（ERP/WMS/MES 对接 + EventBus）
│   ├── MES.Tests/              # 单元测试 + 集成测试
│   └── mes-web/                # Vue 3 前端
├── database/                   # 数据库脚本
├── tests/                      # E2E 测试 + Postman 集合
├── nginx/                      # Nginx 反向代理配置
└── docker-compose.yml          # Docker 编排
```

## 架构设计

本项目采用 **Clean Architecture (DDD)** 分层架构：

```
Api → Application → Domain ← Infrastructure
     ↑              ↑
     └──────────────┘
```

### 分层规则

- **Domain**: 零外部依赖，仅包含 Entity / Enum / ValueObject / DomainEvent / 聚合仓储接口
- **Application**: 引用 Domain，定义 DTO / Service / Interface；禁止引用 Infrastructure / Api
- **Infrastructure**: 引用 Domain + Application，实现仓储和基础设施接口；禁止引用 Api
- **Api**: 引用 Application + Infrastructure + Integration；禁止直接引用 Domain

### DDD 实践

- 聚合根：`WorkOrder` / `Routing` / `QcInspection` / `Bom` / `Equipment` / `Material` / `WorkReport` / `AndonEvent` / `Factory`
- 充血模型：业务逻辑内聚在实体方法中
- 领域事件：基于 `IDomainEvent` 的事件驱动
- 异常体系：`DomainException` → 400 / `EntityNotFoundException` → 404 / `ForbiddenException` → 403

## 核心功能

| 模块 | 说明 |
|------|------|
| 基础数据管理 | 工厂/车间/产线/工位、物料/BOM、工艺路线、设备/人员 |
| 工单管理 | 工单创建、工单拆分、工单状态机 |
| 排产管理 | 工单排产、排产甘特图、产能计算 |
| 派工管理 | 工单派工到产线/工位、派工单管理 |
| 报工管理 | 工单报工（完工/返工/报废）、工时记录、计件统计 |
| PDA 扫码报工 | PDA 端扫码快速报工、离线缓存 |
| 质量管理 | 来料检、首件检、过程检、完工检、不合格品处理 |
| 质检检查点配置 | 工艺路线质检检查点配置、检验项目定义 |
| 物料追溯 | 批次/序列号追溯、正反向追溯、物料消耗跟踪 |
| 设备管理 | 设备台账、点检保养、故障报修、设备 OEE |
| 设备保养计划 | 保养计划制定与执行、保养提醒 |
| 生产看板 | 工单进度看板、产线实时状态 |
| Andon 异常看板 | 异常报警、Andon 信号管理、异常响应流程 |
| 数据采集 | PLC 数据对接、扫码枪/PDA 采集、IoT 设备接入 |
| 系统管理 | 用户/角色/权限、操作日志、系统配置 |

## 快速开始

1. 环境准备：.NET 10 SDK + Node.js 18+ + PostgreSQL（或 Docker）
2. 启动基础设施：`docker-compose up -d`
3. 启动后端：`cd src/MES.Api; dotnet run`（端口 5180）
4. 启动前端：`cd src/mes-web; npm run dev`（端口 5173）
5. 访问 Swagger：http://localhost:5180/swagger
6. 默认账号：`admin / Admin@2026!`

## 构建状态

```bash
# 编译所有项目
dotnet build src/MES.sln

# 运行测试
dotnet test src/MES.Tests
```

| 项目 | 状态 |
|------|------|
| MES.Domain | ✅ 0 错误 0 警告 |
| MES.Application | ✅ 0 错误 0 警告 |
| MES.Infrastructure | ✅ 0 错误 0 警告 |
| MES.Api | ✅ 0 错误 0 警告 |
| MES.Tests | ✅ 0 错误 0 警告 |

## 文档导航

- [项目概述](docs/01-项目概述/项目概述.md) — 项目背景、目标、功能模块
- [技术设计](docs/02-技术设计/README.md) — 数据库设计、接口规范、架构设计
- [项目管理](docs/03-项目管理/README.md) — 开发计划、执行计划、里程碑
- [质量保障](docs/04-质量保障/README.md) — 测试策略、用例与质量门禁

## 开发状态

| 阶段 | 内容 | 状态 |
|------|------|------|
| P0 | 项目脚手架 + 基础数据管理 | ✅ 已完成 |
| P1 | 工单管理（创建→排产→派工） | ✅ 已完成 |
| P2 | 报工 + 质量管理 | ✅ 已完成 |
| P3 | 追溯 + 设备管理 | ✅ 已完成 |
| P4 | 看板 + Andon | ✅ 已完成 |
| P5 | 接口集成 + 数据采集 | ✅ 已完成 |
| P6 | 性能优化 + 上线部署 | ✅ 已完成 |
| **扩展** | **RBAC 权限/PDA 离线/AI 知识库** | **✅ 已完成** |

> P0-P6 + 非核心扩展已全部完成。运行 `docker-compose up -d` 即可启动全套基础设施。

### 已实现的基础设施

| 组件 | 状态 | 说明 |
|------|------|------|
| Redis | ✅ 已集成 | 防重复报工、批次号生成、分布式缓存 |
| RabbitMQ | ✅ 已集成 | 事件总线（发布/订阅）、自动重连、离线降级 |
| Docker Compose | ✅ 已编排 | PostgreSQL + Redis + RabbitMQ |
| RBAC 权限 | ✅ 已集成 | 5 种预设角色 + JWT Claim + Controller 授权 |

### 测试覆盖

| 类型 | 状态 | 工具 |
|------|------|------|
| 单元测试 | ✅ 289 用例 | xUnit + Moq |
| E2E 测试 | ✅ 58 步骤通过 | PowerShell |
| 集成测试 | ✅ 42 用例 | WebApplicationFactory + TestContainers |
| UI 测试 | ✅ 5 测试文件 | Playwright |
| API 测试 | ✅ 17 用例 | Postman (newman) |


