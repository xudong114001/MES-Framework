<template>
  <div class="page-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span>工单管理</span>
          <el-button type="primary" @click="openCreate" v-permission="'workorder:create'">新增工单</el-button>
        </div>
      </template>

      <!-- 筛选条件 -->
      <el-form :inline="true" :model="filters" class="filter-form">
        <el-form-item label="状态">
          <el-select v-model="filters.status" placeholder="全部状态" clearable style="width: 140px">
            <el-option label="待下达" value="PENDING" />
            <el-option label="已下达" value="RELEASED" />
            <el-option label="生产中" value="IN_PROGRESS" />
            <el-option label="已完成" value="COMPLETED" />
            <el-option label="已关闭" value="CLOSED" />
            <el-option label="已取消" value="CANCELLED" />
            <el-option label="已暂停" value="ON_HOLD" />
          </el-select>
        </el-form-item>
        <el-form-item label="物料">
          <el-select v-model="filters.materialId" placeholder="搜索物料" filterable remote :remote-method="searchMaterial" clearable style="width: 200px" @click="loadMaterials">
            <el-option v-for="m in materials" :key="m.id" :label="m.name" :value="m.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="日期">
          <el-date-picker
            v-model="filters.dateRange"
            type="daterange"
            range-separator="至"
            start-placeholder="开始日期"
            end-placeholder="结束日期"
            value-format="YYYY-MM-DD"
            style="width: 260px"
          />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="fetchData">查询</el-button>
          <el-button @click="resetFilters">重置</el-button>
        </el-form-item>
      </el-form>

      <!-- 表格 -->
      <el-table :data="list" border stripe v-loading="loading">
        <el-table-column prop="orderNo" label="工单号" min-width="160" fixed />
        <el-table-column prop="materialName" label="物料名称" min-width="180" show-overflow-tooltip />
        <el-table-column prop="plannedQty" label="计划数量" width="100" align="right" />
        <el-table-column prop="completedQty" label="已完成" width="90" align="right" />
        <el-table-column prop="scrapQty" label="报废" width="80" align="right" />
        <el-table-column label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="statusTagType(row.status)" size="small">
              {{ statusLabel(row.status) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="priority" label="优先级" width="80" align="center" />
        <el-table-column label="计划时间" min-width="170">
          <template #default="{ row }">
            {{ row.planStartTime ? formatTime(row.planStartTime) : '' }} ~ {{ row.planEndTime ? formatTime(row.planEndTime) : '' }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="300" fixed="right">
          <template #default="{ row }">
            <el-button size="small" type="primary" @click="viewDetail(row)">详情</el-button>
            <template v-if="row.status === 'PENDING'">
              <el-button size="small" type="success" @click="handleRelease(row)">下达</el-button>
              <el-button size="small" @click="openSplitDialog(row)">拆分</el-button>
              <el-button size="small" type="warning" @click="handleCancel(row)">取消</el-button>
              <el-button size="small" type="danger" @click="handleDelete(row)" v-permission="'workorder:delete'">删除</el-button>
            </template>
            <el-button v-else-if="row.status === 'RELEASED'" size="small" @click="handleHold(row)">暂停</el-button>
            <el-button v-else-if="row.status === 'IN_PROGRESS'" size="small" @click="handleHold(row)">暂停</el-button>
            <el-button v-else-if="row.status === 'ON_HOLD'" size="small" type="success" @click="handleResume(row)">恢复</el-button>
            <el-button v-else-if="row.status === 'COMPLETED'" size="small" @click="handleClose(row)">关闭</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 分流弹窗 -->
    <SplitDialog v-model:visible="splitVisible" :work-order="currentOrder" @done="fetchData" />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { workOrderApi } from '../../api/work-order'
import type { WorkOrder } from '../../api/work-order'
import { materialApi } from '../../api/material'
import SplitDialog from '../../components/SplitDialog.vue'

const router = useRouter()
const loading = ref(false)
const list = ref<WorkOrder[]>([])
const materials = ref<any[]>([])
const splitVisible = ref(false)
const currentOrder = ref<WorkOrder | null>(null)

const filters = ref({
  status: '',
  materialId: undefined as number | undefined,
  dateRange: [] as string[]
})

function statusTagType(status?: string): string {
  const map: Record<string, string> = {
    PENDING: 'info',
    RELEASED: 'primary',
    IN_PROGRESS: 'warning',
    COMPLETED: 'success',
    CLOSED: '',
    CANCELLED: 'danger',
    ON_HOLD: 'danger'
  }
  return map[status || ''] || 'info'
}

function statusLabel(status?: string): string {
  const map: Record<string, string> = {
    PENDING: '待下达',
    RELEASED: '已下达',
    IN_PROGRESS: '生产中',
    COMPLETED: '已完成',
    CLOSED: '已关闭',
    CANCELLED: '已取消',
    ON_HOLD: '已暂停'
  }
  return map[status || ''] || status || ''
}

function formatTime(t?: string): string {
  if (!t) return ''
  return t.substring(0, 16).replace('T', ' ')
}

async function fetchData() {
  loading.value = true
  try {
    const res: any = await workOrderApi.list()
    list.value = (res.data || []).map((item: any) => ({
      ...item,
      materialName: item.materialName || `物料#${item.materialId}`
    }))
  } catch (e) {
    console.error(e)
  } finally {
    loading.value = false
  }
}

async function loadMaterials() {
  if (materials.value.length > 0) return
  try {
    const res: any = await materialApi.list()
    materials.value = res.data || []
  } catch (e) {
    console.error(e)
  }
}

function searchMaterial(query: string) {
  // Client-side filtering on loaded materials
}

function resetFilters() {
  filters.value = { status: '', materialId: undefined, dateRange: [] }
  fetchData()
}

function openCreate() {
  router.push('/work-order/new')
}

function viewDetail(row: WorkOrder) {
  router.push(`/work-order/${row.id}`)
}

async function handleRelease(row: WorkOrder) {
  try {
    await ElMessageBox.confirm(`确认下达工单「${row.orderNo}」？`, '提示')
    await workOrderApi.release(row.id!)
    ElMessage.success('已下达')
    await fetchData()
  } catch {}
}

async function handleHold(row: WorkOrder) {
  try {
    await ElMessageBox.confirm(`确认暂停工单「${row.orderNo}」？`, '提示')
    await workOrderApi.hold(row.id!)
    ElMessage.success('已暂停')
    await fetchData()
  } catch {}
}

async function handleResume(row: WorkOrder) {
  try {
    await ElMessageBox.confirm(`确认恢复工单「${row.orderNo}」？`, '提示')
    await workOrderApi.resume(row.id!)
    ElMessage.success('已恢复')
    await fetchData()
  } catch {}
}

async function handleCancel(row: WorkOrder) {
  try {
    await ElMessageBox.confirm(`确认取消工单「${row.orderNo}」？取消后不可恢复。`, '警告', { type: 'warning' })
    await workOrderApi.cancel(row.id!)
    ElMessage.success('已取消')
    await fetchData()
  } catch {}
}

async function handleClose(row: WorkOrder) {
  try {
    await ElMessageBox.confirm(`确认关闭工单「${row.orderNo}」？`, '提示')
    await workOrderApi.close(row.id!)
    ElMessage.success('已关闭')
    await fetchData()
  } catch {}
}

async function handleDelete(row: WorkOrder) {
  try {
    await ElMessageBox.confirm(`确认删除工单「${row.orderNo}」？`, '警告', { type: 'warning' })
    await workOrderApi.delete(row.id!)
    ElMessage.success('已删除')
    await fetchData()
  } catch {}
}

function openSplitDialog(row: WorkOrder) {
  currentOrder.value = row
  splitVisible.value = true
}

onMounted(() => {
  fetchData()
  loadMaterials()
})
</script>

<style scoped>
.page-container {
  padding: 16px;
}
.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.filter-form {
  margin-bottom: 8px;
}
</style>
