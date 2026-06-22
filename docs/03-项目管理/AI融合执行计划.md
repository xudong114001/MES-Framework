# MES-AI 融合执行计划

> 基于 [08-AI融合方案.md](08-AI融合方案.md)
> 预计总周期：3周（3个场景全部落地）
> 可独立交付：每周一个场景

---

## 1. 项目总览

### 1.1 执行策略

采用**增量交付**策略：每个场景独立开发、独立测试、独立上线，每周交付一个可用场景。

```
Week 1（第1期）：质量异常智能预警  →  2天出效果，5天完善
Week 2（第2期）：智能排程建议       →  1周开发
Week 3（第3期）：设备预测性维护      →  1周开发
Week 4（可选）：LLM 知识库 / 优化   →  按需
```

### 1.2 里程碑

| 里程碑 | 交付物 | 验收标准 |
|--------|--------|---------|
| M1 - 质量预警 | AI 预警引擎 + 前端卡片 | 当不良率异常时，5秒内推送预警到看板 |
| M2 - 排程建议 | 排程推荐接口 + 前端弹窗 | 新建工单后显示推荐产线，可一键采纳 |
| M3 - 设备预测 | 设备健康度分析 + 维护提醒 | 设备 OEE 下降趋势时，提前3天预警 |
| M4（可选）| LLM 知识库 | 支持自然语言查询工艺标准 |

---

## 2. 第1期：质量异常智能预警（Week 1）

### 2.1 Day 1：环境准备 + 项目创建

#### 后端

| 任务 | 文件 | 内容 |
|------|------|------|
| 创建 AI 领域项目 | `src/MES.AI.Domain/MES.AI.Domain.csproj` | 类库，TargetFramework=net10.0 |
| 创建 AI 应用项目 | `src/MES.AI.Application/MES.AI.Application.csproj` | 类库，依赖 Domain |
| 创建 AI API 项目 | `src/MES.AI.Api/MES.AI.Api.csproj` | 可选，或直接集成到 MES.Api |
| 添加解决方案引用 | `MES.slnx` | 将新项目加入解决方案 |

**推荐**：P1 场景直接将 AI 接口集成在现有 `MES.Api` 中（新增 `AiController.cs`），减少项目数量。

#### 前端

| 任务 | 文件 | 内容 |
|------|------|------|
| 创建 AI 助手组件 | `mes-web/src/components/AiPanel/AiChat.vue` | 右下角浮窗 |
| 创建预警卡片 | `mes-web/src/components/AiPanel/AlertCard.vue` | 弹窗式预警展示 |

#### 设计文档

- [ ] 预警规则配置表设计（见 2.3）
- [ ] AI 接口 API 文档（Swagger 补充）

---

### 2.2 Day 2-3：核心引擎开发

#### 2.2.1 预警规则引擎

**文件**：`src/MES.AI.Application/Services/AiQualityService.cs`

**实现内容**：

```csharp
public class AiQualityService
{
    // 1. 规则配置（可持久化到数据库，或先写死在代码中）
    private readonly List<AlertRule> _rules = new()
    {
        new AlertRule(
            name: "产线不良率飙升",
            condition: "连续3个工单报废率 > 5%",
            level: AlertLevel.HIGH,
            action: "推送到Andon+通知班组长"
        ),
        new AlertRule(
            name: "工位连续返工",
            condition: "某工位连续5次报工有返工",
            level: AlertLevel.MEDIUM,
            action: "推送工艺检查提醒"
        ),
        new AlertRule(
            name: "物料批次异常",
            condition: "某批次在>2个工单中不良率>3%",
            level: AlertLevel.LOW,
            action: "标记重点监控"
        )
    };

    // 2. 分析入口：每次报工完成后调用
    public async Task<List<QualityAlertDto>> AnalyzeAsync(long workOrderId)
    {
        // ① 获取当前工单和同产线最近 N 个工单
        // ② 统计各维度不良率
        // ③ 与规则逐一匹配
        // ④ 返回触发的预警列表
    }
}
```

