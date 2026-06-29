<template>
  <div class="big-screen">
    <!-- 全屏切换按钮 -->
    <el-button class="fullscreen-btn" @click.stop="toggleFullscreen" circle>
      <el-icon><FullScreen /></el-icon>
    </el-button>

    <!-- 顶部标题 -->
    <header class="screen-header">
      <h1 class="screen-title">MES 生产运营看板</h1>
      <div class="screen-time">{{ currentTime }}</div>
    </header>

    <!-- 主体内容：两列布局 -->
    <div class="screen-body">
      <!-- 左列 -->
      <div class="screen-left">
        <!-- 今日产量 -->
        <div class="screen-panel output-panel">
          <div class="panel-title">
            <el-icon><TrendCharts /></el-icon> 今日产量统计
          </div>
          <div class="output-numbers">
            <div class="output-item">
              <span class="output-label">计划产量</span>
              <span class="output-value planned">{{ outputStats.plannedQty }}</span>
            </div>
            <div class="output-item">
              <span class="output-label">完成产量</span>
              <span class="output-value completed">{{ outputStats.completedQty }}</span>
            </div>
            <div class="output-item">
              <span class="output-label">报废数量</span>
              <span class="output-value scrap">{{ outputStats.scrapQty }}</span>
            </div>
          </div>
          <!-- 进度条 -->
          <div class="output-progress">
            <div class="progress-label">完成进度</div>
            <el-progress
              :percentage="outputPercentage"
              :stroke-width="24"
              :color="progressColor"
              :text-inside="true"
            />
          </div>
        </div>

        <!-- 工单状态分布 -->
        <div class="screen-panel">
          <div class="panel-title">
            <el-icon><DataAnalysis /></el-icon> 工单状态分布
          </div>
          <div class="status-list">
            <div v-for="item in statusDistribution" :key="item.status" class="status-bar-item">
              <div class="status-bar-label">
                <span>{{ statusLabel(item.status) }}</span>
                <span>{{ item.count }} 单</span>
              </div>
              <el-progress
                :percentage="statusPercent(item.count)"
                :color="statusColor(item.status)"
                :stroke-width="18"
              />
            </div>
            <div v-if="statusDistribution.length === 0" class="empty-text">暂无数据</div>
          </div>
        </div>
      </div>

      <!-- 右列 -->
      <div class="screen-right">
        <!-- 设备 OEE 一览 -->
        <div class="screen-panel">
          <div class="panel-title">
            <el-icon><Monitor /></el-icon> 设备 OEE 一览
          </div>
          <div class="oee-grid">
            <div v-for="item in equipmentStatus" :key="item.status" class="oee-card">
              <div class="oee-card-title">{{ equipLabel(item.status) }}</div>
              <div class="oee-card-value" :style="{ color: equipOeeColor(item.status) }">
                {{ item.count }}
              </div>
              <div class="oee-card-label">台设备</div>
            </div>
            <div v-if="equipmentStatus.length === 0" class="empty-text">暂无数据</div>
          </div>
        </div>

        <!-- 今日工单概览 -->
        <div class="screen-panel">
          <div class="panel-title">
            <el-icon><Document /></el-icon> 今日工单概览
          </div>
          <div class="order-cards">
            <div class="order-card total-order">
              <div class="order-num">{{ todayStats.total }}</div>
              <div class="order-desc">今日工单</div>
            </div>
            <div class="order-card pending-order">
              <div class="order-num">{{ todayStats.pending }}</div>
              <div class="order-desc">待下达</div>
            </div>
            <div class="order-card progress-order">
              <div class="order-num">{{ todayStats.inProgress }}</div>
              <div class="order-desc">生产中</div>
            </div>
            <div class="order-card done-order">
              <div class="order-num">{{ todayStats.completed }}</div>
              <div class="order-desc">已完成</div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 底部异常滚动 -->
    <div class="screen-footer">
      <div class="footer-title">
        <el-icon><WarningFilled /></el-icon>
        <span>最新异常事件</span>
        <el-tag v-if="activeAndonCount > 0" type="danger" size="small" effect="dark">
          活跃 {{ activeAndonCount }}
        </el-tag>
      </div>
      <div class="andon-scroll">
        <div class="andon-scroll-content" :class="{ 'has-events': activeAndonEvents.length > 0 }">
          <div
            v-for="evt in activeAndonEvents"
            :key="evt.id"
            class="andon-marquee-item"
          >
            <el-tag :type="eventTagType(evt.eventType)" size="small" effect="dark">
              {{ eventTypeLabel(evt.eventType) }}
            </el-tag>
            <span class="andon-desc">{{ evt.description || '未描述' }}</span>
            <span class="andon-workstation">{{ evt.workstationName || '未知工位' }}</span>
            <span class="andon-time">{{ formatTime(evt.occurredAt) }}</span>
          </div>
          <div v-if="activeAndonEvents.length === 0" class="andon-empty">暂无异常事件</div>
        </div>
      </div>
    </div>

    <!-- 连接状态 -->
    <div class="screen-status">
      <el-tag :type="signalrConnected ? 'success' : 'danger'" size="small" effect="dark">
        {{ signalrConnected ? '● 实时' : '○ 离线' }}
      </el-tag>
      <span class="refresh-time">数据更新: {{ lastRefreshTime }}</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { dashboardApi } from '../../api/dashboard'
