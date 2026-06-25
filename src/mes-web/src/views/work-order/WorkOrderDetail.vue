<template>
  <div class="page-container">
    <el-card v-if="isCreate">
      <template #header>
        <div class="card-header">
          <span>新增工单</span>
          <el-button @click="goBack">返回</el-button>
        </div>
      </template>
      <el-form ref="formRef" :model="form" :rules="formRules" label-width="120px" style="max-width: 600px">
        <el-form-item label="物料" prop="materialId">
          <el-select v-model="form.materialId" placeholder="请选择物料" filterable remote :remote-method="materialSearch" @change="onMaterialChange" style="width: 100%">
            <el-option v-for="m in materials" :key="m.id" :label="`${m.code} - ${m.name}`" :value="m.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="工艺路线" prop="routingId">
          <el-select v-model="form.routingId" placeholder="请选择工艺路线" style="width: 100%">
            <el-option v-for="r in routings" :key="r.id" :label="r.name" :value="r.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="计划数量" prop="plannedQty">
          <el-input-number v-model="form.plannedQty" :min="1" style="width: 100%" />
        </el-form-item>
        <el-form-item label="计划开始" prop="planStartTime">
          <el-date-picker v-model="form.planStartTime" type="datetime" placeholder="选择开始时间" value-format="YYYY-MM-DDTHH:mm:ss" style="width: 100%" />
        </el-form-item>
        <el-form-item label="计划结束" prop="planEndTime">
          <el-date-picker v-model="form.planEndTime" type="datetime" placeholder="选择结束时间" value-format="YYYY-MM-DDTHH:mm:ss" style="width: 100%" />
        </el-form-item>
        <el-form-item label="优先级">
          <el-slider v-model="form.priority" :min="0" :max="100" :step="10" show-stops show-input style="width: 100%" />
        </el-form-item>
        <el-form-item label="产线">
          <el-select v-model="form.lineId" placeholder="请选择产线" clearable style="width: 100%">
            <el-option v-for="l in lines" :key="l.id" :label="l.name" :value="l.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="负责人">
          <el-input v-model="form.assignee" />
        </el-form-item>
        <el-form-item label="备注">
          <el-input v-model="form.remark" type="textarea" :rows="3" />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleCreate" :loading="submitting">创建</el-button>
          <el-button @click="goBack">取消</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <template v-else>
      <!-- 基本信息 -->
      <el-card class="detail-card">
        <template #header>
          <div class="card-header">
            <span>工单详情 #{{ detail?.orderNo }}</span>
            <div>
              <el-button @click="goBack">返回</el-button>
              <template v-if="detail?.status === 'PENDING'">
                <el-button type="success" @click="handleRelease">下达</el-button>
                <el-button type="warning" @click="handleCancel">取消</el-button>
              </template>
              <el-button v-else-if="detail?.status === 'RELEASED'" @click="handleHold">暂停</el-button>
              <el-button v-else-if="detail?.status === 'IN_PROGRESS'" @click="handleHold">暂停</el-button>
              <el-button v-if="detail?.status === 'IN_PROGRESS'" type="success" @click="workReportDialogVisible = true">提交报工</el-button>
              <el-button v-if="detail?.status === 'IN_PROGRESS' || detail?.status === 'COMPLETED'" type="warning" @click="scrapDialogVisible = true">报废</el-button>
              <el-button v-if="detail?.status === 'IN_PROGRESS' || detail?.status === 'COMPLETED'" type="danger" @click="reworkDialogVisible = true">返工</el-button>
              <el-button v-else-if="detail?.status === 'ON_HOLD'" type="success" @click="handleResume">恢复</el-button>
              <el-button v-else-if="detail?.status === 'COMPLETED'" @click="handleClose">关闭</el-button>
            </div>
          </div>
        </template>
        <el-descriptions :column="3" border>
          <el-descriptions-item label="工单号">{{ detail?.orderNo }}</el-descriptions-item>
          <el-descriptions-item label="物料">{{ detail?.materialName || `#${detail?.materialId}` }}</el-descriptions-item>
          <el-descriptions-item label="状态">
            <el-tag :type="statusTagType(detail?.status)">{{ statusLabel(detail?.status) }}</el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="计划数量">{{ detail?.plannedQty }}</el-descriptions-item>
          <el-descriptions-item label="已完成">{{ detail?.completedQty ?? 0 }}</el-descriptions-item>
          <el-descriptions-item label="报废">{{ detail?.scrapQty ?? 0 }}</el-descriptions-item>
          <el-descriptions-item label="优先级">{{ detail?.priority ?? '-' }}</el-descriptions-item>
          <el-descriptions-item label="计划开始">{{ detail?.planStartTime }}</el-descriptions-item>
          <el-descriptions-item label="计划结束">{{ detail?.planEndTime }}</el-descriptions-item>
          <el-descriptions-item label="来源" v-if="detail?.sourceType">{{ detail?.sourceType }} / {{ detail?.sourceRef }}</el-descriptions-item>
          <el-descriptions-item label="负责人">{{ detail?.assignee || '-' }}</el-descriptions-item>
          <el-descriptions-item label="备注">{{ detail?.remark || '-' }}</el-descriptions-item>
        </el-descriptions>
      </el-card>

      <!-- 工序进度 -->
      <el-card class="detail-card">
        <template #header>
          <span>工序进度</span>
        </template>
        <el-table :data="detail?.steps || []" border stripe>
          <el-table-column prop="stepNo" label="工序号" width="90" />
          <el-table-column prop="stepName" label="工序名称" min-width="160" />
          <el-table-column prop="plannedQty" label="计划数" width="90" align="right" />
          <el-table-column prop="completedQty" label="已完成" width="90" align="right" />
          <el-table-column prop="scrapQty" label="报废" width="80" align="right" />
          <el-table-column label="状态" width="100">
            <template #default="{ row }">
              <el-tag :type="statusTagType(row.status)" size="small">{{ statusLabel(row.status) }}</el-tag>
            </template>
          </el-table-column>
        </el-table>
      </el-card>

      <!-- 报工对话框 -->
      <el-dialog v-model="workReportDialogVisible" title="提交报工" width="500px" :close-on-click-modal="false">
        <el-form :model="workReportForm" label-width="100px">
          <el-form-item label="工序选择">
            <el-select v-model="workReportForm.stepId" placeholder="请选择工序" clearable style="width:100%">
              <el-option v-for="s in (detail?.steps || [])" :key="s.id" :label="s.stepName" :value="s.id" />
            </el-select>
          </el-form-item>
          <el-form-item label="良品数量">
            <el-input-number v-model="workReportForm.goodQty" :min="0" style="width:100%" />
          </el-form-item>
          <el-form-item label="报废数量">
            <el-input-number v-model="workReportForm.scrapQty" :min="0" style="width:100%" />
          </el-form-item>
          <el-form-item label="返工数量">
            <el-input-number v-model="workReportForm.reworkQty" :min="0" style="width:100%" />
          </el-form-item>
          <el-form-item label="备注">
            <el-input v-model="workReportForm.remark" type="textarea" :rows="2" />
          </el-form-item>
        </el-form>
        <template #footer>
          <el-button @click="workReportDialogVisible = false">取消</el-button>
          <el-button type="primary" @click="handleSubmitWorkReport" :loading="workReportSubmitting">确认提交</el-button>
        </template>
      </el-dialog>

      <!-- 报废对话框 -->
      <el-dialog v-model="scrapDialogVisible" title="报废" width="400px" :close-on-click-modal="false">
        <el-form :model="scrapForm" label-width="100px">
          <el-form-item label="报废数量">
            <el-input-number v-model="scrapForm.scrapQty" :min="1" style="width:100%" />
          </el-form-item>
          <el-form-item label="备注">
            <el-input v-model="scrapForm.remark" type="textarea" :rows="2" />
          </el-form-item>
        </el-form>
        <template #footer>
          <el-button @click="scrapDialogVisible = false">取消</el-button>
          <el-button type="warning" @click="handleScrap" :loading="scrapSubmitting">确认报废</el-button>
        </template>
      </el-dialog>

      <!-- 返工对话框 -->
      <el-dialog v-model="reworkDialogVisible" title="返工" width="400px" :close-on-click-modal="false">
        <el-form :model="reworkForm" label-width="100px">
          <el-form-item label="返工数量">
            <el-input-number v-model="reworkForm.reworkQty" :min="1" style="width:100%" />
          </el-form-item>
          <el-form-item label="备注">
            <el-input v-model="reworkForm.remark" type="textarea" :rows="2" />
          </el-form-item>
        </el-form>
        <template #footer>
          <el-button @click="reworkDialogVisible = false">取消</el-button>
          <el-button type="danger" @click="handleRework" :loading="reworkSubmitting">确认返工</el-button>
        </template>
      </el-dialog>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { workOrderApi } from '../../api/work-order'
