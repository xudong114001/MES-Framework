# Prompt 工程完善方案

## 📋 问题总览

项目已有 `AGENTS.md` 作为主入口指令文件，覆盖了架构、RBAC、测试、文档索引等关键信息。但在代码风格统一、分层约束、多工具适配等方面存在缺失，可能导致 AI 编码助手生成不一致或违反架构规则的代码。

| 文件 | 覆盖范围 | 状态 |
|------|----------|------|
| `AGENTS.md` | 架构、模式、RBAC、测试、文档索引、Seed Data | ✅ 已有 |
| `CLAUDE.md` | 转发到 AGENTS.md | ⚠️ 内容薄弱 |
| `.github/workflows/` | CI/CD 流水线 | ✅ 已有 |

---

### P0 — 必须完成

#### Task #1: `.editorconfig` — 统一代码风格

**严重性:** 🔴 严重 - 无统一风格配置

**问题描述:**
- AI Agent 生成代码的缩进、引号、分号等不一致
- C# 和 TypeScript 混用不同风格，难以维护

**方案:**

```editorconfig
# .editorconfig
root = true

# C# 规则
[*.cs]
indent_style = space
indent_size = 4

# TypeScript / Vue 规则
[*.{ts,vue}]
indent_style = space
indent_size = 2
```

**关键规则:**
| 文件类型 | 缩进 | 特殊规则 |
|----------|------|----------|
| C# (.cs) | 4 空格 | `_camelCase` 私有字段，PascalCase 类型/方法 |
| TypeScript (.ts) | 2 空格 | 单引号，无分号 |
| Vue (.vue) | 2 空格 | `<script setup lang="ts">` |
| JSON / YAML | 2 空格 | - |
| Markdown | - | trim_trailing_whitespace = false |
| PowerShell | 4 空格 | - |

**涉及文件:**
- 新建 `.editorconfig`（项目根目录）

**验收:** 主流 IDE 自动识别 `.editorconfig` 配置

---

#### Task #2: `.env.example` — 环境变量模板

**严重性:** 🔴 严重 - `.env` 被 gitignore

**问题描述:**
- 新人/Agent 无法知道需要哪些环境变量
- 启动服务时缺少配置导致失败

**方案:**

```bash
# .env.example
# PostgreSQL
POSTGRES_HOST=mes-postgres
POSTGRES_PASSWORD=          # 必填

# RabbitMQ
RABBITMQ_DEFAULT_PASS=      # 必填

# MES API
JWT_SECRET_KEY=             # 必填：JWT 签名密钥（至少 32 字符）
```

**分组:** PostgreSQL / Redis / RabbitMQ / MES API（JWT）

**涉及文件:**
- 新建 `.env.example`（项目根目录）

**验收:** 复制为 `.env` 并填入真实值后，`docker-compose up -d` 可正常启动

---

### P1 — 推荐完成

#### Task #3: 子项目级 `AGENTS.md` — 分层约束

**严重性:** 🔴 严重 - Agent 违反 Clean Architecture

**问题描述:**
- Agent 修改某层代码时不知道层间依赖规则
- 容易在 Domain 层引用 Infrastructure，或在 Controller 中直接注入 Repository

**方案:**

```
src/MES.Domain/AGENTS.md         # Domain 层专属约束
src/MES.Application/AGENTS.md    # Application 层专属约束
src/MES.Infrastructure/AGENTS.md # Infrastructure 层专属约束
src/MES.Api/AGENTS.md            # Api 层专属约束
```

**各层约束规则:**

| 层级 | 允许引用 | 禁止引用 | 核心约束 |
|------|----------|----------|----------|
| **Domain** | - | Application / Infrastructure / Api | 零外部依赖，只定义 Entity/Enum/ValueObject/Domain Service |
| **Application** | Domain | Infrastructure / Api | DTO 不能暴露 Entity，不直接操作 DbContext |
| **Infrastructure** | Domain / Application | Api | 实现聚合仓储接口，不含业务规则 |
| **Api** | Application / AI.Application / Infrastructure / Integration | 直接引用 Domain | Controller 不写业务逻辑，异常统一抛给 Middleware |

**DDD 规则来源:** `docs/02-技术设计/DDD重构与异常处理方案.md`

**涉及文件:**
- 新建 `src/MES.Domain/AGENTS.md`
- 新建 `src/MES.Application/AGENTS.md`
- 新建 `src/MES.Infrastructure/AGENTS.md`
- 新建 `src/MES.Api/AGENTS.md`

**依赖:** Task #1（代码风格统一后，约束更清晰）

**验收:** Agent 修改 Domain 层时不会添加对 Infrastructure 的 using

---

#### Task #4: `CLAUDE.md` 内容补充

**严重性:** 🟡 中等 - Claude Code 不会自动读引用

**问题描述:**
- 当前仅一行"去看 AGENTS.md"
- Claude Code 不会自动读 AGENTS.md 引用

**方案:**

```
CLAUDE.md 和 AGENTS.md 内容完全一致，无论哪个被加载都能拿到全部规则。
```

**涉及文件:**
- `CLAUDE.md` 与 `AGENTS.md` 同步更新

**验收:** Claude Code 新会话无需额外提示即可遵守分层规则和中文提交规范

---

### P2 — 可选

#### Task #5: 其他 AI 编码工具适配

**严重性:** 🟢 低 - 工具覆盖不全

**问题描述:**
- 只覆盖了 Claude Code 和 Zed(AGENTS.md)
- Cursor / GitHub Copilot 未覆盖

**方案:**

