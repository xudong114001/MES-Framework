<template>
  <div class="dashboard-container">
    <!-- 统计卡片 -->
    <el-row :gutter="16" style="margin-bottom: 16px">
      <el-col :span="6">
        <el-card shadow="hover" class="stat-card">
          <div class="stat-value primary">{{ stats.total }}</div>
          <div class="stat-label">今日质检总数</div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover" class="stat-card">
          <div class="stat-value success">{{ stats.passed }}</div>
          <div class="stat-label">合格数</div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover" class="stat-card">
          <div class="stat-value danger">{{ stats.failed }}</div>
          <div class="stat-label">不合格数</div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover" class="stat-card">
          <div class="stat-value warning">{{ stats.pending }}</div>
          <div class="stat-label">待检数</div>
        </el-card>
      </el-col>
    </el-row>

    <el-row :gutter="16">
      <!-- 待检列表 -->
      <el-col :span="12">
        <el-card>
          <template #header>
            <div class="card-header">
              <span>待检列表</span>
              <el-button size="small" type="primary" @click="$router.push('/qc')">查看全部</el-button>
            </div>
          </template>
          <el-table :data="pendingList" border stripe v-loading="loading" max-height="400" size="small">
            <el-table-column prop="inspectNo" label="质检单号" min-width="130" />
            <el-table-column label="来源" width="80">
              <template #default="{ row }">
                {{ sourceTypeLabel(row.sourceType) }}
              </template>
            </el-table-column>
            <el-table-column prop="inspectTime" label="创建时间" width="150" />
            <el-table-column label="操作" width="80" fixed="right">
              <template #default="{ row }">
                <el-button size="small" @click="goToQc(row.id)">处理</el-button>
              </template>
            </el-table-column>
          </el-table>
        </el-card>
      </el-col>

      <!-- 近期不合格品 -->
      <el-col :span="12">
        <el-card>
          <template #header>
            <div class="card-header">
              <span>近期不合格品</span>
            </div>
          </template>
          <el-table :data="failedList" border stripe v-loading="loadingFailed" max-height="400" size="small">
            <el-table-column prop="inspectNo" label="质检单号" min-width="130" />
            <el-table-column label="来源" width="80">
              <template #default="{ row }">
                {{ sourceTypeLabel(row.sourceType) }}
              </template>
            </el-table-column>
            <el-table-column prop="handlingAction" label="处理动作" width="100">
              <template #default="{ row }">
                <el-tag v-if="row.handlingAction === 'CONCESSION'" type="success" size="small">让步接收</el-tag>
                <el-tag v-else-if="row.handlingAction === 'REWORK'" type="warning" size="small">返工</el-tag>
                <el-tag v-else-if="row.handlingAction === 'SCRAP'" type="danger" size="small">报废</el-tag>
                <span v-else class="text-muted">未处理</span>
              </template>
            </el-table-column>
            <el-table-column label="操作" width="80" fixed="right">
              <template #default="{ row }">
                <el-button v-if="!row.handlingAction" size="small" @click="showHandleDialog(row)">处理</el-button>
                <span v-else class="text-muted">已处理</span>
              </template>
            </el-table-column>
          </el-table>
        </el-card>
      </el-col>
    </el-row>

    <!-- 不合格品处理弹窗 -->
    <el-dialog v-model="handleDialogVisible" title="不合格品处理" width="450px">
      <el-form ref="handleFormRef" :model="handleForm" :rules="handleRules" label-width="100px">
        <el-form-item label="质检单号">
          <el-input :model-value="currentHandleRow?.inspectNo" disabled />
        </el-form-item>
        <el-form-item label="处理动作" prop="action">
          <el-select v-model="handleForm.action" style="width: 100%">
            <el-option label="让步接收" value="CONCESSION" />
            <el-option label="返工" value="REWORK" />
            <el-option label="报废" value="SCRAP" />
          </el-select>
        </el-form-item>
        <el-form-item label="处理备注" prop="remark">
          <el-input v-model="handleForm.remark" type="textarea" :rows="3" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="handleDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="submitHandle" :loading="handleSubmitting">确认处理</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { qcApi, type QcInspection, type QcDashboardStats } from '../../api/qc'
import { useRouter } from 'vue-router'

const router = useRouter()

const stats = ref<QcDashboardStats>({ total: 0, passed: 0, failed: 0, pending: 0 })
const pendingList = ref<QcInspection[]>([])
const failedList = ref<QcInspection[]>([])
const loading = ref(false)
const loadingFailed = ref(false)

// 不合格品处理
const handleDialogVisible = ref(false)
const handleSubmitting = ref(false)
const handleFormRef = ref()
const currentHandleRow = ref<QcInspection | null>(null)
const handleForm = ref({ action: '', remark: '' })
const handleRules = {
  action: [{ required: true, message: '请选择处理动作', trigger: 'change' }]
}

function sourceTypeLabel(type?: string) {
  const map: Record<string, string> = { INCOMING: '来料检', FIRST_ARTICLE: '首件检', IN_PROCESS: '过程检', FINAL: '完工检' }
  return map[type || ''] || type || '-'
}

async function loadStats() {
  try {
    const res: any = await qcApi.dashboardStats()
    stats.value = res.data || { total: 0, passed: 0, failed: 0, pending: 0 }
  } catch { /* ignore */ }
}

async function loadPending() {
  loading.value = true
  try {
    const res: any = await qcApi.dashboardPending()
    pendingList.value = res.data || []
  } finally {
    loading.value = false
  }
}

async function loadFailed() {
  loadingFailed.value = true
  try {
    const res: any = await qcApi.recentFailed()
    failedList.value = res.data || []
  } finally {
    loadingFailed.value = false
  }
}

function goToQc(id?: number) {
  router.push('/qc')
}

function showHandleDialog(row: QcInspection) {
  currentHandleRow.value = row
  handleForm.value = { action: '', remark: '' }
  handleDialogVisible.value = true
}

async function submitHandle() {
  const valid = await handleFormRef.value.validate().catch(() => false)
  if (!valid || !currentHandleRow.value?.id) return
  handleSubmitting.value = true
  try {
    await qcApi.handleNonconforming(currentHandleRow.value.id, handleForm.value.action, handleForm.value.remark)
    ElMessage.success('处理成功')
    handleDialogVisible.value = false
    await loadFailed()
  } finally {
    handleSubmitting.value = false
  }
}

onMounted(() => {
  loadStats()
  loadPending()
  loadFailed()
})
</script>

<style scoped>
.dashboard-container { padding: 16px; }
.stat-card { text-align: center; padding: 8px 0; }
.stat-value { font-size: 36px; font-weight: bold; line-height: 1.2; }
.stat-label { font-size: 14px; color: #909399; margin-top: 8px; }
.primary { color: #409eff; }
.success { color: #67c23a; }
.danger { color: #f56c6c; }
.warning { color: #e6a23c; }
.card-header { display: flex; justify-content: space-between; align-items: center; }
.text-muted { color: #c0c4cc; font-size: 12px; }
</style>