**关键技术点**：
- [x] 统计查询使用现有 Repository（不新增表）
- [x] 规则匹配完全内存计算（毫秒级）
- [x] 结果通过现有 SignalR 前端推送

#### 2.2.2 触发机制

| 触发方式 | 实现 | 适用场景 |
|----------|------|---------|
| **事件驱动**（推荐）| 报工完成后触发 Analysis | 实时性高，5秒内预警 |
| 定时轮询 | 每 5 分钟扫描一次 | 兜底，防止事件丢失 |

**推荐先实现定时轮询**（简单可靠），后续可升级为事件驱动。

---

### 2.3 Day 4-5：前端集成 + 测试

#### 2.3.1 前端交互

| 组件 | 功能 |
|------|------|
| `AlertCard.vue` | 当收到预警时，自动弹出卡片（类似 Element Plus 的 Notification） |
| `AlertHistory.vue` | 预警历史列表页面（可查看已处理的预警） |

**UI 示例**：

```
┌────────────────────────────────┐
│ ⚠️ 质量异常预警                 │
│                                │
│ 产线：SMT-LINE-01              │
│ 物料：FG-A001                  │
│ 问题：连续3工单不良率 6.2%    │
│ 建议：检查上料工位，建议抽检   │
│                                │
│ [查看详情]  [已处理]           │
└────────────────────────────────┘
```

#### 2.3.2 测试

| 测试项 | 用例数 | 方式 |
|--------|--------|------|
| 规则引擎单元测试 | 10+ | xUnit：模拟各种数据组合 |
| 端到端测试 | 3 | 制造异常数据，验证预警推送 |

---

### 2.4 交付验收

| 验收项 | 验收标准 | 通过条件 |
|--------|---------|---------|
| 功能 | 模拟3个连续高不良率工单，5秒内收到预警 | ✅ |
| 性能 | 单个工单分析 < 100ms | ✅ |
| 日志 | 预警记录持久化到 ai_alert_log 表 | ✅ |

---

## 3. 第2期：智能排程建议（Week 2）

### 3.1 Day 6：需求细化 + 数据准备

#### 3.1.1 分析维度确定

| 维度 | 数据来源 | 权重 |
|------|---------|------|
| 产线当前负荷 | 已排程工单总量 / 产线产能 | 40% |
| 历史完成时间 | 同类工单在该产线的实际耗时 | 30% |
| 交期紧急度 | (计划结束时间 - 当前时间) / 总周期 | 20% |
| 工艺匹配度 | 工单物料的工艺路线与产线能力匹配 | 10% |

#### 3.1.2 数据准备

| 数据 | 来源 | 处理方式 |
|------|------|---------|
| 产线产能 | ProductionLine 表的 throughput 字段 | 直接使用 |
| 历史工单 | WorkOrder 表 + WorkReport 表 | 按产线聚合平均完成时间 |
| 当前负荷 | WorkOrder 表 (SCHEDULED 状态) | 实时查询 |

---

### 3.2 Day 7-9：排程引擎开发

#### 3.2.1 核心算法（简化版）

