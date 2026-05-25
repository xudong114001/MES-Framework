<template>
  <div class="page-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span>工位管理</span>
          <el-button type="primary" @click="openDialog()">新增工位</el-button>
        </div>
      </template>

      <el-table :data="list" border stripe v-loading="loading">
        <el-table-column prop="id" label="ID" width="80" />
        <el-table-column prop="code" label="工位编码" min-width="120" />
        <el-table-column prop="name" label="工位名称" min-width="180" />
        <el-table-column prop="lineName" label="所属产线" min-width="150" />
        <el-table-column prop="sortOrder" label="序号" width="80" />
        <el-table-column label="状态" width="80">
          <template #default="{ row }">
            <el-tag :type="row.status === 1 ? 'success' : 'info'">
              {{ row.status === 1 ? '启用' : '停用' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="180" fixed="right">
          <template #default="{ row }">
            <el-button size="small" @click="openDialog(row)">编辑</el-button>
            <el-button size="small" type="danger" @click="handleDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-dialog v-model="dialogVisible" :title="dialogTitle" width="500px">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="100px">
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
        <el-form-item label="工位编码" prop="code">
          <el-input v-model="form.code" />
        </el-form-item>
        <el-form-item label="工位名称" prop="name">
          <el-input v-model="form.name" />
        </el-form-item>
        <el-form-item label="序号" prop="sortOrder">
          <el-input-number v-model="form.sortOrder" :min="1" style="width: 100%" />
        </el-form-item>
        <el-form-item label="状态">
          <el-switch v-model="form.status" :active-value="1" :inactive-value="0" />
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
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { workstationApi, type Workstation } from '../../api/workstation'
import { factoryApi, type Factory } from '../../api/factory'
import { workshopApi, type Workshop } from '../../api/workshop'
import { productionLineApi, type ProductionLine } from '../../api/production-line'

const list = ref<Workstation[]>([])
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

const form = ref<Workstation>({ code: '', name: '', lineId: undefined, sortOrder: 1, status: 1 })
const rules = {
  code: [{ required: true, message: '请输入工位编码', trigger: 'blur' }],
  name: [{ required: true, message: '请输入工位名称', trigger: 'blur' }],
  lineId: [{ required: true, message: '请选择所属产线', trigger: 'change' }]
}

const dialogTitle = () => isEdit.value ? '编辑工位' : '新增工位'

async function loadData() {
  loading.value = true
  try {
    const res: any = await workstationApi.list()
    list.value = res.data || []
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

function openDialog(row?: Workstation) {
  isEdit.value = !!row
  form.value = row ? { ...row } : { code: '', name: '', lineId: undefined, sortOrder: 1, status: 1 }
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
      await workstationApi.update(form.value.id!, form.value)
      ElMessage.success('更新成功')
    } else {
      await workstationApi.create(form.value)
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    await loadData()
  } finally {
    submitting.value = false
  }
}

async function handleDelete(row: Workstation) {
  await ElMessageBox.confirm('确定删除该工位吗？', '提示', { type: 'warning' })
  await workstationApi.delete(row.id!)
  ElMessage.success('删除成功')
  await loadData()
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
