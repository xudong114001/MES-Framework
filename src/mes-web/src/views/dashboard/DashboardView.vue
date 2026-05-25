<template>
  <div class="dashboard">
    <h2 class="dashboard-welcome">欢迎，{{ authStore.user?.displayName || authStore.user?.username }}</h2>

    <!-- 今日工单统计 + 产量统计行 -->
    <el-row :gutter="20" class="dashboard-cards">
      <el-col :span="6">
        <el-card shadow="hover">
          <div class="stat-card">
            <el-icon :size="40" color="#409eff"><Document /></el-icon>
            <div class="stat-info">
              <span class="stat-value">{{ todayStats.total }}</span>
              <span class="stat-label">今日工单</span>
            </div>
          </div>
          <div class="stat-detail">
            <span>待下达: {{ todayStats.pending }}</span>
            <span>生产中: {{ todayStats.inProgress }}</span>
            <span>已完成: {{ todayStats.completed }}</span>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover">
          <div class="stat-card">
            <el-icon :size="40" color="#67c23a"><TrendCharts /></el-icon>
            <div class="stat-info">
              <span class="stat-value">{{ outputStats.plannedQty }}</span>
              <span class="stat-label">计划产量</span>
            </div>
          </div>
          <div class="stat-detail">
            <span>完成: <span class="highlight-number">{{ outputStats.completedQty }}</span></span>
            <span>报废: {{ outputStats.scrapQty }}</span>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover">
          <div class="stat-card">
            <el-icon :size="40" color="#e6a23c"><CircleCheck /></el-icon>
            <div class="stat-info">
              <span class="stat-value">{{ completedRate }}</span>
              <span class="stat-label">完成率</span>
            </div>
          </div>
          <div class="stat-detail">
            <span>完成 {{ todayStats.completed }} / 总计 {{ todayStats.total }}</span>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover">
          <div class="stat-card">
            <el-icon :size="40" color="#f56c6c"><WarningFilled /></el-icon>
            <div class="stat-info">
              <span class="stat-value">{{ activeAndonCount }}</span>
              <span class="stat-label">异常告警</span>
            </div>
          </div>
          <div class="stat-detail">
            <span>活跃异常事件</span>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- 工单状态分布饼图 + 设备状态饼图行 -->
    <el-row :gutter="20" class="dashboard-cards">
      <el-col :span="12">
        <el-card shadow="hover">
          <template #header>
            <span>工单状态分布</span>
          </template>
          <v-chart class="dashboard-chart" :option="orderStatusOption" autoresize />
        </el-card>
      </el-col>
      <el-col :span="12">
        <el-card shadow="hover">
          <template #header>
            <span>设备状态</span>
          </template>
          <v-chart class="dashboard-chart" :option="equipmentStatusOption" autoresize />
        </el-card>
      </el-col>
    </el-row>

    <!-- 实时推送状态指示器 -->
    <div class="signalr-status">
      <el-tag :type="signalrConnected ? 'success' : 'warning'" size="small" effect="plain">
        <span v-if="signalrConnected">● 实时连接正常</span>
        <span v-else>● 实时连接已断开</span>
      </el-tag>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useAuthStore } from '../../stores/auth'
import { dashboardApi } from '../../api/dashboard'
import { andonApi } from '../../api/andon'
import signalrService from '../../utils/signalr'
import { Document, TrendCharts, CircleCheck, WarningFilled } from '@element-plus/icons-vue'
import VChart from 'vue-echarts'

const authStore = useAuthStore()

const todayStats = ref({ total: 0, pending: 0, inProgress: 0, completed: 0, cancelled: 0 })
const outputStats = ref({ plannedQty: 0, completedQty: 0, scrapQty: 0 })
const statusDistribution = ref<{ status: string; count: number }[]>([])
const equipmentStatus = ref<{ status: string; count: number }[]>([])
const activeAndonCount = ref(0)
const signalrConnected = ref(false)

const completedRate = computed(() => {
  const total = todayStats.value.total
  if (total <= 0) return '0%'
  return ((todayStats.value.completed / total) * 100).toFixed(1) + '%'
})

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

const equipColor = (s: string) => {
  const map: Record<string, string> = {
    RUNNING: '#67c23a', IDLE: '#909399', MAINTENANCE: '#e6a23c', BROKEN: '#f56c6c'
  }
  return map[s] || '#909399'
}

const statusPercent = (count: number) => {
  const total = statusDistribution.value.reduce((a, b) => a + b.count, 0)
  return total > 0 ? Math.round((count / total) * 100) : 0
}

const equipPercent = (count: number) => {
  const total = equipmentStatus.value.reduce((a, b) => a + b.count, 0)
  return total > 0 ? Math.round((count / total) * 100) : 0
}

// ---- ECharts 图表配置 ----

