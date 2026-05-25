<template>
  <div class="gantt-view">
    <el-card>
      <template #header>
        <div class="card-header">
          <span>甘特图排产</span>
          <el-select v-model="selectedLineId" placeholder="选择产线" @change="fetchData" style="width: 200px">
            <el-option v-for="l in lines" :key="l.id" :label="l.name" :value="l.id" />
          </el-select>
        </div>
      </template>

      <div class="gantt-container" v-loading="loading">
        <!-- 表头：日期列 -->
        <div class="gantt-header">
          <div class="gantt-header-left">工单信息</div>
          <div class="gantt-header-right">
            <div
              v-for="day in dateHeaders"
              :key="day.dateStr"
              class="gantt-header-cell"
              :class="{ weekend: day.isWeekend }"
            >
              <div class="date-weekday">{{ day.weekday }}</div>
              <div class="date-day">{{ day.dateStr }}</div>
            </div>
          </div>
        </div>

        <!-- 甘特行 -->
        <div class="gantt-body" v-if="orders.length > 0">
          <div
            v-for="order in orders"
            :key="order.id"
            class="gantt-row"
          >
            <div class="gantt-row-left">
              <div class="order-info">
                <div class="order-no">{{ order.orderNo }}</div>
                <div class="order-meta">
                  <span>{{ order.materialName }}</span>
                  <span class="order-qty">x {{ order.plannedQty }}</span>
                </div>
              </div>
            </div>
            <div class="gantt-row-right">
              <div
                v-for="cell in order.dateCells"
                :key="cell.dateStr"
                class="gantt-cell"
                :class="{
                  'in-range': cell.inRange,
                  'weekend': cell.isWeekend
                }"
                :style="cell.inRange ? { backgroundColor: cell.color } : {}"
                :title="cell.inRange ? order.orderNo + '\n' + order.materialName : ''"
              >
                <span v-if="cell.inRange && cell.isStart" class="gantt-range-label">
                  {{ order.orderNo }}
                </span>
              </div>
            </div>
          </div>
        </div>

        <el-empty v-if="!loading && orders.length === 0" description="暂无已排产工单" />
      </div>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { schedulingApi } from '../../api/scheduling'
import { productionLineApi } from '../../api/production-line'

const loading = ref(false)
const lines = ref<any[]>([])
const selectedLineId = ref<number | null>(null)
const orders = ref<any[]>([])

// 产线颜色表
const lineColors: Record<string, string> = {
  '0': '#409eff',
  '1': '#67c23a',
  '2': '#e6a23c',
  '3': '#f56c6c',
  '4': '#909399',
  '5': '#b37feb',
  '6': '#36cfc9',
}

// 生成今天起7天的日期头
const dateHeaders = ref<Array<{ dateStr: string; weekday: string; isWeekend: boolean }>>([])

function generateDateHeaders() {
  const weekdays = ['日', '一', '二', '三', '四', '五', '六']
  const today = new Date()
  dateHeaders.value = []
  for (let i = 0; i < 7; i++) {
    const d = new Date(today)
    d.setDate(d.getDate() + i)
    const month = String(d.getMonth() + 1).padStart(2, '0')
    const day = String(d.getDate()).padStart(2, '0')
    dateHeaders.value.push({
      dateStr: `${month}/${day}`,
      weekday: weekdays[d.getDay()],
      isWeekend: d.getDay() === 0 || d.getDay() === 6,
    })
  }
}

function formatDateStr(year: number, month: number, day: number): string {
  return `${String(month).padStart(2, '0')}/${String(day).padStart(2, '0')}`
}

function isWeekend(year: number, month: number, day: number): boolean {
  const d = new Date(year, month - 1, day)
  return d.getDay() === 0 || d.getDay() === 6
}

function getLineColor(lineId: number | null): string {
  if (lineId === null) return '#409eff'
  const key = String(lineId)
  return lineColors[key] || lineColors[String(lineId % 7)]
}

