# MES Framework 性能测试

本目录包含使用 [k6](https://k6.io/) 实现的 MES 系统性能测试脚本。

## 目录结构

```
tests/performance/
├── lib.js                    # 共享工具库（登录、请求封装等）
├── basic-workflow.js         # 基础业务流程测试（登录→工单列表→报工）
├── work-report.js            # 报工接口压测（目标 100 TPS）
├── dashboard.js              # 看板数据查询性能测试
└── README.md                 # 本文档
```

## k6 安装

### Windows（使用 winget）

```powershell
winget install k6 --source winget
```

### Windows（使用 Chocolatey）

```powershell
choco install k6
```

### macOS（使用 Homebrew）

```bash
brew install k6
```

### Linux（使用 apt）

```bash
sudo gpg -k
sudo gpg --no-default-keyring --keyring /usr/share/keyrings/k6-archive-keyring.gpg --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys C5AD17C747E3415A3642D57D77C6C491D6AC1D69
echo "deb [signed-by=/usr/share/keyrings/k6-archive-keyring.gpg] https://dl.k6.io/deb stable main" | sudo tee /etc/apt/sources.list.d/k6.list
sudo apt-get update
sudo apt-get install k6
```

### 验证安装

```bash
k6 version
```

## 运行测试

### 前置条件

1. **启动后端服务**：
   ```powershell
   cd src/MES.Api; dotnet run
   # 或使用 Docker 启动基础设施
   docker-compose up -d
   ```

2. **确认服务可访问**：
   - Backend API: http://localhost:5180
   - Swagger: http://localhost:5180/swagger

### 执行测试

#### 1. 基础业务流程测试（10 VU，30 秒）

```bash
# 使用 npm 脚本
npm run test:perf:basic

# 或直接使用 k6
k6 run tests/performance/basic-workflow.js
```

**测试场景**：
- 用户登录 → 获取工单列表 → PDA 扫码报工

**性能阈值**：
- P95 响应时间 < 500ms
- 错误率 < 1%

#### 2. 报工接口压测（目标 100 TPS，1 分钟）

```bash
npm run test:perf:report
# 或
k6 run tests/performance/work-report.js
```

**测试场景**：
- PDA 扫码报工（70 TPS）
- 报工列表查询（30 TPS）

**性能阈值**：
- P95 响应时间 < 1000ms
- 错误率 < 1%

#### 3. 看板数据查询测试（5 VU，30 秒）

```bash
npm run test:perf:dashboard
# 或
k6 run tests/performance/dashboard.js
```

**测试场景**：
- 今日工单统计
- 工单状态分布
- 产量统计
- 设备状态

**性能阈值**：
- P95 响应时间 < 500ms
- 错误率 < 1%

#### 4. 运行所有测试

```bash
npm run test:perf
# 或
k6 run tests/performance/
```

### 可选：InfluxDB + Grafana 实时监控

如需实时查看性能指标，可启动 InfluxDB 并使用 `--out` 参数：

```bash
# 1. 启动 InfluxDB（使用 Docker）
docker run -d --name influxdb -p 8086:8086 -v influxdb-data:/var/lib/influxdb2 influxdb:latest

# 2. 创建 k6 数据库（首次登录后执行）
# 访问 http://localhost:8086 创建 bucket "k6"

# 3. 运行测试并输出到 InfluxDB
npm run test:perf:monitor
# 或
k6 run tests/performance/ --out influxdb=http://localhost:8086/k6
```

**Grafana 配置**：
1. 访问 http://localhost:3000（默认账号 admin/admin）
2. 添加 InfluxDB 数据源（URL: http://influxdb:8086）
3. 导入 k6 官方 Dashboard：https://grafana.com/grafana dashboards/2587

## 环境变量

| 变量 | 默认值 | 说明 |
|------|--------|------|
| `BASE_URL` | http://localhost:5180 | API 基础地址 |

使用示例：

```bash
BASE_URL=http://localhost:5180 k6 run tests/performance/basic-workflow.js
```

## 性能阈值说明

| 指标 | 阈值 | 说明 |
|------|------|------|
| P95 响应时间 | 500ms / 1000ms | 95% 请求的响应时间 |
| 错误率 | < 1% | HTTP 4xx/5xx 占比 |
| TPS | 100（报工场景） | 每秒请求数 |

## 常见问题

### Q: k6 报 "import not supported"

**A**: k6 脚本默认使用 ES Modules，但运行时可能需要 `--兼容模式` 或确保 `package.json` 包含 `"type": "module"`。本项目的 `package.json` 已配置。

### Q: 测试失败，提示 "connect ECONNREFUSED"

**A**: 确保后端服务已启动并监听在 5180 端口：

```powershell
curl http://localhost:5180/api/v1/auth/login -X POST -H "Content-Type: application/json" -d '{"username":"admin","password":"Admin@2026!"}'
```

### Q: 如何调整 VU 或持续时间？

**A**: 在脚本中修改 `export const options`，或在命令行覆盖：

```bash
k6 run tests/performance/basic-workflow.js --vus 20 --duration 1m
```

### Q: 报工测试提示 "工单不存在" 或 "报工数量为 0"

**A**: 这是预期行为——测试会尝试用现有工单报工，若工单已完成会返回业务错误（HTTP 400），但不会被视为性能阈值失败。

## 扩展阅读

- [k6 官方文档](https://k6.io/docs/)
- [k6 JavaScript API](https://k6.io/docs/javascript-api/)
- [k6 最佳实践](https://k6.io/docs/testing-guides/best-practices/)