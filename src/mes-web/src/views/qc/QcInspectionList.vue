<template>
  <div class="page-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span>质量管理</span>
          <el-button type="primary" @click="openCreateDialog">新增质检单</el-button>
        </div>
      </template>

      <el-table :data="list" border stripe v-loading="loading">
        <el-table-column prop="inspectNo" label="质检单号" min-width="140" />
        <el-table-column label="来源类型" width="120">
          <template #default="{ row }">
            {{ sourceTypeLabel(row.sourceType) }}
          </template>
        </el-table-column>
        <el-table-column prop="sourceRef" label="来源单号" min-width="120" />
        <el-table-column prop="inspector" label="质检员" width="100" />
        <el-table-column label="结果" width="100">
          <template #default="{ row }">
            <el-tag v-if="row.inspectResult === 'PASS'" type="success">合格</el-tag>
            <el-tag v-else-if="row.inspectResult === 'FAIL'" type="danger">不合格</el-tag>
            <el-tag v-else type="info">待检</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="inspectTime" label="质检时间" width="160" />
        <el-table-column label="操作" width="100" fixed="right">
          <template #default="{ row }">
            <el-button size="small" @click="viewDetail(row)">详情</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 新增质检单弹窗 -->
    <el-dialog v-model="createDialogVisible" title="新增质检单" width="500px">
      <el-form ref="createFormRef" :model="createForm" :rules="createRules" label-width="100px">
        <el-form-item label="来源类型" prop="sourceType">
          <el-select v-model="createForm.sourceType" style="width: 100%">
            <el-option label="来料检" value="INCOMING" />
            <el-option label="首件检" value="FIRST_ARTICLE" />
            <el-option label="过程检" value="IN_PROCESS" />
            <el-option label="完工检" value="FINAL" />
          </el-select>
        </el-form-item>
        <el-form-item label="来源单号" prop="sourceRef">
          <el-input v-model="createForm.sourceRef" />
        </el-form-item>
        <el-form-item label="工单号" prop="workOrderId">
          <el-input-number v-model="createForm.workOrderId" :min="1" style="width: 100%" />
        </el-form-item>
        <el-form-item label="物料ID" prop="materialId">
          <el-input-number v-model="createForm.materialId" :min="1" style="width: 100%" />
        </el-form-item>
        <el-form-item label="质检员" prop="inspector">
          <el-input v-model="createForm.inspector" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="createDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleCreate" :loading="submitting">保存</el-button>
      </template>
    </el-dialog>

    <!-- 详情弹窗 -->
    <el-dialog v-model="detailDialogVisible" title="质检详情" width="650px">
      <template v-if="detail">
        <el-descriptions :column="2" border style="margin-bottom: 16px">
          <el-descriptions-item label="质检单号">{{ detail.inspectNo }}</el-descriptions-item>
          <el-descriptions-item label="来源类型">{{ sourceTypeLabel(detail.sourceType) }}</el-descriptions-item>
          <el-descriptions-item label="来源单号">{{ detail.sourceRef }}</el-descriptions-item>
          <el-descriptions-item label="工单号">{{ detail.workOrderId }}</el-descriptions-item>
          <el-descriptions-item label="物料ID">{{ detail.materialId }}</el-descriptions-item>
          <el-descriptions-item label="质检员">{{ detail.inspector }}</el-descriptions-item>
          <el-descriptions-item label="结果">
            <el-tag v-if="detail.inspectResult === 'PASS'" type="success">合格</el-tag>
            <el-tag v-else-if="detail.inspectResult === 'FAIL'" type="danger">不合格</el-tag>
            <el-tag v-else type="info">待检</el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="质检时间">{{ detail.inspectTime }}</el-descriptions-item>
        </el-descriptions>

        <el-divider />

        <div class="section-header">
          <span>质检项目</span>
          <el-button size="small" type="primary" @click="openAddItemDialog">添加项目</el-button>
          <el-button
            size="small"
            type="success"
            :disabled="detail.inspectResult === 'PASS' || detail.inspectResult === 'FAIL'"
            @click="handleVerify('PASS')"
          >
            判定合格
          </el-button>
          <el-button
            size="small"
            type="danger"
            :disabled="detail.inspectResult === 'PASS' || detail.inspectResult === 'FAIL'"
            @click="handleVerify('FAIL')"
          >
            判定不合格
          </el-button>
        </div>

        <el-table :data="detail.items || []" border stripe>
          <el-table-column prop="itemName" label="检验项" min-width="120" />
          <el-table-column prop="specValue" label="标准值" width="120" />
          <el-table-column prop="actualValue" label="实测值" width="120" />
          <el-table-column label="结果" width="80">
            <template #default="{ row: item }">
              <el-tag v-if="item.result === 'PASS'" type="success" size="small">合格</el-tag>
              <el-tag v-else-if="item.result === 'FAIL'" type="danger" size="small">不合格</el-tag>
              <el-tag v-else type="info" size="small">待检</el-tag>
            </template>
          </el-table-column>
        </el-table>
      </template>
      <template #footer>
        <el-button @click="detailDialogVisible = false">关闭</el-button>
      </template>
    </el-dialog>

    <!-- 添加质检项弹窗 -->
    <el-dialog v-model="itemDialogVisible" title="添加质检项目" width="450px">
      <el-form ref="itemFormRef" :model="itemForm" :rules="itemRules" label-width="90px">
        <el-form-item label="检验项" prop="itemName">
          <el-input v-model="itemForm.itemName" />
        </el-form-item>
        <el-form-item label="标准值" prop="specValue">
          <el-input v-model="itemForm.specValue" />
        </el-form-item>
        <el-form-item label="实测值" prop="actualValue">
          <el-input v-model="itemForm.actualValue" />
        </el-form-item>
        <el-form-item label="结果" prop="result">
          <el-select v-model="itemForm.result" style="width: 100%">
            <el-option label="合格" value="PASS" />
            <el-option label="不合格" value="FAIL" />
            <el-option label="待检" value="PENDING" />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="itemDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleAddItem" :loading="itemSubmitting">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { qcApi, type QcInspection, type QcInspectionItem } from '../../api/qc'