```csharp
public class AiSchedulingService
{
    public async Task<List<ScheduleSuggestion>> SuggestAsync(long workOrderId)
    {
        // 1. 获取待排程工单信息
        var wo = await _workOrderRepo.GetByIdAsync(workOrderId);
        var material = await _materialRepo.GetByIdAsync(wo.MaterialId);
        
        // 2. 获取所有可用产线
        var lines = await _productionLineRepo.ListAsync(l => l.Status == true);
        
        // 3. 对每个产线评分
        var suggestions = new List<ScheduleSuggestion>();
        foreach (var line in lines)
        {
            var score = 0.0;
            
            // 维度1：负荷率（越低越好）
            var loadRatio = await CalculateLoadRatioAsync(line.Id);
            score += (1 - loadRatio) * 0.4;
            
            // 维度2：历史完成时间匹配度
            var avgDuration = await GetAvgDurationAsync(material.Id, line.Id);
            var timeMatch = 1.0 / (1 + avgDuration / 8.0); // 假设标准时间8h
            score += timeMatch * 0.3;
            
            // 维度3：交期紧急度
            var urgency = CalculateUrgency(wo.PlanEndTime);
            score += urgency * 0.2;
            
            // 维度4：工艺匹配（简单匹配，后续可扩展）
            var routingMatch = wo.RoutingId.HasValue ? 1.0 : 0.5;
            score += routingMatch * 0.1;
            
            suggestions.Add(new ScheduleSuggestion
            {
                LineId = line.Id,
                LineName = line.Name,
                Confidence = score,
                Reason = GenerateReason(score, loadRatio, avgDuration)
            });
        }
        
        // 4. 按得分排序，返回 Top 3
        return suggestions.OrderByDescending(s => s.Confidence).Take(3).ToList();
    }
}
```

> **说明**：算法可后续优化，第一阶段以**简单多因素加权**为主。

#### 3.2.2 接口设计

```http
POST /api/v1/ai/scheduling/suggest
Content-Type: application/json

{ "workOrderId": 123 }

// 返回
{
  "recommendations": [
    {
      "lineId": 2,
      "lineName": "SMT-LINE-02",
      "confidence": 0.87,
      "reason": "当前负荷仅 45%，历史同类工单平均 7.5h"
    },
    {
      "lineId": 1,
      "lineName": "SMT-LINE-01",
      "confidence": 0.72,
      "reason": "当前负荷 68%，但交期更匹配"
    }
  ]
}
```

---

### 3.3 Day 10：前端集成

#### 3.3.1 交互设计

**场景**：调度员在"工单排程"页面点击"获取排程建议"

```
工单：WO-20260606-001
物料：FG-A001，计划数量 100，交期：2026-06-08

[获取排程建议] 

┌──────────────────────────────────────────┐
│ 🤖 AI 排程建议                            │
│                                          │
│ 推荐 1：SMT-LINE-02                   ⭐ │
│ 置信度：87%                              │
│ 预计完成：2026-06-07 16:30             │
│ 理由：当前负荷仅 45%，历史平均 7.5h      │
│ [采纳此方案]                             │
│                                          │
│ 推荐 2：SMT-LINE-01                      │
│ 置信度：72%                              │
│ ...                                      │
│ [采纳此方案]                             │
└──────────────────────────────────────────┘
```

---

### 3.4 交付验收

| 验收项 | 验收标准 | 通过条件 |
|--------|---------|---------|
| 功能 | 对任意待排程工单，返回 ≥1 条建议 | ✅ |
| 性能 | 建议生成 < 500ms | ✅ |
| 交互 | 调度员可一键采纳建议并自动排程 | ✅ |

---

## 4. 第3期：设备预测性维护（Week 3）

### 4.1 Day 11：数据模型准备

#### 4.1.1 新增表（最小化）

```sql
-- 设备健康度快照（每日更新）
CREATE TABLE ai_equipment_health (
    id BIGSERIAL PRIMARY KEY,
    equipment_id BIGINT REFERENCES mes_equipment(id),
    health_score INT NOT NULL CHECK (health_score BETWEEN 0 AND 100),
    oee_trend JSONB,              -- 7日/30日 OEE 趋势数据
    risk_level INT,               -- 0=正常 1=关注 2=预警 3=危险
    next_maintenance_date DATE,   -- 预测下次维护日期
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 健康度趋势索引
CREATE INDEX idx_ai_health_equipment_id ON ai_equipment_health(equipment_id);
CREATE INDEX idx_ai_health_created_at ON ai_equipment_health(created_at);
```

> **注意**：这是整个 AI 方案中**唯一需要新增的表**，其他场景复用现有数据。

---

### 4.2 Day 12-14：预测引擎

#### 4.2.1 核心算法

