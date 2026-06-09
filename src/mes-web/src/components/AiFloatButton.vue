<template>
  <div class="ai-float-container" v-if="showFloat">
    <el-badge :value="alertCount" :hidden="alertCount === 0" :max="99">
      <div class="ai-float-button" @click="togglePanel">
        <el-icon :size="24"><Cpu /></el-icon>
      </div>
    </el-badge>

    <transition name="slide-fade">
      <div v-if="showPanel" class="ai-panel">
        <div class="panel-header">
          <span>AI 智能提醒</span>
          <el-button text @click="showFloat = false">
            <el-icon><Close /></el-icon>
          </el-button>
        </div>
        <div class="panel-content">
          <div v-if="alerts.length === 0" class="empty">
            <el-icon :size="40" color="#67c23a"><CircleCheck /></el-icon>
            <p>暂无新预警</p>
          </div>
          <div v-else class="alert-list">
            <div
              v-for="alert in alerts"
              :key="alert.id"
              class="alert-item"
              :class="'level-' + alert.level.toLowerCase()"
              @click="viewDetail(alert)"
            >
              <div class="alert-title">{{ alert.title }}</div>
              <div class="alert-time">{{ formatTime(alert.createdAt) }}</div>
            </div>
          </div>
        </div>
        <div class="panel-footer">
          <el-button type="primary" link @click="goToAiPage">查看全部</el-button>
        </div>
      </div>
    </transition>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { Cpu, Close, CircleCheck } from '@element-plus/icons-vue'
import { aiApi } from '@/api/ai'

const router = useRouter()
const showFloat = ref(true)
const showPanel = ref(false)
const alerts = ref<any[]>([])
const alertCount = ref(0)
let pollInterval: number | null = null

const togglePanel = () => {
  showPanel.value = !showPanel.value
  if (showPanel.value) {
    fetchAlerts()
  }
}

const fetchAlerts = async () => {
  try {
    const res = await aiApi.getActiveAlerts()
    alerts.value = res.data || []
    alertCount.value = alerts.value.length
  } catch (e) {
    console.error('Failed to fetch alerts:', e)
  }
}

const formatTime = (time: string) => {
  const date = new Date(time)
  const now = new Date()
  const diff = now.getTime() - date.getTime()
  const minutes = Math.floor(diff / 60000)
  if (minutes < 1) return '刚刚'
  if (minutes < 60) return `${minutes}分钟前`
  if (minutes < 1440) return `${Math.floor(minutes / 60)}小时前`
  return `${Math.floor(minutes / 1440)}天前`
}

const viewDetail = (alert: any) => {
  router.push('/ai/dashboard')
}

const goToAiPage = () => {
  router.push('/ai/dashboard')
  showPanel.value = false
}

onMounted(() => {
  fetchAlerts()
  pollInterval = window.setInterval(fetchAlerts, 30000)
})

onUnmounted(() => {
  if (pollInterval) {
    clearInterval(pollInterval)
  }
})
</script>

<style scoped>
.ai-float-container {
  position: fixed;
  bottom: 80px;
  right: 20px;
  z-index: 9999;
}

.ai-float-button {
  width: 50px;
  height: 50px;
  border-radius: 50%;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);
  transition: transform 0.2s, box-shadow 0.2s;
}

.ai-float-button:hover {
  transform: scale(1.1);
  box-shadow: 0 6px 16px rgba(102, 126, 234, 0.6);
}

.ai-panel {
  position: absolute;
  bottom: 60px;
  right: 0;
  width: 320px;
  background: white;
  border-radius: 12px;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.15);
  overflow: hidden;
}

.panel-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 16px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  font-weight: 600;
}

.panel-content {
  max-height: 300px;
  overflow-y: auto;
}

.panel-footer {
  padding: 8px 16px;
  border-top: 1px solid #eee;
  text-align: center;
}

.empty {
  padding: 30px;
  text-align: center;
  color: #999;
}

.empty p {
  margin-top: 10px;
}

.alert-list {
  padding: 8px;
}

.alert-item {
  padding: 10px 12px;
  margin-bottom: 8px;
  border-radius: 8px;
  cursor: pointer;
  transition: background 0.2s;
}

.alert-item:hover {
  background: #f5f7fa;
}

.alert-item.level-critical {
  background: #fee;
  border-left: 3px solid #f56c6c;
}

.alert-item.level-high {
  background: #fff7e6;
  border-left: 3px solid #e6a23c;
}

.alert-item.level-medium {
  background: #e6f7ff;
  border-left: 3px solid #1890ff;
}

.alert-item.level-low {
  background: #f6ffed;
  border-left: 3px solid #67c23a;
}

.alert-title {
  font-size: 14px;
  font-weight: 500;
  margin-bottom: 4px;
}

.alert-time {
  font-size: 12px;
  color: #999;
}

.slide-fade-enter-active,
.slide-fade-leave-active {
  transition: all 0.3s ease;
}

.slide-fade-enter-from,
.slide-fade-leave-to {
  opacity: 0;
  transform: translateY(10px);
}
</style>