import { workReportApi } from '../../api/work-report'
import type { WorkOrder } from '../../api/work-order'
import { materialApi } from '../../api/material'
import { routingApi } from '../../api/routing'
import { productionLineApi } from '../../api/production-line'

const route = useRoute()
const router = useRouter()
const detail = ref<WorkOrder | null>(null)
const loading = ref(false)
const submitting = ref(false)
const materials = ref<any[]>([])
const routings = ref<any[]>([])
const lines = ref<any[]>([])
const formRef = ref()

const isCreate = computed(() => {
  const id = route.params.id as string
  return id === 'new' || id === '0'
})

const form = ref<WorkOrder>({
  orderNo: '',
  materialId: 0,
  plannedQty: 1,
  priority: 50,
  planStartTime: '',
  planEndTime: '',
  lineId: undefined,
  assignee: '',
  remark: ''
})

// 报工件���
const workReportDialogVisible = ref(false)
const workReportSubmitting = ref(false)
const workReportForm = ref({
  goodQty: 0,
  scrapQty: 0,
  reworkQty: 0,
  stepId: undefined as number | undefined,
  remark: ''
})

// 报废对话框
const scrapDialogVisible = ref(false)
const scrapSubmitting = ref(false)
const scrapForm = ref({
  scrapQty: 0,
  remark: ''
})

