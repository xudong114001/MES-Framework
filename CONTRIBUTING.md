# 贡献指南

感谢你对 MES Framework 的关注！以下是参与贡献的流程和规范。

## 开发环境

1. **前置依赖**：.NET 10 SDK + Node.js 18+ + Docker
2. **启动基础设施**：`docker-compose up -d`
3. **启动后端**：`cd src/MES.Api; dotnet run`
4. **启动前端**：`cd src/mes-web; npm install; npm run dev`
5. **环境变量**：复制 `.env.example` 为 `.env` 并填入真实值

## 分支策略

| 分支 | 用途 |
|------|------|
| `main` | 生产发布，仅通过 PR 合入 |
| `develop` | 开发集成分支 |
| `feature/*` | 功能开发 |
| `fix/*` | 缺陷修复 |
| `refactor/*` | 重构 |

## 提交规范

- Commit message 使用**中文**
- 主题行不超过 50 字符，祈使语气
- 格式：`<类型>: <描述>`，如 `feat: 新增设备保养计划接口`、`fix: 修复工单状态流转异常`

## PR 流程

1. 从 `develop` 创建功能分支
2. 开发 + 编写/更新测试
3. 确保所有测试通过：`dotnet test` + `npm run typecheck`
4. 提交 PR 到 `develop`，填写变更说明
5. 等待 Code Review 通过后合并

## 架构约束

- 遵循 4 层 Clean Architecture，详见 `AGENTS.md`
- 遵循 DDD 重构规则，详见 `docs/02-技术设计/DDD重构与异常处理方案.md`
- Domain 层零外部依赖，Infrastructure 不引用 Api
- Controller 不写 try-catch，异常由 ExceptionMiddleware 统一处理

## 代码风格

- 遵循 `.editorconfig` 配置
- C#：4 空格缩进、file-scoped namespace、Allman 花括号
- TypeScript：2 空格缩进、单引号、无分号