const list = ref<QcInspection[]>([])
const loading = ref(false)
const createDialogVisible = ref(false)
const detailDialogVisible = ref(false)
const itemDialogVisible = ref(false)
const submitting = ref(false)
const itemSubmitting = ref(false)
const createFormRef = ref()
const itemFormRef = ref()
const detail = ref<QcInspection>({} as QcInspection)

const createForm = ref<QcInspection>({
  sourceType: 'INCOMING',
  sourceRef: '',
  workOrderId: undefined,
  materialId: undefined,
  inspector: ''
})

const createRules = {
  sourceType: [{ required: true, message: '请选择来源类型', trigger: 'change' }],
  inspector: [{ required: true, message: '请输入质检员', trigger: 'blur' }]
}

const itemForm = ref<QcInspectionItem>({
  itemName: '',
  specValue: '',
  actualValue: '',
  result: 'PENDING'
})

const itemRules = {
  itemName: [{ required: true, message: '请输入检验项名称', trigger: 'blur' }]
}

function sourceTypeLabel(type?: string) {
  const map: Record<string, string> = { INCOMING: '来料检', FIRST_ARTICLE: '首件检', IN_PROCESS: '过程检', FINAL: '完工检' }
  return map[type || ''] || type || '未知'
}

async function loadData() {
  loading.value = true
  try {
    const res: any = await qcApi.list()
    list.value = res.data || []
  } finally {
    loading.value = false
  }
}

function openCreateDialog() {
  createForm.value = {
    sourceType: 'INCOMING',
    sourceRef: '',
    workOrderId: undefined,
    materialId: undefined,
    inspector: ''
  }
  createDialogVisible.value = true
}

async function handleCreate() {
  const valid = await createFormRef.value.validate().catch(() => false)
  if (!valid) return
  submitting.value = true
  try {
    await qcApi.create(createForm.value)
    ElMessage.success('创建质检单成功')
    createDialogVisible.value = false
    await loadData()
  } finally {
    submitting.value = false
  }
}

async function viewDetail(row: QcInspection) {
  try {
    const res: any = await qcApi.getById(row.id!)
    detail.value = res.data || row
    detailDialogVisible.value = true
  } catch {
    detail.value = row
    detailDialogVisible.value = true
  }
}

function openAddItemDialog() {
  itemForm.value = { itemName: '', specValue: '', actualValue: '', result: 'PENDING' }
  itemDialogVisible.value = true
}

async function handleAddItem() {
  const valid = await itemFormRef.value.validate().catch(() => false)
  if (!valid) return
  if (!detail.value.id) {
    ElMessage.warning('请先保存质检单')
    return
  }
  itemSubmitting.value = true
  try {
    await qcApi.addItem(detail.value.id, itemForm.value)
    ElMessage.success('添加质检项成功')
    itemDialogVisible.value = false
    // 重新加载详情
    const res: any = await qcApi.getById(detail.value.id)
    detail.value = res.data || detail.value
  } finally {
    itemSubmitting.value = false
  }
}

async function handleVerify(result: string) {
  if (!detail.value.id) return
  try {
    await qcApi.verify(detail.value.id, result)
    ElMessage.success(result === 'PASS' ? '判定合格' : '判定不合格')
    const res: any = await qcApi.getById(detail.value.id)
    detail.value = res.data || detail.value
    await loadData()
  } catch {
    // error handled by interceptor
  }
}

onMounted(() => {
  loadData()
})
</script>

<style scoped>
.page-container { padding: 16px; }
.card-header { display: flex; justify-content: space-between; align-items: center; }
.section-header { display: flex; align-items: center; gap: 8px; margin-bottom: 12px; font-weight: bold; font-size: 15px; }
</style>
