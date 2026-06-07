<template>
  <div class="page-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span>同步日志</span>
        </div>
      </template>

      <div class="filter-bar">
        <el-select v-model="filterType" placeholder="事件类型" clearable style="width: 160px" @change="loadData">
          <el-option label="全部" value="" />
          <el-option label="ERP" value="ERP" />
          <el-option label="WMS" value="WMS" />
          <el-option label="PLC" value="PLC" />
        </el-select>
        <el-select v-model="filterStatus" placeholder="状态" clearable style="width: 140px" @change="loadData">
          <el-option label="成功" value="SUCCESS" />
          <el-option label="失败" value="FAILED" />
        </el-select>
        <el-button type="primary" @click="loadData">查询</el-button>
      </div>

      <el-table :data="list" border stripe v-loading="loading">
        <el-table-column prop="createdAt" label="时间" width="180">
          <template #default="{ row }">
            {{ formatTime(row.createdAt) }}
          </template>
        </el-table-column>
        <el-table-column prop="eventType" label="事件类型" width="120" />
        <el-table-column prop="direction" label="方向" width="100">
          <template #default="{ row }">
            <el-tag size="small" :type="directionTag(row.direction)">
              {{ row.direction === 'IN' ? '接收' : row.direction === 'OUT' ? '发送' : row.direction }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="status" label="状态" width="100">
          <template #default="{ row }">
            <el-tag size="small" :type="row.status === 'SUCCESS' ? 'success' : 'danger'">
              {{ row.status === 'SUCCESS' ? '成功' : '失败' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="message" label="消息" min-width="250" show-overflow-tooltip />
      </el-table>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { integrationApi, type IntegrationEvent } from '../../api/integration'

const route = useRoute()

const list = ref<IntegrationEvent[]>([])
const loading = ref(false)
const filterType = ref<string>(String(route.query.type || ''))
const filterStatus = ref<string>('')

function formatTime(val: string) {
  if (!val) return '-'
  const d = new Date(val)
  if (isNaN(d.getTime())) return val
  return d.toLocaleString()
}

function directionTag(direction: string) {
  if (direction === 'IN') return 'primary'
  if (direction === 'OUT') return 'warning'
  return 'info'
}

async function loadData() {
  loading.value = true
  try {
    const params: any = {}
    if (filterType.value) params.type = filterType.value
    if (filterStatus.value) params.status = filterStatus.value
    const res: any = await integrationApi.getLogs(params)
    list.value = res.data || []
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  loadData()
})
</script>

<style scoped>
.page-container { padding: 16px; }
.card-header { display: flex; justify-content: space-between; align-items: center; }
.filter-bar {
  display: flex;
  gap: 12px;
  margin-bottom: 16px;
  align-items: center;
}
</style>
