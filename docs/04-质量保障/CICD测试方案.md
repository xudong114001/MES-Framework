# MES Framework CI/CD 完整测试方案

> 版本：1.0
> 日期：2026-06-09
> 状态：已完成

---

## 1. 整体测试架构

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                              CI Pipeline (Push/PR)                             │
├─────────────────────────────────────────────────────────────────────────────────┤
│                                                                                 │
│   ┌──────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐                │
│   │  代码     │ →  │  编译    │ →  │  单元    │ →  │  前端    │                │
│   │  提交     │    │  构建    │    │  测试    │    │  构建    │                │
│   └──────────┘    └──────────┘    └──────────┘    └──────────┘                │
│                                                                                 │
│   .github/workflows/ci.yml                                                     │
│   ├── dotnet restore/build/test                                               │
│   ├── npm install/build                                                       │
│   └── npm run typecheck (TypeScript)                                          │
│                                                                                 │
└─────────────────────────────────────────────────────────────────────────────────┘
                                        ↓
┌─────────────────────────────────────────────────────────────────────────────────┐
│                              CD Pipeline (Main Branch)                         │
├─────────────────────────────────────────────────────────────────────────────────┤
│                                                                                 │
│   ┌──────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐    ┌────────┐  │
│   │  Docker  │ →  │  推送    │ →  │  部署    │ →  │  健康    │ →  │ E2E    │  │
│   │  镜像    │    │  镜像    │    │  到测试   │    │  检查    │    │ 验证   │  │
│   └──────────┘    └──────────┘    └──────────┘    └──────────┘    └────────┘  │
│                                                                                 │
│   .github/workflows/cd.yml                                                     │
│   ├── docker-compose -f docker-compose.prod.yml up -d                        │
│   ├── Health Check: http://localhost:5180/api/health                         │
│   └── tests/e2e-test.ps1                                                       │
│                                                                                 │
└─────────────────────────────────────────────────────────────────────────────────┘
```

---

## 2. CI Pipeline 详解

### 2.1 流程图

```
┌────────────────────────────────────────────────────────────────────────┐
│                         CI Pipeline Jobs                                │
├────────────────────────────────────────────────────────────────────────┤
│                                                                        │
│  Job: backend-build                                                   │
│  ├── Checkout code                                                    │
│  ├── Setup .NET 10.0                                                  │
│  ├── dotnet restore                                                   │
│  ├── dotnet build --no-restore                                        │
│  └── dotnet test --no-build (101 tests)                               │
│                                                                        │
│  Job: frontend-build (depends on: backend-build)                     │
│  ├── Checkout code                                                    │
│  ├── Setup Node.js 20                                                 │
│  ├── npm install                                                      │
│  ├── npm run build                                                    │
│  └── npm run typecheck (TypeScript)                                   │
│                                                                        │
│  Job: security-scan (optional)                                        │
│  ├── dotnet nuget locals http-cache-storage --clear                   │
│  └── OWASP dependency check                                           │
│                                                                        │
└────────────────────────────────────────────────────────────────────────┘
```

### 2.2 CI 配置文件

**文件位置**: `.github/workflows/ci.yml`

```yaml
name: CI

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  backend-build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      
      - name: Cache NuGet packages
        uses: actions/cache@v4
        with:
          path: ~/.nuget/packages
          key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
          restore-keys: |
            ${{ runner.os }}-nuget-
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Build
        run: dotnet build --no-restore --configuration Release
      
      - name: Run Unit Tests
        run: dotnet test --no-build --configuration Release --verbosity normal

  frontend-build:
    runs-on: ubuntu-latest
    needs: backend-build
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'
          cache: 'npm'
          cache-dependency-path: src/mes-web/package-lock.json
      
      - name: Install dependencies
        working-directory: src/mes-web
        run: npm ci
      
      - name: TypeScript type check
        working-directory: src/mes-web
        run: npm run typecheck
      
      - name: Build for production
        working-directory: src/mes-web
        run: npm run build
