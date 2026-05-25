<template>
  <div class="page-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span>质检检查点</span>
          <el-button type="primary" @click="dialogVisible = true">新增检查点</el-button>
        </div>
      </template>

      <!-- 筛选条件 -->
      <el-form :inline="true" style="margin-bottom:12px">
        <el-form-item label="工艺路线">
          <el-select v-model="routingId" placeholder="选择工艺路线" clearable @change="onRoutingChange" style="width:200px">
            <el-option v-for="r in routings" :key="r.id" :label="r.name" :value="r.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="工序">
          <el-select v-model="filterStepId" placeholder="按工序筛选" clearable style="width:200px">
            <el-option v-for="s in steps" :key="s.id" :label="s.stepName" :value="s.id" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="fetchList" :loading="loading">查询</el-button>
        </el-form-item>
      </el-form>

      <el-table :data="list" v-loading="loading" border stripe>
        <el-table-column prop="id" label="ID" width="60" />
        <el-table-column prop="stepName" label="工序名称" min-width="160" />
        <el-table-column prop="checkType" label="检查类型" width="120">
          <template #default="{ row }">
            <el-tag :type="checkTypeTag(row.checkType)" size="small">{{ checkTypeLabel(row.checkType) }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="isMandatory" label="强制检查" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="row.isMandatory ? 'danger' : 'info'" size="small">{{ row.isMandatory ? '是' : '否' }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="remark" label="备注" min-width="200" show-overflow-tooltip />
        <el-table-column label="操作" width="120" fixed="right">
          <template #default="{ row }">
            <el-button size="small" type="danger" @click="handleDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
      <el-empty v-if="!loading && list.length === 0" description="暂无数据" />
    </el-card>

    <!-- 新增/编辑对话框 -->
    <el-dialog v-model="dialogVisible" :title="isEditing ? '编辑检查点' : '新增检查点'" width="500px" :close-on-click-modal="false">
      <el-form ref="formRef" :model="form" :rules="formRules" label-width="110px">
        <el-form-item label="工艺路线" prop="routingId">
          <el-select v-model="form.routingId" placeholder="选择工艺路线" style="width:100%" @change="onFormRoutingChange">
            <el-option v-for="r in routings" :key="r.id" :label="r.name" :value="r.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="工序" prop="stepId">
          <el-select v-model="form.stepId" placeholder="请选择工序" style="width:100%">
            <el-option v-for="s in formSteps" :key="s.id" :label="s.stepName" :value="s.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="检查类型" prop="checkType">
          <el-select v-model="form.checkType" placeholder="请选择检查类型" style="width:100%">
            <el-option label="首检" value="FIRST_PIECE" />
            <el-option label="抽检" value="SPOT_CHECK" />
            <el-option label="全检" value="FULL_INSPECTION" />
            <el-option label="巡检" value="PATROL" />
            <el-option label="末检" value="FINAL_PIECE" />
          </el-select>
        </el-form-item>
        <el-form-item label="强制检查">
          <el-switch v-model="form.isMandatory" />
        </el-form-item>
        <el-form-item label="备注">
          <el-input v-model="form.remark" type="textarea" :rows="2" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSubmit" :loading="submitting">确认</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { qcCheckpointApi } from '../../api/qc-checkpoint'

const loading = ref(false)
const list = ref<any[]>([])
const dialogVisible = ref(false)
const submitting = ref(false)
const isEditing = ref(false)

// 工艺路线和工序
const routings = ref<any[]>([])
const steps = ref<any[]>([])
const formSteps = ref<any[]>([])

// 筛选
const routingId = ref<number | undefined>(undefined)
const filterStepId = ref<number | undefined>(undefined)

const formRef = ref()
const form = reactive({
  routingId: undefined as number | undefined,
  stepId: undefined as number | undefined,
  checkType: 'SPOT_CHECK',
  isMandatory: false,
  remark: ''
})

const formRules = {
  stepId: [{ required: true, message: '请选择工序', trigger: 'change' }],
  checkType: [{ required: true, message: '请选择检查类型', trigger: 'change' }]
}

function checkTypeLabel(type: string): string {
  const map: Record<string, string> = {
    FIRST_PIECE: '首检', SPOT_CHECK: '抽检',
    FULL_INSPECTION: '全检', PATROL: '巡检', FINAL_PIECE: '末检'
  }
  return map[type] || type
}

function checkTypeTag(type: string): string {
  const map: Record<string, string> = {
    FIRST_PIECE: '', SPOT_CHECK: 'primary',
    FULL_INSPECTION: 'success', PATROL: 'warning', FINAL_PIECE: 'info'
  }
  return map[type] || ''
}

async function loadRoutings() {
  try {
    const res: any = await fetch('/api/v1/routings').then(r => r.json())
    routings.value = res.data || []
  } catch {}
}

async function loadStepsByRouting(routingId: number): Promise<any[]> {
  try {
    const res: any = await fetch(`/api/v1/routings/${routingId}/steps`).then(r => r.json())
    return res.data || []
  } catch {
    return []
  }
}

async function onRoutingChange() {
  filterStepId.value = undefined
  if (routingId.value) {
    steps.value = await loadStepsByRouting(routingId.value)
  } else {
    steps.value = []
  }
}

async function onFormRoutingChange() {
  form.stepId = undefined
  if (form.routingId) {
    formSteps.value = await loadStepsByRouting(form.routingId)
  } else {
    formSteps.value = []
  }
}

async function fetchList() {
  loading.value = true
  try {
    let res: any
    if (filterStepId.value) {
      res = await qcCheckpointApi.listByStep(filterStepId.value)
    } else {
      res = await qcCheckpointApi.list()
    }
    list.value = res.data || []
  } finally {
    loading.value = false
  }
}

async function handleSubmit() {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return
  submitting.value = true
  try {
    await qcCheckpointApi.create({
      stepId: form.stepId!,
      checkType: form.checkType,
      isMandatory: form.isMandatory,
      remark: form.remark
    })
    ElMessage.success('检查点创建成功')
    dialogVisible.value = false
    resetForm()
    await fetchList()
  } catch (e: any) {
    ElMessage.error(e.message || '创建失败')
  } finally {
    submitting.value = false
  }
}

function resetForm() {
  form.routingId = undefined
  form.stepId = undefined
  form.checkType = 'SPOT_CHECK'
  form.isMandatory = false
  form.remark = ''
  formSteps.value = []
}

async function handleDelete(row: any) {
  try {
    await ElMessageBox.confirm(`确认删除检查点「${row.stepName} - ${checkTypeLabel(row.checkType)}」？`, '警告', { type: 'warning' })
    await qcCheckpointApi.delete(row.id!)
    ElMessage.success('删除成功')
    await fetchList()
  } catch {}
}

onMounted(async () => {
  await loadRoutings()
  await fetchList()
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
</style>