// 返工对话框
const reworkDialogVisible = ref(false)
const reworkSubmitting = ref(false)
const reworkForm = ref({
  reworkQty: 0,
  remark: ''
})

async function handleSubmitWorkReport() {
  if (!detail.value?.id) return
  if (workReportForm.value.goodQty <= 0 && workReportForm.value.scrapQty <= 0 && workReportForm.value.reworkQty <= 0) {
    ElMessage.warning('请至少填写良品、报废或返工数量之一')
    return
  }
  workReportSubmitting.value = true
  try {
    await workReportApi.create({
      workOrderId: detail.value.id,
      stepId: workReportForm.value.stepId,
      reportType: 'NORMAL',
      goodQty: workReportForm.value.goodQty,
      scrapQty: workReportForm.value.scrapQty,
      reworkQty: workReportForm.value.reworkQty,
      remark: workReportForm.value.remark,
      reportTime: new Date().toISOString()
    })
    ElMessage.success('报工提交成功')
    workReportDialogVisible.value = false
    workReportForm.value = { goodQty: 0, scrapQty: 0, reworkQty: 0, stepId: undefined, remark: '' }
    await loadDetail(detail.value.id)
  } catch (e: any) {
    ElMessage.error(e.message || '报工失败')
  } finally {
    workReportSubmitting.value = false
  }
}

async function handleScrap() {
  if (!detail.value?.id) return
  if (scrapForm.value.scrapQty <= 0) {
    ElMessage.warning('请输入报废数量')
    return
  }
  scrapSubmitting.value = true
  try {
    await workOrderApi.scrap(detail.value.id, scrapForm.value)
    ElMessage.success('报废提交成功')
    scrapDialogVisible.value = false
    scrapForm.value = { scrapQty: 0, remark: '' }
    await loadDetail(detail.value.id)
  } catch (e: any) {
    ElMessage.error(e.message || '报废失败')
  } finally {
    scrapSubmitting.value = false
  }
}

async function handleRework() {
  if (!detail.value?.id) return
  if (reworkForm.value.reworkQty <= 0) {
    ElMessage.warning('请输入返工数量')
    return
  }
  reworkSubmitting.value = true
  try {
    await workOrderApi.rework(detail.value.id, reworkForm.value)
    ElMessage.success('返工提交成功')
    reworkDialogVisible.value = false
    reworkForm.value = { reworkQty: 0, remark: '' }
    await loadDetail(detail.value.id)
  } catch (e: any) {
    ElMessage.error(e.message || '返工失败')
  } finally {
    reworkSubmitting.value = false
  }
}