```

---

## 3. CD Pipeline 详解

### 3.1 流程图

```
┌────────────────────────────────────────────────────────────────────────┐
│                         CD Pipeline Jobs                                │
├────────────────────────────────────────────────────────────────────────┤
│                                                                        │
│  Job: build-and-deploy                                                │
│  ├── Checkout code                                                    │
│  ├── Build Docker images                                              │
│  │   ├── mes-api (ASP.NET Core)                                       │
│  │   └── mes-web (Nginx + Vue)                                        │
│  ├── Push to registry (optional)                                      │
│  ├── Deploy via docker-compose                                        │
│  │   └── docker-compose.prod.yml                                      │
│  └── Wait for services ready                                          │
│                                                                        │
│  Job: smoke-tests (depends on: build-and-deploy)                     │
│  ├── Health check API                                                 │
│  ├── Health check Web                                                 │
│  └── Run E2E tests                                                    │
│                                                                        │
└────────────────────────────────────────────────────────────────────────┘
```

### 3.2 CD 配置文件

**文件位置**: `.github/workflows/cd.yml`

```yaml
name: CD

on:
  push:
    branches: [main]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    steps:
      - uses: actions/checkout@v4
      
      - name: Build Docker images
        run: |
          docker build -t mes-api:${{ github.sha }} -f src/MES.Api/Dockerfile .
          docker build -t mes-web:${{ github.sha }} -f src/mes-web/Dockerfile .
      
      - name: Start services
        run: docker-compose -f docker-compose.prod.yml up -d
      
      - name: Wait for API
        run: |
          echo "Waiting for API to be ready..."
          sleep 30
          for i in {1..30}; do
            if curl -f http://localhost:5180/api/v1/dashboard/orders/today >/dev/null 2>&1; then
              echo "API is ready"
              exit 0
            fi
            echo "Waiting... ($i/30)"
            sleep 5
          done
          echo "API failed to start"
          exit 1

  smoke-tests:
    runs-on: ubuntu-latest
    needs: build-and-deploy
    steps:
      - name: Run E2E tests
        run: |
          powershell -File tests/e2e-test.ps1
      
      - name: Run AI tests
        run: |
          powershell -File tests/ai-test.ps1
```

---

## 4. 测试阶段详解

### 4.1 阶段一：单元测试 (CI)

| 测试类型 | 工具 | 用例数 | 执行时机 | 超时 |
|----------|------|--------|----------|------|
| 后端单元测试 | xUnit + Moq | 101 | 每次 PR | 5 min |
| AI 单元测试 | xUnit + Moq | 189 | 每次 PR | 3 min |
| 前端单元测试 | Vitest | 规划中 | 每次 PR | 2 min |

**执行命令**:
```powershell
# 后端单元测试
dotnet test src/MES.Tests/MES.Tests.csproj --configuration Release

