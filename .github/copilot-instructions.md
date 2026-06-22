# MES Framework - GitHub Copilot Instructions

## 项目概述

MES 制造执行系统，.NET 10 后端 + Vue 3 前端，4 层 Clean Architecture。

## 架构分层

- **Domain**：零外部依赖，Entity / Enum / ValueObject / Domain Service / Domain Event / 聚合仓储接口
- **Application**：引用 Domain，DTO / Service / Interface；禁止引用 Infrastructure / Api
- **Infrastructure**：引用 Domain + Application，实现仓储和基础设施接口；禁止引用 Api
- **Api**：引用 Application + Infrastructure + Integration；禁止直接引用 Domain

## DDD 规则

- 聚合根：WorkOrder / Routing / QcInspection / Bom / Equipment / Material / WorkReport / AndonEvent / Factory
- 充血模型：业务逻辑内聚在实体方法，不用贫血模型
- Controller 不写 try-catch，由 ExceptionMiddleware 统一处理
- 异常层次：DomainException → 400 / EntityNotFoundException → 404 / ForbiddenException → 403

## 代码风格

- C#：4 空格缩进、file-scoped namespace、Allman 花括号、`_camelCase` 私有字段
- TypeScript：2 空格缩进、单引号、无分号、`<script setup lang="ts">`
- 详细规范见 `.editorconfig`

## 提交规范

- Git commit message 使用中文

## RBAC 角色

Admin / ProductionManager / QualityEngineer / EquipmentEngineer / Operator

## 详细文档

修改代码前先读 `AGENTS.md`，按需查阅 `docs/` 目录下的设计文档。