```csharp
public class AiEquipmentService
{
    // 每日凌晨执行（定时任务）
    public async Task<EquipmentHealthDto> AnalyzeAsync(long equipmentId)
    {
        // 1. 查询最近 30 天 OEE 数据
        var oeeHistory = await _equipmentRepo.GetOeeHistoryAsync(equipmentId, days: 30);
        
        // 2. 计算趋势
        var trend7 = CalculateTrend(oeeHistory.TakeLast(7));
        var trend30 = CalculateTrend(oeeHistory.TakeLast(30));
        
        // 3. 评分逻辑
        var score = 100;
        if (trend7.slope < -0.5) score -= 20;   // 7日内快速下降
        if (trend30.slope < -0.2) score -= 15;  // 30日缓慢下降
        
        var healthData = new EquipmentHealth
        {
            EquipmentId = equipmentId,
            HealthScore = Math.Max(0, score),
            OeeTrend = JsonSerializer.Serialize(new { trend7, trend30 }),
            RiskLevel = score switch
            {
                >= 90 => 0,
                >= 70 => 1,
                >= 50 => 2,
                _ => 3
            }
        };
        
        await _healthRepo.AddAsync(healthData);
        return MapToDto(healthData);
    }
    
    // 简单线性趋势分析（可用 MathNet 替代）
    private (double slope, double rSquared) CalculateTrend(List<OeeData> data)
    {
        var n = data.Count;
        var sumX = Enumerable.Range(0, n).Sum(i => (double)i);
        var sumY = data.Sum(d => d.Oee);
        var sumXY = data.Select((d, i) => i * d.Oee).Sum();
        var sumX2 = Enumerable.Range(0, n).Sum(i => (double)i * i);
        
        var slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
        return (slope, 0.0); // rSquared 可后续补充
    }
}
```

#### 4.2.2 前端展示

```
设备健康度看板

设备：SMT-LINE-01 贴片机
状态：🟡 预警（健康度 62）

趋势：
  可用率 ████████░░ 78% ↓
  性能   █████████░ 85% →
  质量   ████████░░ 75% ↓

预测：
  ⚠️ 未来 3-5 天故障风险较高
  建议：安排周末保养，重点检查吸嘴

[查看维护建议] [安排维护计划]
```

---

### 4.3 交付验收

| 验收项 | 验收标准 | 通过条件 |
|--------|---------|---------|
| 功能 | 每日自动生成健康度报告 | ✅ |
| 预警 | 健康度 < 50 时，推送到设备看板 | ✅ |
| 准确率 | 预测趋势与实际趋势方向一致率 > 60% | ✅（第一期目标） |

---

## 5. 执行节奏

### 5.1 时间线

```
Week 1
  Day 1:  项目创建 + 环境准备
  Day 2-3: 质量预警引擎
  Day 4-5: 前端 + 测试 + 交付

Week 2
  Day 6:   需求细化 + 数据准备
  Day 7-9: 排程引擎开发
  Day 10:  前端集成 + 测试 + 交付

Week 3
  Day 11:  数据模型（新增表）
  Day 12-14: 设备预测引擎 + 前端
  Day 15:  测试 + 交付
```

### 5.2 人力安排

| 角色 | 负责 | 工作量 |
|------|------|--------|
| 后端开发（1人）| AI 引擎 + 接口 | 每天 4-6 小时，3 周完成 |
| 前端开发（0.5人）| AI 组件 + 交互 | 每个场景约 1 天 |
| 测试 | 单元测试 + E2E | 每个场景 0.5 天 |

> **实际**：后端和前端可由同一人串行完成（无复杂的并行依赖）。

---

## 6. 风险与应对

| 风险 | 概率 | 影响 | 应对 |
|------|------|------|------|
| 历史数据不足 | 中 | 预测准确性低 | 先以规则为主，数据积累后升级算法 |
| 工厂不允许联网 | 高 | 无法使用云端 LLM | P1-P3 全部本地实现，无需联网 |
| 性能瓶颈 | 低 | 预警延迟 | 规则引擎纯内存计算，< 100ms |
| 用户不接受 AI 建议 | 中 | 功能废弃 | 设计为"建议"而非"强制"，调度员可忽略 |