```markdown
# .cursorrules
（架构 + DDD + 代码风格 + 提交规范 + RBAC）
```

```markdown
# .github/copilot-instructions.md
（架构 + DDD + 代码风格 + 提交规范 + RBAC）
```

**涉及文件:**
- 新建 `.cursorrules`
- 新建 `.github/copilot-instructions.md`

**验收:** 对应工具能自动识别并遵守规则

---

#### Task #6: `CONTRIBUTING.md`

**严重性:** 🟢 低 - 无贡献指南

**问题描述:**
- 外部贡献者不清楚流程
- 新人不知道如何搭建开发环境

**方案:**

```markdown
# 贡献指南

## 开发环境
1. 前置依赖：.NET 10 SDK + Node.js 18+ + Docker
2. 启动基础设施：`docker-compose up -d`
3. 环境变量：复制 `.env.example` 为 `.env` 并填入真实值

## 分支策略
- `main` - 生产发布
- `develop` - 开发集成分支
- `feature/*` - 功能开发

## 架构约束
- 遵循 4 层 Clean Architecture（详见 AGENTS.md）
- 遵循 DDD 重构规则（详见 docs/02-技术设计/DDD重构与异常处理方案.md）
```

**涉及文件:**
- 新建 `CONTRIBUTING.md`（项目根目录）

**验收:** 新贡献者按文档可独立完成首次贡献

---

#### Task #7: `.github/CODEOWNERS`

**严重性:** 🟢 低 - PR 无自动指派

**问题描述:**
- 代码审查依赖手动指派
- 容易遗漏或延迟

**方案:**

```markdown
# CODEOWNERS
/src/MES.Domain/       @mes-backend
/src/MES.Application/  @mes-backend
/src/MES.Api/          @mes-backend
/src/mes-web/          @mes-frontend
```

**涉及文件:**
- 新建 `.github/CODEOWNERS`

**验收:** PR 提交后自动请求对应 Owner 审查

---

## 📊 重构依赖关系图

```
P0: Task #1 (.editorconfig)
    ↓
P1: Task #3 (子项目 AGENTS.md) ← Task #4 (CLAUDE.md)
    ↓
P2: Task #5 (工具适配)
    ↓
    Task #6 (CONTRIBUTING.md) + Task #7 (CODEOWNERS)

P0: Task #2 (.env.example) 独立，可并行执行
```

---

## 🚀 执行计划

### 阶段一: 基础配置（P0）

| Task | 内容 | 产出 |
|------|------|------|
| Task #1 | `.editorconfig` | 代码风格统一 |
| Task #2 | `.env.example` | 环境变量模板 |

### 阶段二: 分层约束（P1）

| Task | 内容 | 产出 |
|------|------|------|
| Task #3 | 子项目 `AGENTS.md` × 4 | 各层专属约束文件 |
| Task #4 | `CLAUDE.md` 补充 | 关键规则内联 |

### 阶段三: 工具适配与协作（P2）

| Task | 内容 | 产出 |
|------|------|------|
| Task #5 | Cursor + Copilot 适配 | 多工具支持 |
| Task #6 | `CONTRIBUTING.md` | 贡献指南 |
| Task #7 | `CODEOWNERS` | 自动审查指派 |

---

## 📝 重构检查清单

### 基础配置

- [ ] `.editorconfig` 存在且 C#/TS/JSON 规则正确
- [ ] `.env.example` 包含所有必需环境变量

### 分层约束

- [ ] `src/MES.Domain/AGENTS.md` 存在且禁止引用 Application
- [ ] `src/MES.Application/AGENTS.md` 存在且 DTO 规则明确
- [ ] `src/MES.Infrastructure/AGENTS.md` 存在且禁止引用 Api
- [ ] `src/MES.Api/AGENTS.md` 存在且异常处理规则明确
- [ ] Agent 修改 Domain 层时不会添加 Infrastructure 引用

### 工具适配

- [ ] `CLAUDE.md` 内联核心规则
- [ ] `.cursorrules` 存在且格式正确
- [ ] `.github/copilot-instructions.md` 存在且格式正确

### 协作流程

- [ ] `CONTRIBUTING.md` 存在且包含开发环境搭建说明
- [ ] `.github/CODEOWNERS` 存在且路径覆盖所有模块

### 整体验收

- [ ] Agent 能根据 AGENTS.md 生成符合架构规则的代码
- [ ] 各 AI 工具（Claude / Zed / Cursor / Copilot）能识别项目规则
- [ ] 新人可按 CONTRIBUTING.md 完成首次开发环境搭建

---

## 执行记录

| 阶段 | 内容 | 状态 | 产出文件 |
|------|------|------|----------|
| P0 | `.editorconfig` | ✅ 完成 | `.editorconfig` |
| P0 | `.env.example` | ✅ 完成 | `.env.example` |
| P1 | 子项目 `AGENTS.md` × 4 | ✅ 完成 | `src/MES.Domain/AGENTS.md`、`src/MES.Application/AGENTS.md`、`src/MES.Infrastructure/AGENTS.md`、`src/MES.Api/AGENTS.md` |
| P1 | `CLAUDE.md` 补充 | ✅ 完成 | `CLAUDE.md` |
| P2 | Cursor + Copilot 适配 | ✅ 完成 | `.cursorrules`、`.github/copilot-instructions.md` |
| P2 | `CONTRIBUTING.md` | ✅ 完成 | `CONTRIBUTING.md` |
| P2 | `CODEOWNERS` | ✅ 完成 | `.github/CODEOWNERS` |