import { andonApi } from '../../api/andon'
import signalrService from '../../utils/signalr'
import { TrendCharts, DataAnalysis, Monitor, Document, WarningFilled, FullScreen } from '@element-plus/icons-vue'

// ============ 数据状态 ============
const todayStats = ref({ total: 0, pending: 0, inProgress: 0, completed: 0, cancelled: 0 })
const outputStats = ref({ plannedQty: 0, completedQty: 0, scrapQty: 0 })
const statusDistribution = ref<{ status: string; count: number }[]>([])
const equipmentStatus = ref<{ status: string; count: number }[]>([])
const activeAndonEvents = ref<any[]>([])
const activeAndonCount = ref(0)
const signalrConnected = ref(false)
const currentTime = ref('')
const lastRefreshTime = ref('')

let timer: number | null = null
let clockTimer: number | null = null

// ============ 计算属性 ============
const outputPercentage = computed(() => {
  const planned = outputStats.value.plannedQty
  if (planned <= 0) return 0
  return Math.round((outputStats.value.completedQty / planned) * 100)
})

const progressColor = computed(() => {
  const pct = outputPercentage.value
  if (pct >= 80) return '#67c23a'
  if (pct >= 50) return '#e6a23c'
  return '#409eff'
})

// ============ 辅助函数 ============
const statusLabel = (s: string) => {
  const map: Record<string, string> = {
    PENDING: '待下达', RELEASED: '已下达', IN_PROGRESS: '生产中',
    COMPLETED: '已完成', CLOSED: '已关闭', CANCELLED: '已取消', ON_HOLD: '暂停'
  }
  return map[s] || s
}

const statusColor = (s: string) => {
  const map: Record<string, string> = {
    PENDING: '#909399', RELEASED: '#409eff', IN_PROGRESS: '#e6a23c',
    COMPLETED: '#67c23a', CLOSED: '#909399', CANCELLED: '#f56c6c', ON_HOLD: '#f56c6c'
  }
  return map[s] || '#409eff'
}

const equipLabel = (s: string) => {
  const map: Record<string, string> = {
    RUNNING: '运行中', IDLE: '空闲', MAINTENANCE: '保养中', BROKEN: '故障'
  }
  return map[s] || s
}

const equipOeeColor = (s: string) => {
  const map: Record<string, string> = {
    RUNNING: '#67c23a', IDLE: '#909399', MAINTENANCE: '#e6a23c', BROKEN: '#f56c6c'
  }
  return map[s] || '#909399'
}

const statusPercent = (count: number) => {
  const total = statusDistribution.value.reduce((a, b) => a + b.count, 0)
  return total > 0 ? Math.round((count / total) * 100) : 0
}

const eventTagType = (type: string) => {
  const map: Record<string, string> = {
    QUALITY_ALARM: 'warning',
    EQUIPMENT_FAULT: 'danger',
    MATERIAL_SHORTAGE: 'info',
    OTHER: 'default'
  }
  return map[type] || 'default'
}

const eventTypeLabel = (type: string) => {
  const map: Record<string, string> = {
    QUALITY_ALARM: '质量异常',
    EQUIPMENT_FAULT: '设备故障',
    MATERIAL_SHORTAGE: '物料短缺',
    OTHER: '其他异常'
  }
  return map[type] || type
}

