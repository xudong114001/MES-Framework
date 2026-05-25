<template>
  <div class="page-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span>报工管理</span>
          <el-button type="primary" @click="openCreateDialog">新增报工</el-button>
        </div>
      </template>

      <el-table :data="list" border stripe v-loading="loading">
        <el-table-column prop="reportNo" label="报工单号" min-width="140" />
        <el-table-column prop="workOrderId" label="工单号" min-width="100" />
        <el-table-column prop="stepId" label="工序" min-width="80" />
        <el-table-column prop="operatorId" label="操作工" min-width="100" />
        <el-table-column label="报工类型" width="120">
          <template #default="{ row }">
            <el-tag :type="reportTypeTag(row.reportType)">
              {{ reportTypeLabel(row.reportType) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="goodQty" label="合格数" width="80" align="right" />
        <el-table-column prop="scrapQty" label="报废数" width="80" align="right" />
        <el-table-column prop="reworkQty" label="返工数" width="80" align="right" />
        <el-table-column prop="durationMin" label="工时(min)" width="100" align="right" />
        <el-table-column prop="reportTime" label="报工时间" width="160" />
        <el-table-column label="操作" width="100" fixed="right">
          <template #default="{ row }">
            <el-button size="small" @click="viewDetail(row)">详情</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 新增报工弹窗 -->
    <el-dialog v-model="dialogVisible" title="新增报工" width="500px">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="100px">
        <el-form-item label="工单号" prop="workOrderId">
          <el-input-number v-model="form.workOrderId" :min="1" style="width: 100%" />
        </el-form-item>
        <el-form-item label="工序" prop="stepId">
          <el-input-number v-model="form.stepId" :min="1" style="width: 100%" />
        </el-form-item>
        <el-form-item label="报工类型" prop="reportType">
          <el-select v-model="form.reportType" style="width: 100%">
            <el-option label="完工" value="COMPLETE" />
            <el-option label="返工" value="REWORK" />
            <el-option label="报废" value="SCRAP" />
          </el-select>
        </el-form-item>
        <el-form-item label="合格数量" prop="goodQty">
          <el-input-number v-model="form.goodQty" :min="0" style="width: 100%" />
        </el-form-item>
        <el-form-item label="报废数量" prop="scrapQty">
          <el-input-number v-model="form.scrapQty" :min="0" style="width: 100%" />
        </el-form-item>
        <el-form-item label="返工数量" prop="reworkQty">
          <el-input-number v-model="form.reworkQty" :min="0" style="width: 100%" />
        </el-form-item>
        <el-form-item label="工时(min)" prop="durationMin">
          <el-input-number v-model="form.durationMin" :min="0" style="width: 100%" />
        </el-form-item>
        <el-form-item label="备注" prop="remark">
          <el-input v-model="form.remark" type="textarea" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSubmit" :loading="submitting">保存</el-button>
      </template>
    </el-dialog>

    <!-- 详情弹窗 -->
    <el-dialog v-model="detailVisible" title="报工详情" width="500px">
      <el-descriptions :column="2" border>
        <el-descriptions-item label="报工单号">{{ detail.reportNo }}</el-descriptions-item>
        <el-descriptions-item label="工单号">{{ detail.workOrderId }}</el-descriptions-item>
        <el-descriptions-item label="工序">{{ detail.stepId }}</el-descriptions-item>
        <el-descriptions-item label="操作工">{{ detail.operatorId }}</el-descriptions-item>
        <el-descriptions-item label="报工类型">
          <el-tag :type="reportTypeTag(detail.reportType)">{{ reportTypeLabel(detail.reportType) }}</el-tag>
        </el-descriptions-item>
        <el-descriptions-item label="合格数">{{ detail.goodQty }}</el-descriptions-item>
        <el-descriptions-item label="报废数">{{ detail.scrapQty }}</el-descriptions-item>
        <el-descriptions-item label="返工数">{{ detail.reworkQty }}</el-descriptions-item>
        <el-descriptions-item label="工时(min)">{{ detail.durationMin }}</el-descriptions-item>
        <el-descriptions-item label="报工时间">{{ detail.reportTime }}</el-descriptions-item>
        <el-descriptions-item label="备注" :span="2">{{ detail.remark }}</el-descriptions-item>
      </el-descriptions>
      <template #footer>
        <el-button @click="detailVisible = false">关闭</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { workReportApi, type WorkReport } from '../../api/work-report'

const list = ref<WorkReport[]>([])
const loading = ref(false)
const dialogVisible = ref(false)
const detailVisible = ref(false)
const submitting = ref(false)
const formRef = ref()
const detail = ref<WorkReport>({} as WorkReport)

const form = ref<WorkReport>({
  workOrderId: 0,
  stepId: undefined,
  reportType: 'COMPLETE',
  goodQty: 0,
  scrapQty: 0,
  reworkQty: 0,
  durationMin: undefined,
  remark: ''
})

const rules = {
  workOrderId: [{ required: true, message: '请输入工单号', trigger: 'blur' }],
  reportType: [{ required: true, message: '请选择报工类型', trigger: 'change' }]
}

function reportTypeTag(type?: string) {
  const map: Record<string, string> = { COMPLETE: 'success', REWORK: 'warning', SCRAP: 'danger' }
  return map[type || ''] || 'info'
}

function reportTypeLabel(type?: string) {
  const map: Record<string, string> = { COMPLETE: '完工', REWORK: '返工', SCRAP: '报废' }
  return map[type || ''] || type || '未知'
}

async function loadData() {
  loading.value = true
  try {
    const res: any = await workReportApi.list()
    list.value = res.data || []
  } finally {
    loading.value = false
  }
}

function openCreateDialog() {
  form.value = {
    workOrderId: 0,
    stepId: undefined,
    reportType: 'COMPLETE',
    goodQty: 0,
    scrapQty: 0,
    reworkQty: 0,
    durationMin: undefined,
    remark: ''
  }
  dialogVisible.value = true
}

async function handleSubmit() {
  const valid = await formRef.value.validate().catch(() => false)
  if (!valid) return
  submitting.value = true
  try {
    await workReportApi.create(form.value)
    ElMessage.success('报工成功')
    dialogVisible.value = false
    await loadData()
  } finally {
    submitting.value = false
  }
}

async function viewDetail(row: WorkReport) {
  try {
    const res: any = await workReportApi.getById(row.id!)
    detail.value = res.data || row
    detailVisible.value = true
  } catch {
    detail.value = row
    detailVisible.value = true
  }
}

onMounted(() => {
  loadData()
})
</script>

<style scoped>
.page-container { padding: 16px; }
.card-header { display: flex; justify-content: space-between; align-items: center; }
</style>
