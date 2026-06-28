<template>
  <div class="page-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span>产线管理</span>
          <el-button type="primary" @click="openDialog()">新增产线</el-button>
        </div>
      </template>

      <el-table :data="list" border stripe v-loading="loading">
        <el-table-column prop="id" label="ID" width="80" />
        <el-table-column prop="code" label="产线编码" min-width="120" />
        <el-table-column prop="name" label="产线名称" min-width="200" />
        <el-table-column prop="workshopName" label="所属车间" min-width="150" />
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
        <el-form-item label="所属车间" prop="workshopId">
          <el-select v-model="form.workshopId" placeholder="请选择车间" style="width: 100%" :disabled="!selectedFactory">
            <el-option v-for="w in workshops" :key="w.id" :label="w.name" :value="w.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="产线编码" prop="code">
          <el-input v-model="form.code" />
        </el-form-item>
        <el-form-item label="产线名称" prop="name">
          <el-input v-model="form.name" />
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
import { productionLineApi, type ProductionLine } from '../../api/production-line'
import { factoryApi, type Factory } from '../../api/factory'
import { workshopApi, type Workshop } from '../../api/workshop'

const list = ref<ProductionLine[]>([])
const factories = ref<Factory[]>([])
const workshops = ref<Workshop[]>([])
const selectedFactory = ref<number | undefined>(undefined)
const loading = ref(false)
const dialogVisible = ref(false)
const submitting = ref(false)
const isEdit = ref(false)
const formRef = ref()

const form = ref<ProductionLine>({ code: '', name: '', workshopId: undefined, status: 1 })
const rules = {
  code: [{ required: true, message: '请输入产线编码', trigger: 'blur' }],
  name: [{ required: true, message: '请输入产线名称', trigger: 'blur' }],
  workshopId: [{ required: true, message: '请选择所属车间', trigger: 'change' }]
}

const dialogTitle = () => isEdit.value ? '编辑产线' : '新增产线'

async function loadData() {
  loading.value = true
  try {
    const res: any = await productionLineApi.list()
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
  form.value.workshopId = undefined
  if (factoryId) {
    try {
      const res: any = await workshopApi.listByFactory(factoryId)
      workshops.value = res.data || []
    } catch {
      workshops.value = []
    }
  } else {
    workshops.value = []
  }
}

function openDialog(row?: ProductionLine) {
  isEdit.value = !!row
  form.value = row ? { ...row } : { code: '', name: '', workshopId: undefined, status: 1 }
  selectedFactory.value = undefined
  workshops.value = []
  dialogVisible.value = true
  // 编辑模式：回显工厂/车间级联选择
  if (row?.workshopId) {
    restoreFactoryAndWorkshop(row)
  }
}

async function restoreFactoryAndWorkshop(row: ProductionLine) {
  // 从车间列表反查所属工厂
  try {
    const factoryRes: any = await factoryApi.list()
    const factoryList = factoryRes.data || []
    for (const f of factoryList) {
      const workshopRes: any = await workshopApi.listByFactory(f.id)
      const ws = workshopRes.data || []
      if (ws.some((w: any) => w.id === row.workshopId)) {
        selectedFactory.value = f.id
        workshops.value = ws
        break
      }
    }
  } catch {
    // 回显失败不影响编辑
  }
}

async function handleSubmit() {
  const valid = await formRef.value.validate().catch(() => false)
  if (!valid) return
  submitting.value = true
  try {
    if (isEdit.value) {
      await productionLineApi.update(form.value.id!, form.value)
      ElMessage.success('更新成功')
    } else {
      await productionLineApi.create(form.value)
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    await loadData()
  } finally {
    submitting.value = false
  }
}

async function handleDelete(row: ProductionLine) {
  await ElMessageBox.confirm('确定删除该产线吗？', '提示', { type: 'warning' })
  await productionLineApi.delete(row.id!)
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