async function fetchLines() {
  const res = await productionLineApi.list()
  lines.value = res.data || []
  if (lines.value.length > 0) {
    selectedLineId.value = lines.value[0].id
  }
}

async function fetchData() {
  if (!selectedLineId.value) return
  loading.value = true
  try {
    const res = await schedulingApi.lineScheduledOrders(selectedLineId.value)
    const rawOrders = res.data || []
    const today = new Date()
    const todayStart = new Date(today.getFullYear(), today.getMonth(), today.getDate())

    orders.value = rawOrders.map((order: any) => {
      let startDate = todayStart
      let endDate = todayStart

      if (order.planStartTime) {
        const ps = new Date(order.planStartTime)
        startDate = new Date(ps.getFullYear(), ps.getMonth(), ps.getDate())
      }
      if (order.planEndTime) {
        const pe = new Date(order.planEndTime)
        endDate = new Date(pe.getFullYear(), pe.getMonth(), pe.getDate())
      }

      const color = getLineColor(order.lineId)

      // 今天起7天，判断每天是否在排产范围内
      const dateCells = dateHeaders.value.map((header, idx) => {
        const d = new Date(today)
        d.setDate(d.getDate() + idx)
        const cellDate = new Date(d.getFullYear(), d.getMonth(), d.getDate())

        // 判断 cellDate 是否在 [startDate, endDate] 范围内
        const inRange = cellDate >= startDate && cellDate <= endDate
        const isStart = cellDate.getTime() === startDate.getTime()

        return {
          dateStr: header.dateStr,
          inRange,
          isStart,
          isWeekend: header.isWeekend,
          color,
        }
      })

      return {
        ...order,
        dateCells,
      }
    })
  } finally {
    loading.value = false
  }
}

onMounted(async () => {
  generateDateHeaders()
  await fetchLines()
  await fetchData()
})
</script>

<style scoped>
.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.gantt-container {
  overflow-x: auto;
}

.gantt-header {
  display: flex;
  border-bottom: 2px solid #dcdfe6;
  min-width: 800px;
}

.gantt-header-left {
  width: 220px;
  flex-shrink: 0;
  padding: 10px 12px;
  font-weight: bold;
  background: #f5f7fa;
  border-right: 1px solid #e4e7ed;
}

.gantt-header-right {
  display: flex;
  flex: 1;
}

.gantt-header-cell {
  flex: 1;
  text-align: center;
  padding: 6px 0;
  background: #f5f7fa;
  border-right: 1px solid #e4e7ed;
  font-size: 13px;
}

.gantt-header-cell.weekend {
  background: #fef0f0;
}

.date-weekday {
  font-weight: bold;
  color: #606266;
}

.date-day {
  font-size: 12px;
  color: #909399;
  margin-top: 2px;
}

.gantt-body {
  min-width: 800px;
}

.gantt-row {
  display: flex;
  border-bottom: 1px solid #ebeef5;
  transition: background-color 0.2s;
}

.gantt-row:hover {
  background-color: #f5f7fa;
}

.gantt-row-left {
  width: 220px;
  flex-shrink: 0;
  padding: 10px 12px;
  border-right: 1px solid #e4e7ed;
  display: flex;
  align-items: center;
}

.order-info {
  overflow: hidden;
}

.order-no {
  font-weight: 600;
  font-size: 13px;
  color: #303133;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.order-meta {
  font-size: 12px;
  color: #909399;
  margin-top: 2px;
  display: flex;
  gap: 6px;
}

.order-qty {
  color: #409eff;
}

.gantt-row-right {
  display: flex;
  flex: 1;
  position: relative;
}

.gantt-cell {
  flex: 1;
  min-height: 48px;
  border-right: 1px solid #f0f0f0;
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
}

.gantt-cell.weekend {
  background-color: #fafafa;
}

.gantt-cell.in-range {
  opacity: 0.85;
}

.gantt-range-label {
  font-size: 11px;
  color: #fff;
  font-weight: 600;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 100%;
  padding: 0 4px;
  text-shadow: 0 1px 2px rgba(0,0,0,0.3);
}
</style>