const orderStatusOption = computed(() => {
  const data = statusDistribution.value.map(item => ({
    name: statusLabel(item.status),
    value: item.count,
    itemStyle: { color: statusColor(item.status) }
  }))
  return {
    tooltip: {
      trigger: 'item',
      formatter: '{b}: {c} ({d}%)'
    },
    legend: {
      bottom: 0,
      type: 'scroll'
    },
    series: [{
      type: 'pie',
      radius: ['35%', '65%'],
      center: ['50%', '45%'],
      avoidLabelOverlap: true,
      itemStyle: {
        borderRadius: 6,
        borderColor: '#fff',
        borderWidth: 2
      },
      label: {
        show: true,
        formatter: '{b}\n{d}%'
      },
      emphasis: {
        label: { show: true, fontSize: 16, fontWeight: 'bold' }
      },
      data: data.length > 0 ? data : [{ name: '暂无数据', value: 1, itemStyle: { color: '#dcdfe6' } }]
    }]
  }
})

const equipmentStatusOption = computed(() => {
  const data = equipmentStatus.value.map(item => ({
    name: equipLabel(item.status),
    value: item.count,
    itemStyle: { color: equipColor(item.status) }
  }))
  return {
    tooltip: {
      trigger: 'item',
      formatter: '{b}: {c} ({d}%)'
    },
    legend: {
      bottom: 0,
      type: 'scroll'
    },
    series: [{
      type: 'pie',
      radius: ['35%', '65%'],
      center: ['50%', '45%'],
      avoidLabelOverlap: true,
      itemStyle: {
        borderRadius: 6,
        borderColor: '#fff',
        borderWidth: 2
      },
      label: {
        show: true,
        formatter: '{b}\n{d}%'
      },
      emphasis: {
        label: { show: true, fontSize: 16, fontWeight: 'bold' }
      },
      data: data.length > 0 ? data : [{ name: '暂无数据', value: 1, itemStyle: { color: '#dcdfe6' } }]
    }]
  }
})

// SignalR 事件处理器
function handleOutputUpdate(data: any) {
  // 收到实时产量推送，增加产量计数
  if (data) {
    outputStats.value.completedQty = (outputStats.value.completedQty || 0) + (data.goodQty || 0)
    outputStats.value.scrapQty = (outputStats.value.scrapQty || 0) + (data.scrapQty || 0)
  }
}

function handleOrderUpdate() {
  // 收到工单更新通知，刷新工单数据
  loadData()
}

function handleAndonEvent() {
  // 收到异常事件，刷新异常计数
  loadAndonCount()
}

async function loadData() {
  try {
    const [todayRes, statusRes, outputRes, equipRes] = await Promise.all([
      dashboardApi.todayOrders(),
      dashboardApi.orderStatus(),
      dashboardApi.output(),
      dashboardApi.equipment()
    ])
    todayStats.value = todayRes.data
    statusDistribution.value = statusRes.data || []
    outputStats.value = outputRes.data
    equipmentStatus.value = equipRes.data || []
  } catch {
    // errors handled by interceptor
  }
}

async function loadAndonCount() {
  try {
    const andonRes = await andonApi.active()
    activeAndonCount.value = (andonRes.data || []).length
  } catch {
    // handled by interceptor
  }
}

// 检查 SignalR 连接状态
let signalrStatusTimer: number | null = null

function startSignalrStatusCheck() {
  signalrStatusTimer = window.setInterval(() => {
    signalrConnected.value = signalrService.connected
  }, 3000)
}

onMounted(async () => {
  await loadData()
  await loadAndonCount()

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

  startSignalrStatusCheck()
})

onUnmounted(() => {
  signalrService.off('onOutputUpdate', handleOutputUpdate)
  signalrService.off('onOrderUpdate', handleOrderUpdate)
  signalrService.off('onAndonEvent', handleAndonEvent)

  if (signalrStatusTimer) {
    clearInterval(signalrStatusTimer)
    signalrStatusTimer = null
  }
})
</script>

<style scoped>
.dashboard {
  padding: 20px;
}

.dashboard-welcome {
  font-size: 22px;
  color: #303133;
  margin-bottom: 24px;
}

.dashboard-cards {
  margin-bottom: 20px;
}

.stat-card {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 12px;
}

.stat-info {
  display: flex;
  flex-direction: column;
}

.stat-value {
  font-size: 28px;
  font-weight: bold;
  color: #303133;
}

.stat-label {
  font-size: 14px;
  color: #909399;
  margin-top: 4px;
}

.stat-detail {
  display: flex;
  justify-content: space-between;
  font-size: 12px;
  color: #909399;
  border-top: 1px solid #f0f0f0;
  padding-top: 10px;
}

.dashboard-chart {
  width: 100%;
  height: 300px;
}

.dist-item {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 12px;
}

.dist-label {
  width: 60px;
  font-size: 13px;
  color: #606266;
  flex-shrink: 0;
}

.dist-item .el-progress {
  flex: 1;
}

.empty-text {
  text-align: center;
  color: #c0c4cc;
  padding: 30px 0;
}

.highlight-number {
  color: #67c23a;
  font-weight: bold;
}

.signalr-status {
  position: fixed;
  bottom: 16px;
  right: 16px;
  z-index: 1000;
}
</style>
