<template>
  <div class="page-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span>种子数据初始化</span>
        </div>
      </template>

      <div style="text-align:center; padding:40px">
        <el-icon :size="64" color="#409eff"><DataBoard /></el-icon>
        <h3 style="margin:16px 0 8px">种子数据管理</h3>
        <p style="color:#999; margin-bottom:24px">
          初始化系统种子数据，包括工厂、车间、产线、工位、物料、BOM、工艺路线、工单等基础数据。
        </p>
        <el-button
          type="primary"
          size="large"
          @click="handleInit"
          :loading="loading"
          :disabled="initialized"
        >
          {{ initialized ? '已初始化' : '初始化种子数据' }}
        </el-button>
      </div>

      <!-- 初始化结果 -->
      <el-card v-if="result" shadow="never" style="margin-top:16px">
        <template #header>
          <span style="color:#67c23a">初始化完成</span>
        </template>
        <el-descriptions :column="3" border>
          <el-descriptions-item label="工厂数">{{ result.factoryCount ?? result.factories ?? 0 }}</el-descriptions-item>
          <el-descriptions-item label="车间数">{{ result.workshopCount ?? result.workshops ?? 0 }}</el-descriptions-item>
          <el-descriptions-item label="产线数">{{ result.lineCount ?? result.lines ?? result.productionLines ?? 0 }}</el-descriptions-item>
          <el-descriptions-item label="工位数">{{ result.workstationCount ?? result.workstations ?? 0 }}</el-descriptions-item>
          <el-descriptions-item label="物料数">{{ result.materialCount ?? result.materials ?? 0 }}</el-descriptions-item>
          <el-descriptions-item label="BOM数">{{ result.bomCount ?? result.boms ?? 0 }}</el-descriptions-item>
          <el-descriptions-item label="工艺路线数">{{ result.routingCount ?? result.routings ?? 0 }}</el-descriptions-item>
          <el-descriptions-item label="工单数">{{ result.workOrderCount ?? result.workOrders ?? 0 }}</el-descriptions-item>
          <el-descriptions-item label="用户数">{{ result.userCount ?? result.users ?? 0 }}</el-descriptions-item>
        </el-descriptions>
      </el-card>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { DataBoard } from '@element-plus/icons-vue'
import { seedApi } from '../../api/seed'

const loading = ref(false)
const initialized = ref(false)
const result = ref<any>(null)

async function handleInit() {
  try {
    await ElMessageBox.confirm(
      '确认初始化种子数据？初始化将创建工厂、车间、产线、工位、物料、BOM、工艺路线、工单、用户等基础数据。',
      '确认初始化',
      { type: 'warning', confirmButtonText: '确认初始化', cancelButtonText: '取消' }
    )
  } catch {
    return
  }

  loading.value = true
  try {
    const res: any = await seedApi.initSeed()
    result.value = res.data || res
    initialized.value = true
    ElMessage.success('种子数据初始化完成')
  } catch (e: any) {
    ElMessage.error(e.message || '初始化失败')
  } finally {
    loading.value = false
  }
}
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
