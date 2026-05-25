<template>
  <div class="page-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span>工艺路线</span>
        </div>
      </template>

      <div style="margin-bottom: 16px; display: flex; gap: 12px; align-items: center;">
        <span>选择物料：</span>
        <el-select
          v-model="selectedMaterialId"
          placeholder="请选择物料"
          filterable
          style="width: 300px"
          @change="onMaterialChange"
        >
          <el-option
            v-for="m in materials"
            :key="m.id"
            :label="`[${m.code}] ${m.name}`"
            :value="m.id"
          />
        </el-select>
        <el-button type="primary" :disabled="!selectedMaterialId" @click="openDialog()">
          新增工序
        </el-button>
      </div>

      <el-table :data="routingList" border stripe v-loading="loading">
        <el-table-column prop="stepOrder" label="工序号" width="80" />
        <el-table-column prop="stepName" label="工序名称" min-width="200" />
        <el-table-column prop="workstationName" label="工位" min-width="150" />
        <el-table-column prop="durationMinutes" label="工时(分钟)" width="120" />
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
      <div v-if="!selectedMaterialId" style="text-align: center; padding: 40px; color: #999;">
        请先在上方选择一个物料以查看其工艺路线
      </div>
    </el-card>

    <el-dialog v-model="dialogVisible" :title="dialogTitle" width="500px">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="120px">
        <el-form-item label="工序名称" prop="stepName">
          <el-input v-model="form.stepName" />
        </el-form-item>
        <el-form-item label="工序号" prop="stepOrder">
          <el-input-number v-model="form.stepOrder" :min="1" :step="1" style="width: 100%" />
        </el-form-item>
        <el-form-item label="工位" prop="workstationId">
          <el-select v-model="form.workstationId" placeholder="请选择工位" filterable style="width: 100%">
            <el-option
              v-for="ws in workstations"
              :key="ws.id"
              :label="ws.name"
              :value="ws.id"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="工时(分钟)" prop="durationMinutes">
          <el-input-number v-model="form.durationMinutes" :min="0" :step="1" style="width: 100%" />
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
import { routingApi, type Routing } from '../../api/routing'
import { materialApi, type Material } from '../../api/material'
import { workstationApi, type Workstation } from '../../api/workstation'

const selectedMaterialId = ref<number | undefined>(undefined)
const materials = ref<Material[]>([])
const workstations = ref<Workstation[]>([])
const routingList = ref<Routing[]>([])
const loading = ref(false)
const dialogVisible = ref(false)
const submitting = ref(false)
const isEdit = ref(false)
const formRef = ref()

const form = ref<Routing>({
  materialId: 0, stepOrder: 1, stepName: '',
  workstationId: undefined, durationMinutes: 0, status: 1
})
const rules = {
  stepName: [{ required: true, message: '请输入工序名称', trigger: 'blur' }],
  stepOrder: [{ required: true, message: '请输入工序号', trigger: 'blur' }]
}

const dialogTitle = () => isEdit.value ? '编辑工序' : '新增工序'

async function loadMaterials() {
  try {
    const res: any = await materialApi.list()
    materials.value = res.data || []
  } catch { /* ignore */ }
}

async function loadWorkstations() {
  try {
    const res: any = await workstationApi.list()
    workstations.value = res.data || []
  } catch { /* ignore */ }
}

async function onMaterialChange(materialId: number) {
  if (!materialId) {
    routingList.value = []
    return
  }
  loading.value = true
  try {
    const res: any = await routingApi.listByMaterial(materialId)
    const data = res.data || []
    routingList.value = Array.isArray(data) ? data : []
  } finally {
    loading.value = false
  }
}

function openDialog(row?: Routing) {
  isEdit.value = !!row
  form.value = row
    ? { ...row }
    : { materialId: selectedMaterialId.value!, stepOrder: 1, stepName: '', workstationId: undefined, durationMinutes: 0, status: 1 }
  dialogVisible.value = true
}

async function handleSubmit() {
  const valid = await formRef.value.validate().catch(() => false)
  if (!valid) return
  submitting.value = true
  try {
    const data = { ...form.value, materialId: selectedMaterialId.value! }
    if (isEdit.value) {
      await routingApi.update(data.id!, data)
      ElMessage.success('更新成功')
    } else {
      await routingApi.create(data)
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    await onMaterialChange(selectedMaterialId.value!)
  } finally {
    submitting.value = false
  }
}

async function handleDelete(row: Routing) {
  await ElMessageBox.confirm('确定删除该工序吗？', '提示', { type: 'warning' })
  await routingApi.delete(row.id!)
  ElMessage.success('删除成功')
  await onMaterialChange(selectedMaterialId.value!)
}

onMounted(() => {
  loadMaterials()
  loadWorkstations()
})
</script>

<style scoped>
.page-container { padding: 16px; }
.card-header { display: flex; justify-content: space-between; align-items: center; }
</style>
