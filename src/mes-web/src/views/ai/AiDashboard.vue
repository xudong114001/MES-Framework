<template>
  <div class="ai-dashboard">
    <el-row :gutter="20">
      <el-col :span="24">
        <h2>AI 智能分析中心</h2>
      </el-col>
    </el-row>

    <el-row :gutter="20" class="stat-cards">
      <el-col :span="8">
        <el-card shadow="hover">
          <template #header>
            <div class="card-header">
              <span>质量预警</span>
              <el-tag type="danger">{{ qualityAlerts.length }}</el-tag>
            </div>
          </template>
          <div class="stat-value">{{ qualityAlerts.length }}</div>
          <div class="stat-label">活跃预警数</div>
          <el-button type="primary" size="small" @click="refreshQuality">刷新</el-button>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card shadow="hover">
          <template #header>
            <div class="card-header">
              <span>排程建议</span>
              <el-tag type="success">{{ schedulingCount }}</el-tag>
            </div>
          </template>
          <div class="stat-value">{{ schedulingCount }}</div>
          <div class="stat-label">待排产工单</div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card shadow="hover">
          <template #header>
            <div class="card-header">
              <span>设备健康</span>
              <el-tag :type="highRiskCount > 0 ? 'danger' : 'success'">{{ highRiskCount }}</el-tag>
            </div>
          </template>
          <div class="stat-value">{{ highRiskCount }}</div>
          <div class="stat-label">高风险设备</div>
          <el-button type="primary" size="small" @click="refreshEquipment">刷新</el-button>
        </el-card>
      </el-col>
    </el-row>

    <el-row :gutter="20">
      <el-col :span="12">
        <el-card>
          <template #header>
            <span>质量预警</span>
          </template>
          <el-table :data="qualityAlerts" max-height="300">
            <el-table-column prop="title" label="预警标题" />
            <el-table-column prop="level" label="级别" width="80">
              <template #default="{ row }">
                <el-tag :type="getLevelType(row.level)">{{ row.level }}</el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="message" label="消息" show-overflow-tooltip />
            <el-table-column label="操作" width="100">
              <template #default="{ row }">
                <el-button size="small" @click="processAlert(row)">处理</el-button>
              </template>
            </el-table-column>
          </el-table>
        </el-card>
      </el-col>
      <el-col :span="12">
        <el-card>
          <template #header>
            <span>设备健康状态</span>
          </template>
          <el-table :data="equipmentHealth" max-height="300">
            <el-table-column prop="equipmentName" label="设备名称" />
            <el-table-column prop="healthScore" label="健康度" width="80" />
            <el-table-column prop="riskLevel" label="风险" width="80">
              <template #default="{ row }">
                <el-tag :type="getRiskType(row.riskLevel)">{{ row.riskLevel }}</el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="recommendation" label="建议" show-overflow-tooltip />
          </el-table>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { aiApi } from '../../api/ai'

const qualityAlerts = ref<any[]>([])
const equipmentHealth = ref<any[]>([])
const schedulingCount = ref(0)
const highRiskCount = ref(0)

const getLevelType = (level: string) => {
  const map: Record<string, string> = {
    'Critical': 'danger',
    'High': 'warning',
    'Medium': 'info',
    'Low': 'success'
  }
  return map[level] || 'info'
}

const getRiskType = (risk: string) => {
  const map: Record<string, string> = {
    '优': 'success',
    '良': 'info',
    '预警': 'warning',
    '危险': 'danger'
  }
  return map[risk] || 'info'
}

const refreshQuality = async () => {
  try {
    const res = await aiApi.getActiveAlerts()
    qualityAlerts.value = res.data || []
  } catch (e) {
    ElMessage.error('获取质量预警失败')
  }
}

const refreshEquipment = async () => {
  try {
    const res = await aiApi.getAllEquipmentHealth()
    equipmentHealth.value = res.data || []
    highRiskCount.value = equipmentHealth.value.filter((e: any) => e.healthScore < 70).length
  } catch (e) {
    ElMessage.error('获取设备健康失败')
  }
}

const processAlert = async (alert: any) => {
  try {
    await aiApi.processAlert(alert.id, 'operator')
    ElMessage.success('处理成功')
    refreshQuality()
  } catch (e) {
    ElMessage.error('处理失败')
  }
}

onMounted(() => {
  refreshQuality()
  refreshEquipment()
})
</script>

<style scoped>
.ai-dashboard {
  padding: 20px;
}
.stat-cards {
  margin-bottom: 20px;
}
.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.stat-value {
  font-size: 32px;
  font-weight: bold;
  text-align: center;
  margin: 10px 0;
}
.stat-label {
  text-align: center;
  color: #666;
}
</style>