const formRules = {
  materialId: [{ required: true, message: '请选择物料', trigger: 'change' }],
  plannedQty: [{ required: true, message: '请输入计划数量', trigger: 'blur' }]
}

function statusTagType(status?: string): string {
  const map: Record<string, string> = {
    PENDING: 'info', RELEASED: 'primary', IN_PROGRESS: 'warning',
    COMPLETED: 'success', CLOSED: '', CANCELLED: 'danger', ON_HOLD: 'danger'
  }
  return map[status || ''] || 'info'
}

function statusLabel(status?: string): string {
  const map: Record<string, string> = {
    PENDING: '待下达', RELEASED: '已下达', IN_PROGRESS: '生产中',
    COMPLETED: '已完成', CLOSED: '已关闭', CANCELLED: '已取消', ON_HOLD: '已暂停'
  }
  return map[status || ''] || status || ''
}

function goBack() {
  router.push('/work-order')
}

async function loadDetail(id: number) {
  loading.value = true
  try {
    const res: any = await workOrderApi.getById(id)
    detail.value = res.data
  } catch (e) {
    console.error(e)
    ElMessage.error('加载工单详情失败')
  } finally {
    loading.value = false
  }
}

async function loadMaterials() {
  try {
    const res: any = await materialApi.list()
    materials.value = res.data || []
  } catch {}
}

function materialSearch(query: string) {
  // Client-side filter
}

async function onMaterialChange(materialId: number) {
  if (!materialId) { routings.value = []; return }
  try {
    const res: any = await routingApi.listByMaterial(materialId)
    routings.value = res.data || []
  } catch {
    routings.value = []
  }
}

async function loadLines() {
  try {
    const res: any = await productionLineApi.list()
    lines.value = res.data || []
  } catch {}
}

async function handleCreate() {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return
  submitting.value = true
  try {
    if (!form.value.orderNo) {
      form.value.orderNo = 'WO-' + Date.now()
    }
    const res: any = await workOrderApi.create(form.value)
    ElMessage.success('工单创建成功')
    router.push(`/work-order/${res.data?.id || ''}`)
  } catch (e: any) {
    ElMessage.error(e.message || '创建失败')
  } finally {
    submitting.value = false
  }
}

async function handleRelease() {
  try {
    await ElMessageBox.confirm(`确认下达工单「${detail.value?.orderNo}」？`, '提示')
    await workOrderApi.release(detail.value!.id!)
    ElMessage.success('已下达')
    await loadDetail(detail.value!.id!)
  } catch {}
}

async function handleHold() {
  try {
    await ElMessageBox.confirm(`确认暂停工单「${detail.value?.orderNo}」？`, '提示')
    await workOrderApi.hold(detail.value!.id!)
    ElMessage.success('已暂停')
    await loadDetail(detail.value!.id!)
  } catch {}
}

async function handleResume() {
  try {
    await ElMessageBox.confirm(`确认恢复工单「${detail.value?.orderNo}」？`, '提示')
    await workOrderApi.resume(detail.value!.id!)
    ElMessage.success('已恢复')
    await loadDetail(detail.value!.id!)
  } catch {}
}

async function handleCancel() {
  try {
    await ElMessageBox.confirm(`确认取消工单「${detail.value?.orderNo}」？`, '警告', { type: 'warning' })
    await workOrderApi.cancel(detail.value!.id!)
    ElMessage.success('已取消')
    await loadDetail(detail.value!.id!)
  } catch {}
}

async function handleClose() {
  try {
    await ElMessageBox.confirm(`确���关闭工单「${detail.value?.orderNo}」？`, '提示')
    await workOrderApi.close(detail.value!.id!)
    ElMessage.success('已关闭')
    await loadDetail(detail.value!.id!)
  } catch {}
}

onMounted(async () => {
  if (!isCreate.value) {
    const id = parseInt(route.params.id as string)
    if (id) await loadDetail(id)
  }
  await loadMaterials()
  await loadLines()
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
.detail-card {
  margin-bottom: 16px;
}
</style>
