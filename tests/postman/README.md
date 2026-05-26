# MES Framework Postman API 测试

本目录包含 MES Framework 的 Postman API 自动化测试集合，使用 Newman（Postman CLI）运行。

## 目录结构

```
tests/postman/
├── MES-Framework.postman_collection.json   # Postman 集合（API 测试用例）
├── mes-local.postman_environment.json      # 本地环境配置
├── run-api-tests.ps1                       # PowerShell 测试运行脚本
├── report.html                             # HTML 测试报告（运行后生成）
└── README.md                               # 本文档
```

## 前置要求

### 1. 安装 Newman

Newman 是 Postman 的命令行工具，用于运行 API 测试。

```powershell
# 使用 npm 全局安装（推荐）
npm install -g newman

# 验证安装
newman --version
```

或者使用 Chocolatey：

```powershell
choco install newman
```

### 2. 启动后端服务

确保 MES Framework 后端服务已启动：

```powershell
# 启动基础设施（PostgreSQL, Redis, RabbitMQ）
docker-compose up -d

# 启动后端（端口 5180）
cd src/MES.Api
dotnet run
```

后端地址：`http://localhost:5180`

## 运行测试

### 方式一：使用 PowerShell 脚本（推荐）

```powershell
# 运行测试并生成 HTML 报告
.\tests\postman\run-api-tests.ps1
```

脚本会自动：
1. 检查 Newman 是否已安装
2. 检查后端服务是否运行
3. 运行所有 API 测试
4. 生成 HTML 测试报告

### 方式二：使用 npm 脚本

```powershell
# 简单运行（仅控制台输出）
npm run test:api

# 运行并生成 HTML 报告
npm run test:api:report
```

### 方式三：直接使用 newman 命令

```powershell
# 进入项目根目录
cd D:\Project\MES-Framework

# 运行测试
newman run tests/postman/MES-Framework.postman_collection.json -e tests/postman/mes-local.postman_environment.json

# 运行并生成 HTML 报告
newman run tests/postman/MES-Framework.postman_collection.json -e tests/postman/mes-local.postman_environment.json -r html --reporter-html-export tests/postman/report.html
```

## 测试覆盖的 API

### 1. 认证 (Auth)
| 测试用例 | 说明 |
|---------|------|
| 1.1 管理员登录 | 验证登���成功并获取 token |
| 1.2 错误密码登录 | 验证错误密码返回 401 |

### 2. 基础数据
| 测试用例 | 说明 |
|---------|------|
| 2.1 获取工厂列表 | 获取所有工厂 |
| 2.2 获取工厂详情 | 根据 ID 获取工厂详情 |
| 2.3 获取车间列表 | 获取所有车间 |
| 2.4 获取产线列表 | 获取所有产线 |
| 2.5 获取工位列表 | 获取所有工位 |
| 2.6 获取物料列表 | 获取所有物料 |
| 2.7 获取工艺路线列表 | 获取所有工艺路线 |
| 2.8 获取 BOM 列表 | 获取所有 BOM |
| 2.9 获取设备列表 | 获取所有设备 |

### 3. 工单管理
| 测试用例 | 说明 |
|---------|------|
| 3.1 获取工单列表 | 获取所有工单，保存不同状态的工单 ID |
| 3.2 获取工单详情 | 根据 ID 获取工单详情 |
| 3.3 创建工单 | 创建新工单 |
| 3.4 下达工单 (Release) | 将 PENDING 工单下达为 RELEASED |
| 3.5 暂停工单 (Hold) | 暂停工单 |
| 3.6 恢复工单 (Resume) | 恢复已暂停的工单 |
| 3.7 工单拆分 (Split) | 拆分工单 |
| 3.8 工单报废 (Scrap) | 报废工单 |
| 3.9 取消工单 (Cancel) | 取消工单 |

### 4. 排产调度
| 测试用例 | 说明 |
|---------|------|
| 4.1 获取可排产工单 | 获取所有已下达但未排产的工��� |
| 4.2 排产 | 将工单分配到指定产线 |
| 4.3 获取产线已排产工单 | 获取指定产线的已排产工单 |
| 4.4 取消排产 | 取消工单的排产 |
| 4.5 自动排产 | 自动排产算法 |
| 4.6 获取排产产线列表 | 获取所有可用产线 |

### 5. 报工管理
| 测试用例 | 说明 |
|---------|------|
| 5.1 获取报工列表 | 获取所有报工记录 |
| 5.2 PDA 扫码报工 | 使用扫码枪报工 |
| 5.3 获取报工详情 | 根据 ID 获取报工详情 |

