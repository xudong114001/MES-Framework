<template>
  <div class="page-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span>设备管理</span>
          <el-button type="primary" @click="openDialog()">新增设备</el-button>
        </div>
      </template>

      <el-table :data="list" border stripe v-loading="loading">
        <el-table-column prop="id" label="ID" width="80" />
        <el-table-column prop="code" label="设备编码" min-width="120" />
        <el-table-column prop="name" label="设备名称" min-width="200" />
        <el-table-column prop="model" label="型号" min-width="120" />
        <el-table-column prop="factoryName" label="所属工厂" min-width="120" show-overflow-tooltip />
        <el-table-column prop="workshopName" label="所属车间" min-width="120" show-overflow-tooltip />
        <el-table-column prop="lineName" label="所属产线" min-width="120" show-overflow-tooltip />
        <el-table-column prop="installDate" label="安装日期" width="120" />
        <el-table-column label="OEE" width="90" align="center">
          <template #default="{ row }">
            <el-progress
              type="circle"
              :percentage="Math.round((row._oee || 0) * 100)"
              :width="40"
              :stroke-width="4"
              :color="oeeColor(row._oee)"
            />
          </template>
        </el-table-column>
        <el-table-column label="状态" width="120">
          <template #default="{ row }">
            <el-tag :type="statusType(row.status)">
              {{ statusLabel(row.status) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="300" fixed="right">
          <template #default="{ row }">
            <el-button size="small" type="warning" @click="handleMaintain(row)">保养</el-button>
            <el-button size="small" type="danger" @click="handleFault(row)">报修</el-button>
            <el-button size="small" type="success" @click="viewPlan(row)">计划</el-button>
            <el-button size="small" type="info" @click="viewOee(row)">OEE</el-button>
            <el-button size="small" @click="openDialog(row)">编辑</el-button>
            <el-button size="small" type="danger" @click="handleDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 新增/编辑设备弹窗 -->
    <el-dialog v-model="dialogVisible" :title="dialogTitle" width="600px">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="120px">
        <el-form-item label="所属工厂">
          <el-select v-model="selectedFactory" placeholder="请选择工厂" style="width: 100%" @change="onFactoryChange">
            <el-option v-for="f in factories" :key="f.id" :label="f.name" :value="f.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="所属车间">
          <el-select v-model="selectedWorkshop" placeholder="请选择车间" style="width: 100%" :disabled="!selectedFactory" @change="onWorkshopChange">
            <el-option v-for="w in workshops" :key="w.id" :label="w.name" :value="w.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="所属产线" prop="lineId">
          <el-select v-model="form.lineId" placeholder="请选择产线" style="width: 100%" :disabled="!selectedWorkshop">
            <el-option v-for="l in lines" :key="l.id" :label="l.name" :value="l.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="设备编码" prop="code">
          <el-input v-model="form.code" />
        </el-form-item>
        <el-form-item label="设备名称" prop="name">
          <el-input v-model="form.name" />
        </el-form-item>
        <el-form-item label="型号" prop="model">
          <el-input v-model="form.model" />
        </el-form-item>
        <el-form-item label="理论节拍(秒/件)">
          <el-input-number v-model="form.theoreticalCycleTime" :min="0" :step="0.1" style="width: 100%" placeholder="理论节拍" />
        </el-form-item>
        <el-form-item label="日计划运行(小时)">
          <el-input-number v-model="form.plannedRunTime" :min="0" :step="0.5" style="width: 100%" placeholder="日计划运行时间" />
        </el-form-item>
        <el-form-item label="保养周期(天)">
          <el-input-number v-model="form.maintainCycle" :min="0" style="width: 100%" placeholder="保养周期" />
        </el-form-item>
        <el-form-item label="安装日期" prop="installDate">
          <el-date-picker v-model="form.installDate" type="date" placeholder="选择日期" style="width: 100%" value-format="YYYY-MM-DD" />
        </el-form-item>
        <el-form-item label="状态" prop="status">
          <el-select v-model="form.status" placeholder="请选择状态" style="width: 100%">
            <el-option label="运行中" value="RUNNING" />
            <el-option label="空闲" value="IDLE" />
            <el-option label="保养中" value="MAINTENANCE" />
            <el-option label="故障" value="BROKEN" />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSubmit" :loading="submitting">保存</el-button>
      </template>
    </el-dialog>

    <!-- OEE 详情弹窗 -->
    <el-dialog v-model="oeeDialogVisible" :title="`OEE 详情 - ${oeeEquipment?.name || ''}`" width="650px">
      <template v-if="oeeData">
        <!-- OEE 仪表盘 -->
        <div style="display: flex; justify-content: space-around; margin-bottom: 20px">
          <div style="text-align: center">
            <el-progress type="dashboard" :percentage="Math.round((oeeData.oeeValue || 0) * 100)" :width="120" :stroke-width="8" color="#409EFF">
              <template #default>
                <span style="font-size: 18px; font-weight: bold">{{ ((oeeData.oeeValue || 0) * 100).toFixed(1) }}%</span>
              </template>
            </el-progress>
            <div style="margin-top: 6px; font-size: 13px; color: #666">OEE</div>
          </div>
          <div style="text-align: center">
            <el-progress type="dashboard" :percentage="Math.round((oeeData.availability || 0) * 100)" :width="100" :stroke-width="8" color="#67C23A">
              <template #default>
                <span style="font-size: 14px; font-weight: bold">{{ ((oeeData.availability || 0) * 100).toFixed(1) }}%</span>
              </template>
            </el-progress>
            <div style="margin-top: 6px; font-size: 13px; color: #666">时间开动率</div>
          </div>
          <div style="text-align: center">
            <el-progress type="dashboard" :percentage="Math.round((oeeData.performance || 0) * 100)" :width="100" :stroke-width="8" color="#E6A23C">
              <template #default>
                <span style="font-size: 14px; font-weight: bold">{{ ((oeeData.performance || 0) * 100).toFixed(1) }}%</span>
              </template>
            </el-progress>
            <div style="margin-top: 6px; font-size: 13px; color: #666">性能开动率</div>
          </div>
          <div style="text-align: center">
            <el-progress type="dashboard" :percentage="Math.round((oeeData.quality || 0) * 100)" :width="100" :stroke-width="8" color="#F56C6C">
              <template #default>
                <span style="font-size: 14px; font-weight: bold">{{ ((oeeData.quality || 0) * 100).toFixed(1) }}%</span>
              </template>
            </el-progress>
            <div style="margin-top: 6px; font-size: 13px; color: #666">良品率</div>
          </div>
        </div>

        <el-descriptions :column="2" border>
          <el-descriptions-item label="设备状态">{{ oeeData.status || '-' }}</el-descriptions-item>
          <el-descriptions-item label="实际运行(分钟)">{{ oeeData.actualRunMinutes || 0 }}</el-descriptions-item>
          <el-descriptions-item label="计划运行(分钟)">{{ oeeData.plannedRunMinutes || 0 }}</el-descriptions-item>
          <el-descriptions-item label="良品数">{{ oeeData.goodQty || 0 }}</el-descriptions-item>
          <el-descriptions-item label="不良品数">{{ oeeData.badQty || 0 }}</el-descriptions-item>
          <el-descriptions-item label="理论节拍">{{ oeeData.theoreticalCycleTime ?? '-' }} 秒/件</el-descriptions-item>
          <el-descriptions-item label="日计划运行">{{ oeeData.plannedRunTime ?? '-' }} 小时</el-descriptions-item>
          <el-descriptions-item label="保养周期">{{ oeeData.maintainCycleDays ?? '-' }} 天</el-descriptions-item>
          <el-descriptions-item label="上次保养">{{ oeeData.lastMaintainTime ? formatDate(oeeData.lastMaintainTime) : '-' }}</el-descriptions-item>
          <el-descriptions-item label="下次保养">{{ oeeData.nextMaintainTime ? formatDate(oeeData.nextMaintainTime) : '-' }}</el-descriptions-item>
        </el-descriptions>
      </template>
      <el-empty v-else description="暂无 OEE 数据" />
      <template #footer>
        <el-button @click="oeeDialogVisible = false">关闭</el-button>
      </template>
    </el-dialog>

    <!-- 保养计划管理弹窗 -->
    <el-dialog v-model="planDialogVisible" :title="`保养计划 - ${planEquipment?.name || ''}`" width="700px">
      <div style="margin-bottom: 16px">
        <el-button type="primary" @click="showCreatePlan = true">新建计划</el-button>
      </div>

      <!-- 新建保养计划表单 -->
      <el-form v-if="showCreatePlan" ref="planFormRef" :model="planForm" :rules="planRules" label-width="100px" style="margin-bottom: 16px; padding: 16px; border: 1px solid #e4e7ed; border-radius: 4px">
        <el-form-item label="计划名称" prop="planName">
          <el-input v-model="planForm.planName" placeholder="如：月度保养、季度保养" />
        </el-form-item>
        <el-form-item label="周期(天)" prop="cycleDays">
          <el-input-number v-model="planForm.cycleDays" :min="1" :max="365" style="width: 100%" />
        </el-form-item>
        <el-form-item label="描述">
          <el-input v-model="planForm.description" type="textarea" :rows="2" placeholder="可选备注" />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" :loading="planSubmitting" @click="handleCreatePlan">保存</el-button>
          <el-button @click="showCreatePlan = false; planForm = { planName: '', cycleDays: 30, description: '' }">取消</el-button>
        </el-form-item>
      </el-form>

      <el-table :data="maintenancePlans" border stripe v-loading="plansLoading" empty-text="暂无保养计划">
        <el-table-column prop="planName" label="计划名称" min-width="140" />
        <el-table-column prop="cycleDays" label="周期(天)" width="90" />
        <el-table-column label="上次完成" width="160">
          <template #default="{ row }">
            {{ row.lastCompletedDate ? formatDate(row.lastCompletedDate) : '-' }}
          </template>
        </el-table-column>
        <el-table-column label="下次到期" width="160">
          <template #default="{ row }">
            <span :style="{ color: isPlanOverdue(row.nextDueDate) ? '#F56C6C' : '#67C23A', fontWeight: isPlanOverdue(row.nextDueDate) ? 'bold' : 'normal' }">
              {{ formatDate(row.nextDueDate) }}
            </span>
          </template>
        </el-table-column>
        <el-table-column prop="status" label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="planStatusType(row.status)">{{ planStatusLabel(row.status) }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="description" label="描述" min-width="120" show-overflow-tooltip />
        <el-table-column label="操作" width="140" fixed="right">
          <template #default="{ row }">
            <el-button size="small" type="success" :disabled="row.status === 'COMPLETED'" @click="handleCompletePlan(row)">
              完成
            </el-button>
          </template>
        </el-table-column>
      </el-table>
      <template #footer>
        <el-button @click="planDialogVisible = false">关闭</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { equipmentApi, type Equipment } from '../../api/equipment'
import { factoryApi, type Factory } from '../../api/factory'
import { workshopApi, type Workshop } from '../../api/workshop'
import { productionLineApi, type ProductionLine } from '../../api/production-line'
import { equipmentExtApi } from '../../api/equipment-ext'

const list = ref<Equipment[]>([])
const factories = ref<Factory[]>([])
const workshops = ref<Workshop[]>([])
const lines = ref<ProductionLine[]>([])
const selectedFactory = ref<number | undefined>(undefined)
const selectedWorkshop = ref<number | undefined>(undefined)
const loading = ref(false)
const dialogVisible = ref(false)
const submitting = ref(false)
const isEdit = ref(false)
const formRef = ref()

const form = ref<Equipment>({
  code: '', name: '', model: '', lineId: undefined,
  factoryId: undefined, workshopId: undefined,
  installDate: '', status: 'RUNNING',
  theoreticalCycleTime: undefined,
  plannedRunTime: undefined,
  maintainCycle: undefined
})
const rules = {
  code: [{ required: true, message: '请输入设备编码', trigger: 'blur' }],
  name: [{ required: true, message: '请输入设备名称', trigger: 'blur' }]
}

const dialogTitle = () => isEdit.value ? '编辑设备' : '新增设备'

function statusType(status?: string) {
  const map: Record<string, string> = { RUNNING: 'success', IDLE: 'info', MAINTENANCE: 'warning', BROKEN: 'danger' }
  return map[status || ''] || 'info'
}

function statusLabel(status?: string) {
  const map: Record<string, string> = { RUNNING: '运行中', IDLE: '空闲', MAINTENANCE: '保养中', BROKEN: '故障' }
  return map[status || ''] || status || '未知'
}

function oeeColor(oee?: number) {
  if (!oee) return '#909399'
  if (oee >= 0.85) return '#67C23A'
  if (oee >= 0.6) return '#E6A23C'
  return '#F56C6C'
}

function formatDate(dateStr: string | number) {
  if (!dateStr) return '-'
  const d = new Date(dateStr)
  const pad = (n: number) => n.toString().padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}`
}

async function loadData() {
  loading.value = true
  try {
    const res: any = await equipmentApi.list()
    const devices = res.data || []
    // 并行加载 OEE
    const oeePromises = devices.map(async (dev: any) => {
      try {
        const oeeRes: any = await equipmentExtApi.oee(dev.id)
        dev._oee = oeeRes.data?.oeeValue || 0
      } catch {
        dev._oee = 0
      }
    })
    await Promise.all(oeePromises)
    list.value = devices
  } finally {
    loading.value = false
  }
}

async function loadFactories() {
  try {
    const res: any = await factoryApi.list()
    factories.value = res.data || []
  } catch { /* ignore */ }
}

async function onFactoryChange(factoryId: number) {
  selectedFactory.value = factoryId
  selectedWorkshop.value = undefined
  form.value.lineId = undefined
  workshops.value = []
  lines.value = []
  if (factoryId) {
    try {
      const res: any = await workshopApi.listByFactory(factoryId)
      workshops.value = res.data || []
    } catch {
      workshops.value = []
    }
  }
}

async function onWorkshopChange(workshopId: number) {
  selectedWorkshop.value = workshopId
  form.value.lineId = undefined
  lines.value = []
  if (workshopId) {
    try {
      const res: any = await productionLineApi.listByWorkshop(workshopId)
      lines.value = res.data || []
    } catch {
      lines.value = []
    }
  }
}

function openDialog(row?: Equipment) {
  isEdit.value = !!row
  form.value = row
    ? { ...row }
    : { code: '', name: '', model: '', lineId: undefined, factoryId: undefined, workshopId: undefined, installDate: '', status: 'RUNNING', theoreticalCycleTime: undefined, plannedRunTime: undefined, maintainCycle: undefined }
  selectedFactory.value = undefined
  selectedWorkshop.value = undefined
  workshops.value = []
  lines.value = []
  dialogVisible.value = true
}

async function handleSubmit() {
  const valid = await formRef.value.validate().catch(() => false)
  if (!valid) return
  submitting.value = true
  try {
    if (isEdit.value) {
      await equipmentApi.update(form.value.id!, form.value)
      ElMessage.success('更新成功')
    } else {
      await equipmentApi.create(form.value)
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    await loadData()
  } finally {
    submitting.value = false
  }
}

async function handleDelete(row: Equipment) {
  await ElMessageBox.confirm('确定删除该设备吗？', '提示', { type: 'warning' })
  await equipmentApi.delete(row.id!)
  ElMessage.success('删除成功')
  await loadData()
}

// ======================== 设备扩展操作 ========================

const oeeDialogVisible = ref(false)
const oeeEquipment = ref<Equipment | null>(null)
const oeeData = ref<any>(null)

async function handleMaintain(row: Equipment) {
  try {
    await ElMessageBox.confirm(`确认对设备「${row.name}」进行保养操作？`, '保养确认', { type: 'warning' })
    await equipmentExtApi.maintain(row.id!)
    ElMessage.success('保养登记成功')
    await loadData()
  } catch {
    // cancelled or error
  }
}

async function handleFault(row: Equipment) {
  try {
    await ElMessageBox.confirm(`确认对设备「${row.name}」进行报修操作？`, '报修确认', { type: 'warning' })
    await equipmentExtApi.fault(row.id!)
    ElMessage.success('报修登记成功')
    await loadData()
  } catch {
    // cancelled or error
  }
}

async function viewOee(row: Equipment) {
  oeeEquipment.value = row
  oeeData.value = null
  oeeDialogVisible.value = true
  try {
    const res: any = await equipmentExtApi.oee(row.id!)
    oeeData.value = res.data || {}
  } catch {
    oeeData.value = null
  }
}

// ======================== 保养计划管理 ========================

const planDialogVisible = ref(false)
const planEquipment = ref<Equipment | null>(null)
const maintenancePlans = ref<any[]>([])
const plansLoading = ref(false)
const showCreatePlan = ref(false)
const planSubmitting = ref(false)
const planFormRef = ref()

const planForm = ref({
  planName: '',
  cycleDays: 30,
  description: ''
})

const planRules = {
  planName: [{ required: true, message: '请输入计划名称', trigger: 'blur' }],
  cycleDays: [{ required: true, message: '请输入保养周期', trigger: 'blur' }]
}

function planStatusType(status?: string) {
  const map: Record<string, string> = { PENDING: 'warning', COMPLETED: 'success', OVERDUE: 'danger' }
  return map[status || ''] || 'info'
}

function planStatusLabel(status?: string) {
  const map: Record<string, string> = { PENDING: '待处理', COMPLETED: '已完成', OVERDUE: '已逾期' }
  return map[status || ''] || status || '未知'
}

function isPlanOverdue(dateStr: string) {
  if (!dateStr) return false
  return new Date(dateStr) < new Date()
}

async function viewPlan(row: Equipment) {
  planEquipment.value = row
  maintenancePlans.value = []
  showCreatePlan.value = false
  planForm.value = { planName: '', cycleDays: 30, description: '' }
  planDialogVisible.value = true
  await loadPlans(row.id!)
}

async function loadPlans(equipmentId: number) {
  plansLoading.value = true
  try {
    const res: any = await equipmentExtApi.getMaintenancePlans(equipmentId)
    maintenancePlans.value = res.data || []
  } finally {
    plansLoading.value = false
  }
}

async function handleCreatePlan() {
  const valid = await planFormRef.value.validate().catch(() => false)
  if (!valid || !planEquipment.value?.id) return
  planSubmitting.value = true
  try {
    await equipmentExtApi.createMaintenancePlan(planEquipment.value.id!, planForm.value)
    ElMessage.success('保养计划创建成功')
    showCreatePlan.value = false
    planForm.value = { planName: '', cycleDays: 30, description: '' }
    await loadPlans(planEquipment.value.id!)
  } finally {
    planSubmitting.value = false
  }
}

async function handleCompletePlan(row: any) {
  if (!planEquipment.value?.id) return
  try {
    await ElMessageBox.confirm(`确认完成保养计划「${row.planName}」？`, '确认', { type: 'warning' })
    await equipmentExtApi.completeMaintenance(planEquipment.value.id!, row.id)
    ElMessage.success('保养完成')
    await loadPlans(planEquipment.value.id!)
    // 刷新设备列表更新状态
    await loadData()
  } catch {
    // cancelled or error
  }
}

onMounted(() => {
  loadData()
  loadFactories()
})
</script>

<style scoped>
.page-container { padding: 16px; }
.card-header { display: flex; justify-content: space-between; align-items: center; }
</style>