# AI 测试（已包含在上面）
dotnet test src/MES.Tests/MES.Tests.csproj --filter "FullyQualifiedName~AI"
```

### 4.2 阶段二：构建验证 (CI)

| 检查项 | 工具 | 失败处理 |
|--------|------|----------|
| .NET 编译 | dotnet build | 阻止合并 |
| TypeScript 类型检查 | npm run typecheck | 阻止合并 |
| 前端生产构建 | npm run build | 阻止合并 |

### 4.3 阶段三：API 冒烟测试 (CD)

| 测试��� | 端点 | 预期响应 | 超时 |
|--------|------|----------|------|
| API 健康 | GET /api/v1/dashboard/orders/today | 200 OK | 10s |
| 前端健康 | GET / | 200 OK | 10s |
| 数据库连接 | - | 无异常 | 5s |

### 4.4 阶段四：E2E 测试 (CD)

| 测试脚本 | 覆盖范围 | 用例数 | 超时 |
|----------|----------|--------|------|
| e2e-test.ps1 | API 端到端 | 29 | 5 min |
| ai-test.ps1 | AI 服务 | 62 | 3 min |
| postman/run-api-tests.ps1 | Postman 集合 | 60+ | 5 min |

---

## 5. 测试矩阵

### 5.1 CI 阶段测试矩阵

| 阶段 | 测试类型 | 触发条件 | 失败处理 |
|------|----------|----------|----------|
| L1 | 代码格式检查 | PR | 警告 |
| L2 | 单元测试 | PR | 阻止合并 |
| L3 | 构建验证 | PR | 阻止合并 |
| L4 | 前端构建 | PR | 阻止合并 |

### 5.2 CD 阶段测试矩阵

| 阶段 | 测试类型 | 触发条件 | 失败处理 |
|------|----------|----------|----------|
| D1 | Docker 构建 | Main branch | 回滚 |
| D2 | 部署验证 | Main branch | 回滚 |
| D3 | 健康检查 | Main branch | 回滚 |
| D4 | E2E 测试 | Main branch | 告警 |

---

## 6. 测试执行脚本

### 6.1 E2E 测试脚本

**文件**: `tests/e2e-test.ps1`

覆盖范围：
- ✅ 认证登录
- ✅ 基础数据 CRUD（工厂/车间/产线/工位/物料/BOM/工艺）
- ✅ 工单生命周期（PENDING → RELEASED → SCHEDULED → IN_PROGRESS → COMPLETED → CLOSED）
- ✅ 报工（PDA 扫码报工）
- ✅ 质检（创建/判定/处理不合格品）
- ✅ 看板数据
- ✅ 追溯查询
- ✅ 排产管理
- ✅ Andon 异常

**执行命令**:
```powershell
powershell -File tests/e2e-test.ps1
```

### 6.2 AI 测试脚本

**文件**: `tests/ai-test.ps1`

覆盖范围：
- ✅ 质量预警（3 条规则）
- ✅ 智能排程建议
- ✅ 设备健康分析

**执行命令**:
```powershell
powershell -File tests/ai-test.ps1
```

### 6.3 集成测试

**文件**: `src/MES.Integration.Tests/`

覆盖范围：
- ✅ ERP 适配器（Mock + SAP B1 接口）
- ✅ WMS 适配器
- ✅ PLC 采集器（Mock + Modbus）
- ✅ 事件总线

**执行命令**:
```powershell
dotnet test src/MES.Integration.Tests/MES.Integration.Tests.csproj
```

### 6.4 前端 UI 测试

**文件**: `src/mes-web/e2e/`

覆盖范围：
- ✅ 登录页面
- ✅ 工单列表/创建/详情
- ✅ 报工页面
- ✅ 质检页面
- ✅ 集成页面

**执行命令**:
```powershell
cd src/mes-web
npx playwright test
```

### 6.5 性能测试

**文件**: `tests/k6/`

覆盖场景：
| 场景 | 文件 | 目标 TPS | 响应时间 |
|------|------|----------|----------|
| 并发报工 | work-report-scenario.js | 50 | P95 < 500ms |
| 工单列表 | work-order-list.js | 100 | P95 < 300ms |
| 看板刷新 | dashboard-refresh.js | 30 | P95 < 800ms |
| Redis 防重 | redis-anti-duplicate.js | 200 | P95 < 50ms |

**执行命令**:
```powershell
# 报工压测
k6 run tests/k6/work-report-scenario.js --vus 50 --duration 60s