function formatTime(ts: string) {
  if (!ts) return '-'
  const d = new Date(ts)
  return d.toLocaleString('zh-CN', { timeZone: 'Asia/Shanghai', hour12: false })
    .replace(/\//g, '-')
}

function updateClock() {
  const now = new Date()
  currentTime.value = now.toLocaleString('zh-CN', {
    timeZone: 'Asia/Shanghai',
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
    hour12: false
  }).replace(/\//g, '-')
}

function nowStr() {
  return new Date().toLocaleString('zh-CN', {
    timeZone: 'Asia/Shanghai',
    hour12: false
  })
}

// ============ 数据加载 ============
async function loadAllData() {
  try {
    const [todayRes, statusRes, outputRes, equipRes, andonRes] = await Promise.all([
      dashboardApi.todayOrders(),
      dashboardApi.orderStatus(),
      dashboardApi.output(),
      dashboardApi.equipment(),
      andonApi.active()
    ])
    todayStats.value = todayRes.data
    statusDistribution.value = statusRes.data || []
    outputStats.value = outputRes.data
    equipmentStatus.value = equipRes.data || []
    activeAndonEvents.value = andonRes.data || []
    activeAndonCount.value = (andonRes.data || []).length
    lastRefreshTime.value = nowStr()
  } catch {
    // handled by interceptor
  }
}

// ============ SignalR 事件 ============
function handleOutputUpdate(data: any) {
  if (data) {
    outputStats.value.completedQty = (outputStats.value.completedQty || 0) + (data.goodQty || 0)
    outputStats.value.scrapQty = (outputStats.value.scrapQty || 0) + (data.scrapQty || 0)
    lastRefreshTime.value = nowStr()
  }
}

function handleOrderUpdate() {
  loadAllData()
}

function handleAndonEvent(data: any) {
  if (data) {
    activeAndonEvents.value.unshift(data)
    activeAndonCount.value++
    // 最多保留 50 条
    if (activeAndonEvents.value.length > 50) {
      activeAndonEvents.value = activeAndonEvents.value.slice(0, 50)
    }
  }
}

// ============ 全屏 ============
function toggleFullscreen() {
  if (!document.fullscreenElement) {
    document.documentElement.requestFullscreen().catch(() => {})
  } else {
    document.exitFullscreen().catch(() => {})
  }
}

// ============ 生命周期 ============
onMounted(async () => {
  // 时钟
  updateClock()
  clockTimer = window.setInterval(updateClock, 1000)

  // 加载数据
  await loadAllData()

  // 连接 SignalR
  try {
    await signalrService.connect()
    signalrConnected.value = signalrService.connected
  } catch {
    signalrConnected.value = false
  }

  // 订阅事件
  signalrService.on('onOutputUpdate', handleOutputUpdate)
  signalrService.on('onOrderUpdate', handleOrderUpdate)
  signalrService.on('onAndonEvent', handleAndonEvent)

  // 30 秒自动刷新 fallback
  timer = window.setInterval(async () => {
    signalrConnected.value = signalrService.connected
    // 如果 SignalR 断开，用 HTTP fallback
    if (!signalrService.connected) {
      await loadAllData()
    }
  }, 30000)
})

onUnmounted(() => {
  signalrService.off('onOutputUpdate', handleOutputUpdate)
  signalrService.off('onOrderUpdate', handleOrderUpdate)
  signalrService.off('onAndonEvent', handleAndonEvent)

  if (timer) {
    clearInterval(timer)
    timer = null
  }
  if (clockTimer) {
    clearInterval(clockTimer)
    clockTimer = null
  }
})
</script>

<style scoped>
.big-screen {
  width: 100%;
  min-height: 100vh;
  background: #0a1a2f;
  color: #fff;
  padding: 16px 24px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  font-family: 'Microsoft YaHei', 'PingFang SC', sans-serif;
}

/* ===== 全屏按钮 ===== */
.fullscreen-btn {
  position: fixed;
  top: 16px;
  right: 16px;
  z-index: 1000;
  opacity: 0.7;
}

.fullscreen-btn:hover {
  opacity: 1;
}

/* ===== 顶部标题 ===== */
.screen-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 0 16px;
  border-bottom: 2px solid rgba(64, 158, 255, 0.3);
}

.screen-title {
  font-size: 28px;
  font-weight: bold;
  background: linear-gradient(90deg, #409eff, #67c23a);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  margin: 0;
  letter-spacing: 4px;
}

.screen-time {
  font-size: 22px;
  color: #bfcbd9;
  font-family: 'Consolas', monospace;
}

/* ===== 主体布局 ===== */
.screen-body {
  display: flex;
  gap: 16px;
  flex: 1;
}

.screen-left {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.screen-right {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

/* ===== 面板通用 ===== */
.screen-panel {
  background: rgba(255, 255, 255, 0.05);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 12px;
  padding: 20px;
  backdrop-filter: blur(10px);
}

.panel-title {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 16px;
  font-weight: bold;
  color: #e6e6e6;
  margin-bottom: 16px;
  padding-bottom: 12px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.08);
}

/* ===== 产量统计 ===== */
.output-numbers {
  display: flex;
  justify-content: space-around;
  margin-bottom: 16px;
}

.output-item {
  text-align: center;
}

.output-label {
  display: block;
  font-size: 13px;
  color: #909399;
  margin-bottom: 8px;
}

.output-value {
  display: block;
  font-size: 36px;
  font-weight: bold;
  font-family: 'Consolas', monospace;
}

.output-value.planned { color: #409eff; }
.output-value.completed { color: #67c23a; }
.output-value.scrap { color: #f56c6c; }

.output-progress {
  margin-top: 8px;
}

.progress-label {
  font-size: 13px;
  color: #909399;
  margin-bottom: 8px;
}

/* ===== 工单状态 ===== */
.status-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.status-bar-item {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.status-bar-label {
  display: flex;
  justify-content: space-between;
  font-size: 13px;
  color: #bfcbd9;
}

/* ===== OEE 网格 ===== */
.oee-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
}

.oee-card {
  background: rgba(255, 255, 255, 0.03);
  border-radius: 10px;
  padding: 16px;
  text-align: center;
  border: 1px solid rgba(255, 255, 255, 0.06);
}

.oee-card-title {
  font-size: 14px;
  color: #909399;
  margin-bottom: 8px;
}

.oee-card-value {
  font-size: 32px;
  font-weight: bold;
  font-family: 'Consolas', monospace;
}

.oee-card-label {
  font-size: 12px;
  color: #606266;
  margin-top: 4px;
}

/* ===== 工单概览卡片 ===== */
.order-cards {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
}

.order-card {
  text-align: center;
  padding: 14px;
  border-radius: 10px;
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid rgba(255, 255, 255, 0.06);
}

.order-num {
  font-size: 30px;
  font-weight: bold;
  font-family: 'Consolas', monospace;
}

.order-desc {
  font-size: 13px;
  color: #909399;
  margin-top: 6px;
}

.total-order .order-num { color: #409eff; }
.pending-order .order-num { color: #e6a23c; }
.progress-order .order-num { color: #67c23a; }
.done-order .order-num { color: #909399; }

/* ===== 底部异常滚动 ===== */
.screen-footer {
  background: rgba(255, 255, 255, 0.05);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 12px;
  padding: 12px 20px;
  backdrop-filter: blur(10px);
}

.footer-title {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
  color: #bfcbd9;
  margin-bottom: 8px;
}

.andon-scroll {
  overflow: hidden;
  height: 40px;
  position: relative;
}

.andon-scroll-content {
  display: flex;
  gap: 24px;
  animation: marquee 20s linear infinite;
  white-space: nowrap;
}

.andon-scroll-content.has-events {
  animation: marquee 20s linear infinite;
}

.andon-marquee-item {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 6px 16px;
  background: rgba(245, 108, 108, 0.1);
  border-radius: 6px;
  font-size: 13px;
  white-space: nowrap;
}

.andon-desc {
  color: #e6e6e6;
  max-width: 200px;
  overflow: hidden;
  text-overflow: ellipsis;
}

.andon-workstation {
  color: #909399;
}

.andon-time {
  color: #606266;
  font-family: 'Consolas', monospace;
  font-size: 12px;
}

.andon-empty {
  color: #606266;
  padding: 8px 0;
}

@keyframes marquee {
  0% { transform: translateX(100%); }
  100% { transform: translateX(-200%); }
}

/* ===== 底部状态 ===== */
.screen-status {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 12px;
  padding: 8px 0;
  font-size: 12px;
  color: #606266;
}

.refresh-time {
  color: #606266;
}

/* ===== 空状态 ===== */
.empty-text {
  text-align: center;
  color: #606266;
  padding: 16px 0;
}

/* ===== 深色主题 Progress 适配 ===== */
.screen-panel :deep(.el-progress__text) {
  color: #e6e6e6 !important;
}

.screen-panel :deep(.el-progress-bar__outer) {
  background-color: rgba(255, 255, 255, 0.08);
}
</style>