---

## 7. 技术规范

### 7.1 命名规范

| 层级 | 命名 | 示例 |
|------|------|------|
| 服务 | `Ai{场景}Service` | `AiQualityService`, `AiSchedulingService` |
| DTO | `{场景}AlertDto` | `QualityAlertDto`, `ScheduleSuggestionDto` |
| 接口 | `IAi{场景}Service` | `IAiQualityService` |
| 前端组件 | `Ai{功能}Card` | `AiAlertCard`, `AiSchedulePanel` |

### 7.2 文件目录规范

```
src/
├── MES.AI.Domain/
│   ├── Entities/
│   │   ├── AiAlertRule.cs
│   │   ├── AiAnomalyRecord.cs
│   │   └── AiEquipmentHealth.cs
│   └── Enums/
│       └── AiAlertLevel.cs
│
├── MES.AI.Application/
│   ├── Services/
│   │   ├── AiQualityService.cs
│   │   ├── AiSchedulingService.cs
│   │   ├── AiEquipmentService.cs
│   │   └── Interfaces/
│   │       ├── IAiQualityService.cs
│   │       ├── IAiSchedulingService.cs
│   │       └── IAiEquipmentService.cs
│   └── Dtos/
│       ├── QualityAlertDto.cs
│       ├── ScheduleSuggestionDto.cs
│       └── EquipmentHealthDto.cs
│
├── MES.Api/
│   └── Controllers/
│       └── AiController.cs    # 集中管理 AI 接口
│
└── mes-web/
    └── src/
        ├── components/AiPanel/
        │   ├── AiChat.vue
        │   ├── AlertCard.vue
        │   ├── ScheduleSuggestion.vue
        │   └── EquipmentHealthWidget.vue
        ├── views/ai/
        │   ├── AlertHistory.vue
        │   └── EquipmentHealthDashboard.vue
        └── api/ai.ts
```

---

## 8. 附录

### 8.1 启动条件

| 条件 | 说明 |
|------|------|
| 现有系统稳定 | MES 核心功能（工单/报工/质检）已正常运行 |
| 数据量 | 至少有 1 周的生产数据（工单、报工记录） |
| 开发环境 | 与现有 MES 开发环境一致（.NET 10 + Vue 3 + PostgreSQL） |
| 网络 | 无特殊要求（P1-P3 全部本地方案） |

### 8.2 验收流程

```
1. 开发人员完成功能 + 自测
2. 演示给业务方（调度员/班组长）
3. 业务方试用 1-2 天，收集反馈
4. 根据反馈调整（规则阈值、UI 细节）
5. 确认无误后，合并到主分支
```

### 8.3 后续优化方向

| 方向 | 说明 | 优先级 |
|------|------|--------|
| 规则配置化 | 将硬编码规则改为数据库配置，用户可自定义阈值 | P1 |
| 算法升级 | 从规则引擎升级为简单 ML（如线性回归预测） | P2 |
| LLM 增强 | 为 P2 排程建议、P3 维护建议，引入 LLM 生成更自然的解释 | P3 |
| 知识库 | 将工艺文档导入向量数据库，支持自然语言查询 | P4 |
| 视觉质检 | 产线部署相机，做 AI 视觉检测 | 待定 |

---

## 9. 关联文档

| 文档 | 说明 |
|------|------|
| [08-AI融合方案.md](08-AI融合方案.md) | 方案设计文档（需求、架构、场景） |
| [04-项目架构.md](04-项目架构.md) | 现有 MES 系统架构 |
| [02-数据库设计.md](02-数据库设计.md) | 现有数据库表结构 |
| [03-接口设计.md](03-接口设计.md) | RESTful 接口规范 |
| [05-开发计划.md](05-开发计划.md) | 原 MES 开发计划（需更新加入 AI 阶段） |
| [07-测试计划.md](07-测试计划.md) | 测试策略（需新增 AI 模块测试用例） |
