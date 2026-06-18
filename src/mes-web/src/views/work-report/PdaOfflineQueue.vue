<template>
  <div class="queue-container">
    <div class="queue-header">
      <h2>离线队列管理</h2>
      <div class="queue-actions">
        <el-button type="primary" @click="retryAll" :disabled="!queueItems.length || !isOnline">全部重试</el-button>
        <el-button type="danger" @click="clearAll" :disabled="!queueItems.length">清空队列</el-button>
      </div>
    </div>

    <el-alert v-if="!isOnline" type="warning" show-icon :closable="false" title="当前处于离线状态，同步需要网络连接" />

    <el-table :data="queueItems" stripe v-loading="loading">
      <el-table-column prop="type" label="类型" width="120" />
      <el-table-column label="工单号" width="180">
        <template #default="{ row }">{{ row.payload?.scanCode || '-' }}</template>
      </el-table-column>
      <el-table-column label="工序" width="140">
        <template #default="{ row }">{{ row.payload?.stepName || '-' }}</template>
      </el-table-column>
      <el-table-column label="良品/报废/返工" width="200">
        <template #default="{ row }">
          {{ row.payload?.goodQty || 0 }}/{{ row.payload?.scrapQty || 0 }}/{{ row.payload?.reworkQty || 0 }}
        </template>
      </el-table-column>
      <el-table-column prop="createdAt" label="创建时间" width="180" />
      <el-table-column prop="retryCount" label="重试次数" width="100" />
      <el-table-column label="操作" width="160">
        <template #default="{ row }">
          <el-button size="small" type="primary" @click="retryOne(row)" :disabled="!isOnline">重试</el-button>
          <el-button size="small" type="danger" @click="removeOne(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-empty v-if="!queueItems.length && !loading" description="队列为空" />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { offlineQueue } from '../../utils/offline-queue'
import { workReportApi } from '../../api/work-report'

const queueItems = ref<any[]>([])
const loading = ref(false)
const isOnline = ref(navigator.onLine)

window.addEventListener('online', () => { isOnline.value = true })
window.addEventListener('offline', () => { isOnline.value = false })

async function loadQueue() {
  loading.value = true
  queueItems.value = await offlineQueue.getAll()
  loading.value = false
}

async function retryOne(item: any) {
  try {
    await workReportApi.pdaScan(item.payload)
    await offlineQueue.remove(item.id)
    ElMessage.success('同步成功')
    await loadQueue()
  } catch (err: any) {
    await offlineQueue.incrementRetry(item.id)
    ElMessage.error(err?.message || '同步失败')
  }
}

async function retryAll() {
  for (const item of [...queueItems.value]) {
    try {
      await workReportApi.pdaScan(item.payload)
      await offlineQueue.remove(item.id)
    } catch { /* skip failed */ }
  }
  ElMessage.success('批量同步完成')
  await loadQueue()
}

async function removeOne(item: any) {
  await offlineQueue.remove(item.id)
  ElMessage.success('已删除')
  await loadQueue()
}

async function clearAll() {
  await ElMessageBox.confirm('确定清空所有离线记录？', '确认')
  await offlineQueue.clear()
  ElMessage.success('已清空')
  await loadQueue()
}

onMounted(loadQueue)
</script>

<style scoped>
.queue-container { padding: 20px; }
.queue-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
.queue-header h2 { margin: 0; }
.queue-actions { display: flex; gap: 8px; }
</style>