# 列表查询压测
k6 run tests/k6/work-order-list.js --vus 100 --duration 60s
```

---

## 7. 环境配置

### 7.1 GitHub Secrets

| Secret 名称 | 用途 | 必需 |
|-------------|------|------|
| DOCKER_USERNAME | Docker Hub 用户名 | 否 |
| DOCKER_PASSWORD | Docker Hub 密码 | 否 |
| DEPLOY_HOST | 部署服务器 IP | 否 |
| DEPLOY_SSH_KEY | SSH 私钥 | 否 |

### 7.2 GitHub Variables

| 变量名称 | 默认值 | 用途 |
|----------|--------|------|
| DOTNET_VERSION | 10.0.x | .NET 版本 |
| NODE_VERSION | 20.x | Node.js 版本 |
| API_PORT | 5180 | API 端口 |
| WEB_PORT | 80 | Web 端口 |

---

## 8. 质量门禁

### 8.1 CI 门禁

```
✅ 代码提交 → 编译通过 → 单元测试通过 → 前端构建成功 → 代码审查 → 合并
```

| 门禁条件 | 说明 |
|----------|------|
| 单元测试通过率 | 100% (290/290) |
| 代码覆盖率 | ≥ 80% |
| 构建状态 | 0 错误 |
| TypeScript 检查 | 0 错误 |

### 8.2 CD 门禁

```
✅ Main 分支 → Docker 构建 → 部署 → 健康检查 → E2E 通过 → 监控
```

| 门禁条件 | 说明 |
|----------|------|
| Docker 构建 | 成功 |
| 服务启动 | 30 秒内 |
| 健康检查 | HTTP 200 |
| E2E 测试 | 100% 通过 |

---

## 9. 告警配置

### 9.1 失败告警

| 场景 | 告警方式 | 接收人 |
|------|----------|--------|
| CI 构建失败 | GitHub Actions | 代码提交者 |
| CD 部署失败 | GitHub Actions + Email | DevOps |
| E2E 测试失败 | GitHub Actions + Email | 测试负责人 |
| 生产环境异常 | 监控系统 | 运维值班 |

### 9.2 告警阈值

| 指标 | 阈值 | 告警级别 |
|------|------|----------|
| 单元测试失败 | ≥ 1 | P0 |
| 构建失败 | = 1 | P0 |
| E2E 失败率 | > 5% | P1 |
| API 响应时间 | P95 > 2s | P2 |
| CPU 使用率 | > 80% | P2 |

---

## 10. 完整执行流程

### 10.1 开发流程

```
┌──────────────────────────────────────────────────────────────────────────┐
│                           开发流程                                        │
├──────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  1. 本地开发                                                             │
│     ├── 修改代码                                                         │
│     ├── dotnet build (本地验证)                                         │
│     └── npm run typecheck (本地验证)                                    │
│                                                                          │
│  2. 提交代码                                                             │
│     ├── git commit -m "feat: xxx"                                       │
│     └── git push origin feature/xxx                                     │
│                                                                          │
│  3. 创建 PR                                                              │
│     ├── GitHub 创建 Pull Request                                        │
│     └── 触发 CI Pipeline                                                │
│                                                                          │
│  4. CI 验证 ⏳                                                           │
│     ├── ✅ .NET 编译                                                     │
│     ├── ✅ 单元测试 (101 + 189 = 290 tests)                             │
│     ├── ✅ TypeScript 检查                                              │
│     └── ✅ 前端构建                                                      │
│                                                                          │
│  5. 代码审查                                                             │
│     ├── ✅ 代码审查通过                                                  │
│     └── ✅ Squash merge 到 main                                         │
│                                                                          │
│  6. CD 部署 ⏳                                                           │
│     ├── ✅ Docker 镜像构建                                               │
│     ├── ✅ 部署到测试环境                                                │
│     ├── ✅ 健康检查                                                      │
│     └── ✅ E2E 测试 (29/29)                                             │
│                                                                          │
│  7. 完成 ✅                                                              │
│     └── 部署成功，监控中                                                 │
│                                                                          │
└──────────────────────────────────────────────────────────────────────────┘
```

### 10.2 执行时间预估

| 阶段 | 预计时间 | 累计 |
|------|----------|------|
| CI - .NET 编译 | ~30s | 30s |
| CI - 单元测试 | ~2min | 2.5min |
| CI - 前端构建 | ~1min | 3.5min |
| CD - Docker 构建 | ~3min | 6.5min |
| CD - 部署 | ~30s | 7min |
| CD - E2E 测试 | ~5min | 12min |

**总计**: 约 12 分钟完成 CI + CD

---

## 11. 附录

### 11.1 相关文件

| 文件 | 说明 |
|------|------|
| `.github/workflows/ci.yml` | CI 工作流配置 |
| `.github/workflows/cd.yml` | CD 工作流配置 |
| `tests/e2e-test.ps1` | E2E 测试脚本 |
| `tests/ai-test.ps1` | AI 测试脚本 |
| `tests/k6/` | 性能测试脚本 |
| `src/mes-web/e2e/` | 前端 UI 测试 |
| `src/MES.Tests/` | 单元测试项目 |
| `src/MES.Integration.Tests/` | 集成测试项目 |

### 11.2 相关文档

| 文档 | 说明 |
|------|------|
| `docs/07-测试计划.md` | 详细测试计划 |
| `docs/05-开发计划.md` | 开发计划 |
| `docs/04-项目架构.md` | 系统架构 |

### 11.3 常用命令

```powershell
# 本地运行单元测试
dotnet test src/MES.Tests/MES.Tests.csproj

# 本地运行 E2E 测试（需先启动服务）
powershell -File tests/e2e-test.ps1

# 本地运行 AI 测试
powershell -File tests/ai-test.ps1

# 本地运行前端构建检查
cd src/mes-web
npm run typecheck
npm run build

# 本地运行性能测试
k6 run tests/k6/work-report-scenario.js --vus 50 --duration 60s
```