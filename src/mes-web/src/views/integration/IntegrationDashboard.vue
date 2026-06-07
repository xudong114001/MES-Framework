<template>
  <div class="page-container">
    <h2 class="page-title">系统集成</h2>
    <el-row :gutter="20">
      <el-col :span="8" v-for="item in adapters" :key="item.name">
        <el-card shadow="hover" class="adapter-card">
          <div class="adapter-header">
            <div class="adapter-icon">
              <el-icon :size="32" color="#409eff"><Connection /></el-icon>
            </div>
            <div class="adapter-info">
              <span class="adapter-name">{{ item.displayName }}</span>
              <el-tag :type="item.connected ? 'success' : 'danger'" size="small">
                {{ item.connected ? '已连接' : '未连接' }}
              </el-tag>
            </div>
          </div>
          <div class="adapter-body">
            <div class="adapter-meta">
              <span class="meta-label">最后同步:</span>
              <span class="meta-value">{{ item.lastSyncTime ? formatTime(item.lastSyncTime) : '无' }}</span>
            </div>
          </div>
          <div class="adapter-footer">
            <el-button size="small" :loading="item.testing" @click="testConnection(item)">测试连接</el-button>
            <el-button size="small" type="primary" :loading="item.syncing" @click="syncData(item)">手动同步</el-button>
            <el-link type="primary" size="small" @click="goToLogs(item.name)">查看日志</el-link>
          </div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { Connection } from '@element-plus/icons-vue'
import { integrationApi, type Adapter } from '../../api/integration'

const router = useRouter()

interface AdapterView extends Adapter {
  testing: boolean
  syncing: boolean
}

const adapters = ref<AdapterView[]>([
  { name: 'ERP', displayName: 'ERP 系统', connected: false, testing: false, syncing: false },
  { name: 'WMS', displayName: 'WMS 系统', connected: false, testing: false, syncing: false },
  { name: 'PLC', displayName: 'PLC 设备', connected: false, testing: false, syncing: false }
])

function formatTime(val: string) {
  if (!val) return '无'
  const d = new Date(val)
  if (isNaN(d.getTime())) return val
  return d.toLocaleString()
}

async function loadAdapters() {
  try {
    const res: any = await integrationApi.getAdapters()
    const data: Adapter[] = res.data || []
    adapters.value = adapters.value.map(item => {
      const found = data.find(d => d.name === item.name)
      return {
        ...item,
        connected: found ? found.connected : false,
        lastSyncTime: found ? found.lastSyncTime : undefined
      }
    })
  } catch {
    // handled by interceptor
  }
}

async function testConnection(item: AdapterView) {
  item.testing = true
  try {
    const res: any = await integrationApi.testAdapter(item.name)
    item.connected = res.data?.connected ?? true
    ElMessage.success(`${item.displayName} 连接成功`)
  } finally {
    item.testing = false
  }
}

async function syncData(item: AdapterView) {
  item.syncing = true
  try {
    await integrationApi.syncAdapter(item.name)
    ElMessage.success(`${item.displayName} 同步已触发`)
    await loadAdapters()
  } finally {
    item.syncing = false
  }
}

function goToLogs(adapterName?: string) {
  router.push({ path: '/integration/logs', query: adapterName ? { type: adapterName } : undefined })
}

onMounted(() => {
  loadAdapters()
})
</script>

<style scoped>
.page-container {
  padding: 16px;
}
.page-title {
  font-size: 20px;
  color: #303133;
  margin-bottom: 20px;
}
.adapter-card {
  margin-bottom: 20px;
}
.adapter-header {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 16px;
}
.adapter-icon {
  width: 48px;
  height: 48px;
  border-radius: 8px;
  background-color: #ecf5ff;
  display: flex;
  align-items: center;
  justify-content: center;
}
.adapter-info {
  display: flex;
  flex-direction: column;
  gap: 6px;
}
.adapter-name {
  font-size: 16px;
  font-weight: bold;
  color: #303133;
}
.adapter-body {
  margin-bottom: 16px;
}
.adapter-meta {
  font-size: 13px;
  color: #606266;
}
.meta-label {
  color: #909399;
  margin-right: 4px;
}
.adapter-footer {
  display: flex;
  align-items: center;
  gap: 8px;
  border-top: 1px solid #f0f0f0;
  padding-top: 12px;
}
</style>
