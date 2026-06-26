<template>
  <div class="andon-board">
    <!-- 顶部活跃异常大标签 -->
    <div class="andon-header">
      <div class="active-alarm" :class="{ 'has-active': activeEvents.length > 0 }">
        <el-icon :size="32"><WarningFilled /></el-icon>
        <div class="alarm-info">
          <span class="alarm-count">{{ activeEvents.length }}</span>
          <span class="alarm-label">活跃异常</span>
        </div>
      </div>
      <div class="header-actions">
        <el-button type="primary" @click="showTriggerDialog = true">
          <el-icon><Plus /></el-icon>触发异常
        </el-button>
        <el-button @click="loadData">
          <el-icon><Refresh /></el-icon>刷新
        </el-button>
        <el-tag type="info" effect="plain">每 {{ autoRefreshInterval / 1000 }} 秒自动刷新</el-tag>
      </div>
    </div>

    <!-- 异常事件列表 -->
    <el-table :data="events" style="width: 100%" stripe max-height="calc(100vh - 260px)">
      <el-table-column prop="id" label="ID" width="70" />
      <el-table-column label="事件类型" width="140">
        <template #default="{ row }">
          <el-tag :type="eventTagType(row.eventType)" size="small" effect="dark">
            {{ eventTypeLabel(row.eventType) }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="workstationName" label="工位" width="120" />
      <el-table-column prop="description" label="描述" min-width="200" show-overflow-tooltip />
      <el-table-column label="发生时间" width="180">
        <template #default="{ row }">
          {{ formatTime(row.occurredAt) }}
        </template>
      </el-table-column>
      <el-table-column prop="handler" label="处理人" width="120" />
      <el-table-column label="处理时间" width="180">
        <template #default="{ row }">
          {{ row.handledAt ? formatTime(row.handledAt) : '-' }}
        </template>
      </el-table-column>
      <el-table-column label="状态" width="100">
        <template #default="{ row }">
          <el-tag :type="row.isHandled ? 'success' : 'danger'" size="small">
            {{ row.isHandled ? '已处理' : '未处理' }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column label="操作" width="120" fixed="right">
        <template #default="{ row }">
          <el-button
            v-if="!row.isHandled"
            type="warning"
            size="small"
            @click="handleResolve(row)"
          >
            处理
          </el-button>
        </template>
      </el-table-column>
    </el-table>

    <!-- 触发异常对话框 -->
    <el-dialog v-model="showTriggerDialog" title="触发异常" width="500px">
      <el-form :model="triggerForm" label-width="100px">
        <el-form-item label="事件类型" required>
          <el-select v-model="triggerForm.eventType" placeholder="选择异常类型" style="width: 100%">
            <el-option label="质量异常" :value="0" />
            <el-option label="设备故障" :value="1" />
            <el-option label="物料短缺" :value="2" />
            <el-option label="其他异常" :value="3" />
          </el-select>
        </el-form-item>
        <el-form-item label="工位">
          <el-input v-model="triggerForm.workstationName" placeholder="输入工位名称" />
        </el-form-item>
        <el-form-item label="描述">
          <el-input
            v-model="triggerForm.description"
            type="textarea"
            :rows="3"
            placeholder="描述异常情况"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showTriggerDialog = false">取消</el-button>
        <el-button type="primary" @click="submitTrigger" :loading="triggering">确认触发</el-button>
      </template>
    </el-dialog>

    <!-- 处理对话框 -->
    <el-dialog v-model="showResolveDialog" title="处理异常" width="400px">
      <el-form :model="resolveForm" label-width="80px">
        <el-form-item label="处理人">
          <el-input v-model="resolveForm.handler" placeholder="输入处理人姓名" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showResolveDialog = false">取消</el-button>
        <el-button type="primary" @click="submitResolve" :loading="resolving">确认处理</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { ElMessage } from 'element-plus'
import { andonApi } from '../../api/andon'
import { Plus, Refresh, WarningFilled } from '@element-plus/icons-vue'

const events = ref<any[]>([])
const activeEvents = ref<any[]>([])
const showTriggerDialog = ref(false)
const showResolveDialog = ref(false)
const triggering = ref(false)
const resolving = ref(false)

const triggerForm = ref({ eventType: 0, level: 0, workstationName: '', description: '' })
const resolveForm = ref({ handler: '' })
const resolvingId = ref(0)

const autoRefreshInterval = 10000 // 10 seconds
let timer: number | null = null

const eventTagType = (type: number) => {
  const map: Record<number, string> = {
    0: 'warning',
    1: 'danger',
    2: 'info',
    3: 'default'
  }
  return map[type] || 'default'
}

const eventTypeLabel = (type: number) => {
  const map: Record<number, string> = {
    0: '质量异常',
    1: '设备故障',
    2: '物料短缺',
    3: '其他异常'
  }
  return map[type] || '未知'
}

function formatTime(ts: string) {
  if (!ts) return '-'
  const d = new Date(ts)
  return d.toLocaleString('zh-CN', { timeZone: 'Asia/Shanghai' })
}

async function loadData() {
  try {
    const [allRes, activeRes] = await Promise.all([
      andonApi.all(),
      andonApi.active()
    ])
    events.value = allRes.data || []
    activeEvents.value = activeRes.data || []
  } catch {
    // handled by interceptor
  }
}

async function submitTrigger() {
  if (triggerForm.value.eventType === 0 && !triggerForm.value.description) {
    ElMessage.warning('请选择事件类型')
    return
  }
  triggering.value = true
  try {
    await andonApi.trigger(triggerForm.value)
    ElMessage.success('异常事件已触发')
    showTriggerDialog.value = false
    triggerForm.value = { eventType: 0, level: 0, workstationName: '', description: '' }
    await loadData()
  } finally {
    triggering.value = false
  }
}

function handleResolve(row: any) {
  resolvingId.value = row.id
  resolveForm.value.handler = ''
  showResolveDialog.value = true
}

async function submitResolve() {
  resolving.value = true
  try {
    await andonApi.resolve(resolvingId.value, resolveForm.value.handler || undefined)
    ElMessage.success('事件已处理')
    showResolveDialog.value = false
    await loadData()
  } finally {
    resolving.value = false
  }
}

onMounted(() => {
  loadData()
  timer = window.setInterval(loadData, autoRefreshInterval)
})

onUnmounted(() => {
  if (timer) {
    clearInterval(timer)
    timer = null
  }
})
</script>

<style scoped>
.andon-board {
  padding: 20px;
}

.andon-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.active-alarm {
  display: flex;
  align-items: center;
  gap: 16px;
  background: #f5f7fa;
  border: 2px solid #e4e7ed;
  border-radius: 12px;
  padding: 16px 24px;
  transition: all 0.3s;
}

.active-alarm.has-active {
  background: #fef0f0;
  border-color: #f56c6c;
  animation: alarm-pulse 2s infinite;
}

@keyframes alarm-pulse {
  0%, 100% { box-shadow: 0 0 0 0 rgba(245, 108, 108, 0.4); }
  50% { box-shadow: 0 0 0 12px rgba(245, 108, 108, 0); }
}

.alarm-info {
  display: flex;
  flex-direction: column;
}

.alarm-count {
  font-size: 42px;
  font-weight: bold;
  color: #f56c6c;
  line-height: 1;
}

.alarm-label {
  font-size: 14px;
  color: #606266;
  margin-top: 4px;
}

.header-actions {
  display: flex;
  align-items: center;
  gap: 12px;
}

/* 未处理行红色高亮 */
.el-table :deep(.el-table__row) {
  &.el-table__row--striped {
    background-color: transparent;
  }
}
</style>
