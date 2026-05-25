<template>
  <div class="line-task-board">
    <el-card>
      <template #header>
        <div class="card-header">
          <span>产线任务看板</span>
          <el-select v-model="selectedLineId" placeholder="选择产线" @change="fetchTasks" style="width:200px">
            <el-option v-for="l in lines" :key="l.id" :label="l.name" :value="l.id" />
          </el-select>
        </div>
      </template>

      <div v-if="!selectedLineId" style="text-align:center; padding:40px; color:#999">
        请先选择产线查看今日任务
      </div>

      <div v-else-if="loading" v-loading="loading" style="height:200px" />

      <div v-else-if="tasks.length === 0" style="text-align:center; padding:40px; color:#999">
        该产线暂无今日派工任务
      </div>

      <div v-else class="task-grid">
        <el-card v-for="task in tasks" :key="task.id" class="task-card" shadow="hover">
          <div class="task-header">
            <span class="task-no">{{ task.orderNo }}</span>
            <el-tag :type="statusTagType(task.status)" size="small">{{ statusLabel(task.status) }}</el-tag>
          </div>
          <div class="task-body">
            <div class="task-info">
              <label>物料：</label><span>{{ task.materialName || '-' }}</span>
            </div>
            <div class="task-info">
              <label>计划数量：</label><span>{{ task.plannedQty }}</span>
            </div>
            <div class="task-info">
              <label>已完成：</label><span>{{ task.completedQty || 0 }}</span>
            </div>
            <div class="task-info">
              <label>计划时间：</label><span>{{ task.planStartTime ? task.planStartTime.substring(0, 16) : '-' }} ~ {{ task.planEndTime ? task.planEndTime.substring(0, 16) : '-' }}</span>
            </div>
            <div class="task-info">
              <label>工序：</label>
              <span v-if="task.steps && task.steps.length > 0">
                <el-tag v-for="s in task.steps" :key="s.id" size="small" style="margin-right:4px">
                  {{ s.stepName }}
                </el-tag>
              </span>
              <span v-else>-</span>
            </div>
          </div>
          <div class="task-footer">
            <el-button size="small" type="primary" @click="goDetail(task.id)">查看详情</el-button>
            <el-button size="small" type="success" @click="openDispatchDialog(task)">派工管理</el-button>
          </div>
        </el-card>
      </div>
    </el-card>

    <!-- 派工管理对话框 -->
    <el-dialog v-model="dispatchDialogVisible" :title="'派工管理 - ' + (dispatchOrder?.orderNo || '')" width="750px" :close-on-click-modal="false">
      <h4 style="margin-bottom:12px">工序派工情况</h4>
      <el-table :data="dispatchSteps" border stripe>
        <el-table-column prop="stepNo" label="工序号" width="80" />
        <el-table-column prop="stepName" label="工序名称" min-width="140" />
        <el-table-column label="派工状态" width="100">
          <template #default="{ row }">
            <el-tag v-if="row.workstationId" type="success" size="small">已派工</el-tag>
            <el-tag v-else type="info" size="small">未派工</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="已派工位" min-width="140">
          <template #default="{ row }">
            <span v-if="row.workstationName">{{ row.workstationName }}</span>
            <span v-else-if="row.workstationId">工位#{{ row.workstationId }}</span>
            <span v-else style="color:#999">-</span>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="200">
          <template #default="{ row }">
            <template v-if="row.workstationId">
              <el-button size="small" type="warning" @click="handleUndispatch(row)" :loading="dispatchLoadingMap[row.id!]">取消派工</el-button>
              <el-button size="small" type="primary" @click="openReassignStep(row)" style="margin-left:4px">更换工位</el-button>
            </template>
            <el-button v-else size="small" type="success" @click="openAssignStep(row)">派工</el-button>
          </template>
        </el-table-column>
      </el-table>
      <template #footer>
        <el-button @click="dispatchDialogVisible = false">关闭</el-button>
      </template>
    </el-dialog>

    <!-- 选择工位对话框 -->
    <el-dialog v-model="assignDialogVisible" :title="assignDialogTitle" width="400px" :close-on-click-modal="false">
      <el-form label-width="100px">
        <el-form-item label="工序">
          <span>{{ assignStep?.stepName }}</span>
        </el-form-item>
        <el-form-item label="选择工位">
          <el-select v-model="assignWorkstationId" placeholder="请选择工位" style="width:100%">
            <el-option v-for="ws in availableWorkstations" :key="ws.id" :label="ws.name" :value="ws.id" />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="assignDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="confirmAssign" :loading="assignSubmitting">确认</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { dispatchApi } from '../../api/scheduling'
