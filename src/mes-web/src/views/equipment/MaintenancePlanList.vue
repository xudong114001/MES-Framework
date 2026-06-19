<template>
  <div class="page-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span>设备保养计划</span>
          <el-button type="primary" @click="openDialog()">新建计划</el-button>
        </div>
      </template>

      <!-- 筛选条件 -->
      <div class="filter-bar">
        <el-input v-model="filters.equipmentName" placeholder="设备名称" clearable style="width: 200px; margin-right: 10px" />
        <el-select v-model="filters.status" placeholder="保养状态" clearable style="width: 150px; margin-right: 10px">
          <el-option label="待保养" value="PENDING" />
          <el-option label="已完成" value="COMPLETED" />
          <el-option label="已逾期" value="OVERDUE" />
        </el-select>
        <el-date-picker
          v-model="filters.planDate"
          type="daterange"
          range-separator="至"
          start-placeholder="计划开始日期"
          end-placeholder="计划结束日期"
          value-format="YYYY-MM-DD"
          style="width: 260px; margin-right: 10px"
        />
        <el-button type="primary" @click="loadData()">查询</el-button>
        <el-button @click="resetFilters()">重置</el-button>
      </div>

      <!-- 表格 -->
      <el-table :data="list" border stripe v-loading="loading">
        <el-table-column prop="id" label="ID" width="80" />
        <el-table-column prop="planName" label="计划名称" min-width="150" />
        <el-table-column prop="equipmentName" label="关联设备" min-width="150" />
        <el-table-column prop="cycleDays" label="周期(天)" width="100" align="center" />
        <el-table-column label="上次完成日期" width="160">
          <template #default="{ row }">
            {{ row.lastCompletedDate ? formatDate(row.lastCompletedDate) : '-' }}
          </template>
        </el-table-column>
        <el-table-column label="下次到期日期" width="160">
          <template #default="{ row }">
            <span :class="{ 'overdue-date': isOverdue(row.nextDueDate) }">
              {{ row.nextDueDate ? formatDate(row.nextDueDate) : '-' }}
            </span>
          </template>
        </el-table-column>
        <el-table-column label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="statusType(row.status)">{{ statusLabel(row.status) }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="创建时间" width="160">
          <template #default="{ row }">
            {{ row.createdAt ? formatDate(row.createdAt) : '-' }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="200" fixed="right">
          <template #default="{ row }">
            <el-button size="small" @click="openDialog(row)">编辑</el-button>
            <el-button size="small" type="success" :disabled="row.status === 'COMPLETED'" @click="handleComplete(row)">
              完成保养
            </el-button>
            <el-button size="small" type="danger" @click="handleDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 新增/编辑保养计划弹窗 -->
    <el-dialog v-model="dialogVisible" :title="dialogTitle" width="550px">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="100px">
        <el-form-item label="选择设备" prop="equipmentId">
          <el-select v-model="form.equipmentId" placeholder="请选择设备" style="width: 100%" filterable>
            <el-option v-for="eq in equipmentList" :key="eq.id" :label="`${eq.code} - ${eq.name}`" :value="eq.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="计划名称" prop="planName">
          <el-input v-model="form.planName" placeholder="如：月度保养、季度保养" />
        </el-form-item>
        <el-form-item label="周期(天)" prop="cycleDays">
          <el-input-number v-model="form.cycleDays" :min="1" :max="365" style="width: 100%" />
        </el-form-item>
        <el-form-item label="描述">
          <el-input v-model="form.description" type="textarea" :rows="3" placeholder="可选备注" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSubmit" :loading="submitting">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { equipmentExtApi, type MaintenancePlan } from '../../api/equipment-ext'

interface Equipment {
  id?: number
  code: string
  name: string
}

const list = ref<any[]>([])
const equipmentList = ref<Equipment[]>([])
const loading = ref(false)
const dialogVisible = ref(false)
const submitting = ref(false)
const isEdit = ref(false)
const formRef = ref()

const filters = ref({
  equipmentName: '',
  status: '',
  planDate: [] as string[]
})

const form = ref<MaintenancePlan>({
  equipmentId: undefined,
  planName: '',
  cycleDays: 30,
  description: ''
})

const rules = {
  equipmentId: [{ required: true, message: '请选择设备', trigger: 'change' }],
  planName: [{ required: true, message: '请输入计划名称', trigger: 'blur' }],
  cycleDays: [{ required: true, message: '请输入���养周期', trigger: 'blur' }]
}

const dialogTitle = computed(() => (isEdit.value ? '编辑保养计划' : '新建保养计划'))

function statusType(status?: string) {
  const map: Record<string, string> = { PENDING: 'warning', COMPLETED: 'success', OVERDUE: 'danger' }
  return map[status || ''] || 'info'
}

function statusLabel(status?: string) {
  const map: Record<string, string> = { PENDING: '待保养', COMPLETED: '已完成', OVERDUE: '已逾期' }
  return map[status || ''] || status || '未知'
}

function formatDate(dateStr: string | number) {
  if (!dateStr) return '-'
  const d = new Date(dateStr)
  const pad = (n: number) => n.toString().padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}`
}

function isOverdue(dateStr: string) {
  if (!dateStr) return false
  return new Date(dateStr) < new Date()
}

async function loadData() {
  loading.value = true
  try {
    const params: { equipmentName?: string; status?: string } = {}
    if (filters.value.equipmentName) params.equipmentName = filters.value.equipmentName
    if (filters.value.status) params.status = filters.value.status

    const res: any = await equipmentExtApi.getAllMaintenancePlans(params)
    const plans = res.data || []

    // 获取设备名称映射
    const equipmentMap = new Map(equipmentList.value.map(e => [e.id, e.name]))

    // 添加设备名称到计划对象
    list.value = plans.map((plan: any) => ({
      ...plan,
      equipmentName: equipmentMap.get(plan.equipmentId) || `设备ID:${plan.equipmentId}`
    }))

    // 客户端日期筛选（如果选择了日期范围）
    if (filters.value.planDate && filters.value.planDate.length === 2) {
      const [startDate, endDate] = filters.value.planDate
      list.value = list.value.filter((plan: any) => {
        if (!plan.nextDueDate) return false
        const dueDate = new Date(plan.nextDueDate)
        return dueDate >= new Date(startDate) && dueDate <= new Date(endDate)
      })
    }
  } finally {
    loading.value = false
  }
}

async function loadEquipmentList() {
  try {
    const res: any = await equipmentExtApi.getEquipmentList()
    equipmentList.value = res.data || []
  } catch {
    equipmentList.value = []
  }
}

function resetFilters() {
  filters.value = { equipmentName: '', status: '', planDate: [] }
  loadData()
}

function openDialog(row?: MaintenancePlan) {
  isEdit.value = !!row
  form.value = row
    ? { ...row }
    : { equipmentId: undefined, planName: '', cycleDays: 30, description: '' }
  dialogVisible.value = true
}

async function handleSubmit() {
  const valid = await formRef.value.validate().catch(() => false)
  if (!valid) return

  submitting.value = true
  try {
    if (isEdit.value) {
      await equipmentExtApi.updateMaintenancePlan(form.value.id!, {
        planName: form.value.planName,
        cycleDays: form.value.cycleDays,
        description: form.value.description
      })
      ElMessage.success('更新成功')
    } else {
      await equipmentExtApi.createMaintenancePlan(form.value.equipmentId!, {
        planName: form.value.planName,
        cycleDays: form.value.cycleDays,
        description: form.value.description
      })
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    await loadData()
  } finally {
    submitting.value = false
  }
}

async function handleDelete(row: MaintenancePlan) {
  await ElMessageBox.confirm('确定删除该保养计划吗？', '提示', { type: 'warning' })
  await equipmentExtApi.deleteMaintenancePlan(row.id!)
  ElMessage.success('删除成功')
  await loadData()
}

async function handleComplete(row: MaintenancePlan) {
  if (!row.equipmentId || !row.id) return
  try {
    await ElMessageBox.confirm(`确认完成保养计划「${row.planName}」？`, '确认', { type: 'warning' })
    await equipmentExtApi.completeMaintenance(row.equipmentId, row.id)
    ElMessage.success('保养完成')
    await loadData()
  } catch {
    // cancelled or error
  }
}

onMounted(() => {
  loadEquipmentList().then(() => loadData())
})
</script>

<style scoped>
.page-container { padding: 16px; }
.card-header { display: flex; justify-content: space-between; align-items: center; }
.filter-bar { margin-bottom: 16px; display: flex; align-items: center; flex-wrap: wrap; }
.overdue-date { color: #F56C6C; font-weight: bold; }
</style>