### 6. 质量管理 (QC)
| 测试用例 | 说明 |
|---------|------|
| 6.1 获取质检单列表 | 获取所有质检单 |
| 6.2 创建质检单 | 创建新质检单 |
| 6.3 判定质检结果 | 判定质检单为 PASS/FAIL |
| 6.4 添加质检项 | 为质检单添加质检项 |
| 6.5 不合格品处理 | 处理不合格品（让步/返工/报废） |
| 6.6 获取今日质检统计 | 获取今日质检统计 |
| 6.7 获取待检列表 | 获取待质检列表 |
| 6.8 获取近期不合格品 | 获取近期不合格品列表 |

### 7. 设备管理
| 测试用例 | 说明 |
|---------|------|
| 7.1 获取设备列表 | 获取所有设备 |
| 7.2 获取设备详情 | 根据 ID 获取设备详情 |
| 7.3 记录保养 | 记录设备保养 |
| 7.4 报修 | 报告设备故障 |
| 7.5 获取设备 OEE | 获取设备 OEE 数据 |
| 7.6 创建保养计划 | 为设备创建保养计划 |
| 7.7 获取保养计划 | 获取设备的保养计划 |
| 7.8 完成保养 | 完成保养计划 |

### 8. 安灯 (Andon)
| 测试用例 | 说明 |
|---------|------|
| 8.1 获取异常事件列表 | 获取所有异常事件 |
| 8.2 触发异常事件 | 触发新的异常事件 |
| 8.3 获取活跃异常 | 获取未处理的异常事件 |
| 8.4 处理异常事件 | 处理异常事件 |

### 9. 追溯
| 测试用例 | 说明 |
|---------|------|
| 9.1 按批次号追溯 | 根据批次号追溯 |
| 9.2 按序列号追溯 | 根据序列号追溯 |
| 9.3 正向追溯 | 追溯物料的流向 |
| 9.4 反向追溯 | 追溯物料的来源 |

### 10. 看板
| 测试用例 | 说明 |
|---------|------|
| 10.1 今日工单统计 | 获取今日工单统计 |
| 10.2 工单状态分布 | 获取工单状态分布 |
| 10.3 产量统计 | 获取产量统计 |
| 10.4 设备状态 | 获取设备状态统计 |

### 11. 调度 (Dispatch)
| 测试用例 | 说明 |
|---------|------|
| 11.1 派工 | 将工序分配到工位 |
| 11.2 取消派工 | 取消派工 |
| 11.3 获取今日任务 | 获取产线今日派工任务 |
| 11.4 获取可选工位 | 获取产线下的可用工位 |

### 12. 质检点配置
| 测试用例 | 说明 |
|---------|------|
| 12.1 获取质检点 | 根据工序获取质检点 |
| 12.2 配置质检点 | 配置新的质检点 |
| 12.3 删除质检点 | 删除质检点配置 |

## 测试报告

运行测试后，HTML 报告将保存在：
- `tests/postman/report.html`

在浏览器中打开报告文件即可查看详细的测试结果。

## 补充说明

- **Postman 集合** vs **E2E 测试**：Postman 适合 API 级别的单元测试，E2E 测试（Playwright）适合端到端场景测试，两者互补。
- **环境变量**：测试使用 `mes-local.postman_environment.json` 中的变量，包括 baseUrl、用户名、密码等。
- **自动 Token**：登录测试会自动将 token 保存到环境变量，后续请求会自动添加 Bearer Token。
- **状态码验证**：每个测试用例都会验证 HTTP 状态码为 200。
- **响应字段验证**：测试会验证响应中包含必要的字段和数据结构。

## 故障排除

### Newman 找不到

```powershell
# 检查 node 和 npm 是否安装
node --version
npm --version

# 重新安装 newman
npm uninstall -g newman
npm install -g newman
```

### 后端连接失败

```powershell
# 检查后端是否运行
curl http://localhost:5180/api/v1/factories
```

### 测试失败

1. 检查后端服务是否正常运行
2. 检查数据库是否已初始化（种子数据）
3. 查看 HTML 报告中的详细错误信息

## 相关文档

- [AGENTS.md](../../AGENTS.md) - 项目总体说明
- [E2E 测试脚本](../../tests/e2e-test.ps1) - PowerShell E2E 测试
- [性能测试](../performance/) - k6 性能测试