import { productionLineApi } from '../../api/production-line'
import { workstationApi } from '../../api/workstation'

const router = useRouter()
const selectedLineId = ref<number | null>(null)
const lines = ref<any[]>([])
const tasks = ref<any[]>([])
const loading = ref(false)

async function fetchLines() {
  const res = await productionLineApi.list()
  lines.value = res.data || []
}

async function fetchTasks() {
  if (!selectedLineId.value) return
  loading.value = true
  try {
    const res = await dispatchApi.lineTasks(selectedLineId.value)
    tasks.value = res.data || []
  } finally {
    loading.value = false
  }
}

function goDetail(id: number) {
  router.push('/work-order/' + id)
}

function statusTagType(status: string): string {
  const map: Record<string, string> = {
    SCHEDULED: 'warning',
    IN_PROGRESS: 'primary',
    COMPLETED: 'success',
    ON_HOLD: 'info'
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

// 派工管理
const dispatchDialogVisible = ref(false)
const dispatchOrder = ref<any>(null)
const dispatchSteps = ref<any[]>([])
const dispatchLoadingMap = reactive<Record<number, boolean>>({})

const assignDialogVisible = ref(false)
const assignDialogTitle = ref('')
const assignStep = ref<any>(null)
const assignWorkstationId = ref<number | null>(null)
const availableWorkstations = ref<any[]>([])
const assignSubmitting = ref(false)

function openDispatchDialog(task: any) {
  dispatchOrder.value = task
  dispatchSteps.value = task.steps || []
  dispatchDialogVisible.value = true
}

async function handleUndispatch(step: any) {
  dispatchLoadingMap[step.id!] = true
  try {
    await dispatchApi.undispatch(step.id!)
    ElMessage.success(`工序「${step.stepName}」已取消派工`)
    step.workstationId = null
    step.workstationName = null
    // 刷新任务
    fetchTasks()
  } catch (e: any) {
    ElMessage.error(e.message || '取消派工失败')
  } finally {
    dispatchLoadingMap[step.id!] = false
  }
}

async function openAssignStep(step: any) {
  assignStep.value = step
  assignDialogTitle.value = '派工 - ' + step.stepName
  assignWorkstationId.value = null
  await loadAvailableWorkstations()
  assignDialogVisible.value = true
}

async function openReassignStep(step: any) {
  assignStep.value = step
  assignDialogTitle.value = '更换工位 - ' + step.stepName
  assignWorkstationId.value = step.workstationId
  await loadAvailableWorkstations()
  assignDialogVisible.value = true
}

async function loadAvailableWorkstations() {
  if (!selectedLineId.value) return
  try {
    const res = await dispatchApi.availableWorkstations(selectedLineId.value)
    availableWorkstations.value = res.data || []
  } catch {
    try {
      const res = await workstationApi.listByLine(selectedLineId.value)
      availableWorkstations.value = res.data || []
    } catch {}
  }
}

async function confirmAssign() {
  if (!assignStep.value?.id || !assignWorkstationId.value) {
    ElMessage.warning('请选择工位')
    return
  }
  assignSubmitting.value = true
  try {
    await dispatchApi.dispatch(assignStep.value.id, assignWorkstationId.value)
    ElMessage.success(`工序「${assignStep.value.stepName}」派工成功`)
    assignDialogVisible.value = false
    // 刷新数据
    fetchTasks()
  } catch (e: any) {
    ElMessage.error(e.message || '派工失败')
  } finally {
    assignSubmitting.value = false
  }
}

onMounted(() => {
  fetchLines()
})
</script>

<style scoped>
.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.task-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(360px, 1fr));
  gap: 16px;
}
.task-card {
  cursor: default;
}
.task-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
  padding-bottom: 8px;
  border-bottom: 1px solid #eee;
}
.task-no {
  font-weight: bold;
  font-size: 15px;
}
.task-body {
  font-size: 13px;
}
.task-info {
  margin-bottom: 6px;
}
.task-info label {
  color: #666;
  margin-right: 4px;
}
.task-footer {
  margin-top: 12px;
  text-align: right;
}
</style>
