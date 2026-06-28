<template>
  <div class="scheduling-view">
    <el-card>
      <template #header>
        <div class="card-header">
          <span>排产管理</span>
          <div>
            <el-button type="primary" @click="fetchUnscheduled" :loading="loading">刷新待排产</el-button>
            <el-button type="success" @click="handleAutoSchedule" :loading="autoLoading">自动排产</el-button>
          </div>
        </div>
      </template>

      <el-tabs v-model="activeTab" @tab-change="onTabChange">
        <el-tab-pane label="待排产工单" name="unscheduled">
          <el-table :data="unscheduledOrders" v-loading="loading" stripe style="width: 100%">
            <el-table-column prop="orderNo" label="工单号" width="200" />
            <el-table-column prop="materialName" label="物料" />
            <el-table-column prop="plannedQty" label="计划数���" width="100" />
            <el-table-column prop="priority" label="优先级" width="80" />
            <el-table-column prop="planStartTime" label="计划开始" width="160" />
            <el-table-column prop="planEndTime" label="计划结束" width="160" />
            <el-table-column label="操作" width="200" fixed="right">
              <template #default="{ row }">
                <el-select v-model="scheduleLineMap[row.id]" placeholder="选择产线" size="small" style="width:110px">
                  <el-option v-for="l in lines" :key="l.id" :label="l.name" :value="l.id" />
                </el-select>
                <el-button type="primary" size="small" @click="handleSchedule(row.id)" style="margin-left:4px">排产</el-button>
              </template>
            </el-table-column>
          </el-table>
          <el-empty v-if="!loading && unscheduledOrders.length === 0" description="暂无待排产工单" />
        </el-tab-pane>

        <el-tab-pane v-for="line in lines" :key="line.id" :label="line.name" :name="'line-' + line.id">
          <el-table :data="scheduledOrders[line.id] || []" v-loading="lineLoading[line.id]" stripe style="width: 100%">
            <el-table-column prop="orderNo" label="工单号" width="200" />
            <el-table-column prop="materialName" label="物料" />
            <el-table-column prop="plannedQty" label="计划数量" width="100" />
            <el-table-column prop="completedQty" label="已完成" width="100" />
            <el-table-column prop="status" label="状态" width="100">
              <template #default="{ row }">
                <el-tag :type="statusTagType(row.status)">{{ statusLabel(row.status) }}</el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="planStartTime" label="计划开始" width="160" />
            <el-table-column prop="planEndTime" label="计划结束" width="160" />
            <el-table-column label="操作" width="220" fixed="right">
              <template #default="{ row }">
                <el-button size="small" @click="handleUnschedule(row.id)">取消排产</el-button>
                <el-button size="small" type="primary" @click="goDetail(row.id)">详情</el-button>
                <el-button size="small" type="success" @click="openDispatchDialog(row)">派工</el-button>
              </template>
            </el-table-column>
          </el-table>
          <el-empty v-if="!lineLoading[line.id] && (!scheduledOrders[line.id] || scheduledOrders[line.id].length === 0)" description="该产线暂无已排产工单" />
        </el-tab-pane>
      </el-tabs>
    </el-card>

    <!-- 派工对话框 -->
    <el-dialog v-model="dispatchDialogVisible" :title="'派工 - ' + (dispatchOrder?.orderNo || '')" width="700px" :close-on-click-modal="false">
      <el-table :data="dispatchSteps" border stripe>
        <el-table-column prop="stepNo" label="工序号" width="80" />
        <el-table-column prop="stepName" label="工序名称" min-width="140" />
        <el-table-column label="派工状态" width="100">
          <template #default="{ row }">
            <el-tag v-if="row.workstationId" type="success" size="small">已派工</el-tag>
            <el-tag v-else type="info" size="small">未派工</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="选择工位" min-width="180">
          <template #default="{ row }">
            <el-select
              v-model="dispatchWorkstationMap[row.id!]"
              :placeholder="row.workstationId ? '更换工位' : '请选择工位'"
              clearable
              style="width:100%"
            >
              <el-option
                v-for="ws in availableWorkstations"
                :key="ws.id"
                :label="ws.name"
                :value="ws.id"
              />
            </el-select>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="100">
          <template #default="{ row }">
            <el-button
              size="small"
              type="primary"
              :disabled="!dispatchWorkstationMap[row.id!]"
              :loading="dispatchLoadingMap[row.id!]"
              @click="handleDispatchStep(row)"
            >
              派工
            </el-button>
          </template>
        </el-table-column>
      </el-table>
      <template #footer>
        <el-button @click="dispatchDialogVisible = false">关闭</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { useRouter } from 'vue-router'
import { schedulingApi, dispatchApi } from '../../api/scheduling'
import { productionLineApi } from '../../api/production-line'
import { workstationApi } from '../../api/workstation'

const router = useRouter()
const loading = ref(false)
const autoLoading = ref(false)
const activeTab = ref('unscheduled')
const unscheduledOrders = ref<any[]>([])
const lines = ref<any[]>([])
const scheduledOrders = reactive<Record<number, any[]>>({})
const lineLoading = reactive<Record<number, boolean>>({})
const scheduleLineMap = reactive<Record<number, number | null>>({})

// 派工相关
const dispatchDialogVisible = ref(false)
const dispatchOrder = ref<any>(null)
const dispatchSteps = ref<any[]>([])
const availableWorkstations = ref<any[]>([])
const dispatchWorkstationMap = reactive<Record<number, number | null>>({})
const dispatchLoadingMap = reactive<Record<number, boolean>>({})

async function fetchLines() {
  const res = await productionLineApi.list()
  lines.value = res.data || []
}

async function fetchUnscheduled() {
  loading.value = true
  try {
    const res = await schedulingApi.unscheduledOrders()
    unscheduledOrders.value = res.data || []
  } finally {
    loading.value = false
  }
}

async function fetchLineOrders(lineId: number) {
  lineLoading[lineId] = true
  try {
    const res = await schedulingApi.lineScheduledOrders(lineId)
    scheduledOrders[lineId] = res.data || []
  } finally {
    lineLoading[lineId] = false
  }
}

async function handleSchedule(workOrderId: number) {
  const lineId = scheduleLineMap[workOrderId]
  if (!lineId) {
    ElMessage.warning('请先选择产线')
    return
  }
  try {
    await schedulingApi.schedule(workOrderId, lineId)
    ElMessage.success('排产成功')
    // 切换 tab 到对应产线查看
    activeTab.value = 'line-' + lineId
    await fetchUnscheduled()
    await fetchLineOrders(lineId)
  } catch (e: any) {
    ElMessage.error(e.message || '排产失败')
  }
}

async function handleAutoSchedule() {
  autoLoading.value = true
  try {
    await schedulingApi.autoSchedule()
    ElMessage.success('自动排产完成')
    await fetchUnscheduled()
    lines.value.forEach(l => fetchLineOrders(l.id))
  } catch (e: any) {
    ElMessage.error(e.message || '自动排产失败')
  } finally {
    autoLoading.value = false
  }
}

async function handleUnschedule(workOrderId: number) {
  try {
    await ElMessageBox.confirm('确定取消该工单的排产？', '提示')
    await schedulingApi.unschedule(workOrderId)
    ElMessage.success('已取消排产')
    // 重新加载当前 tab
    const lineId = parseInt(activeTab.value.replace('line-', ''))
    await fetchUnscheduled()
    await fetchLineOrders(lineId)
  } catch {}
}

function goDetail(id: number) {
  router.push('/work-order/' + id)
}

function openDispatchDialog(order: any) {
  dispatchOrder.value = order
  dispatchSteps.value = order.steps || []
  // 重置映射
  Object.keys(dispatchWorkstationMap).forEach(k => { dispatchWorkstationMap[Number(k)] = null })
  Object.keys(dispatchLoadingMap).forEach(k => { dispatchLoadingMap[Number(k)] = false })
  // 预填已有工位
  for (const step of (order.steps || [])) {
    if (step.workstationId) {
      dispatchWorkstationMap[step.id] = step.workstationId
    }
  }
  // 获取可用工位
  const lineId = order.lineId || parseInt(activeTab.value.replace('line-', ''))
  fetchAvailableWorkstations(lineId)
  dispatchDialogVisible.value = true
}

async function fetchAvailableWorkstations(lineId: number) {
  try {
    const res = await dispatchApi.availableWorkstations(lineId)
    availableWorkstations.value = res.data || []
  } catch {
    try {
      const res = await workstationApi.listByLine(lineId)
      availableWorkstations.value = res.data || []
    } catch {}
  }
}

async function handleDispatchStep(step: any) {
  const workstationId = dispatchWorkstationMap[step.id!]
  if (!workstationId) {
    ElMessage.warning('请选择工位')
    return
  }
  dispatchLoadingMap[step.id!] = true
  try {
    await dispatchApi.dispatch(step.id!, workstationId)
    ElMessage.success(`工序「${step.stepName}」派工成功`)
    step.workstationId = workstationId
    const lineId = parseInt(activeTab.value.replace('line-', ''))
    if (lineId) {
      await fetchLineOrders(lineId)
    }
  } catch (e: any) {
    ElMessage.error(e.message || '派工失败')
  } finally {
    dispatchLoadingMap[step.id!] = false
  }
}

function onTabChange(tab: string) {
  if (tab.startsWith('line-')) {
    const lineId = parseInt(tab.replace('line-', ''))
    if (!scheduledOrders[lineId]) {
      fetchLineOrders(lineId)
    }
  }
}

function statusTagType(status: string): string {
  const map: Record<string, string> = {
    SCHEDULED: 'warning',
    IN_PROGRESS: 'primary',
    COMPLETED: 'success',
    ON_HOLD: 'info',
    CANCELLED: 'danger'
  }
  return map[status] || 'info'
}

function statusLabel(status: string): string {
  const map: Record<string, string> = {
    SCHEDULED: '已排产',
    IN_PROGRESS: '生产中',
    COMPLETED: '已完成',
    ON_HOLD: '已暂停',
    CANCELLED: '已取消'
  }
  return map[status] || status
}

onMounted(async () => {
  await fetchLines()
  await fetchUnscheduled()
})
</script>

<style scoped>
.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
</style>
