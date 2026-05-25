<template>
  <div class="page-container">
    <el-card>
      <template #header>
        <span>追溯查询</span>
      </template>

      <el-tabs v-model="activeTab">
        <el-tab-pane label="按批次追溯" name="batch">
          <el-form :model="searchForm" inline style="margin-bottom: 16px">
            <el-form-item label="批次号">
              <el-input v-model="searchForm.batchNo" placeholder="请输入批次号" style="width: 280px" />
            </el-form-item>
            <el-form-item>
              <el-button type="primary" @click="searchByBatch" :loading="loading">查询</el-button>
            </el-form-item>
          </el-form>
          <el-table v-if="batchResult.length" :data="batchResult" border stripe v-loading="loading">
            <el-table-column type="index" label="#" width="50" />
            <el-table-column prop="materialName" label="物料名称" min-width="140" />
            <el-table-column prop="batchNo" label="批次号" min-width="140" />
            <el-table-column prop="workOrderNo" label="工单号" min-width="120" />
            <el-table-column prop="qty" label="数量" width="80" align="right" />
            <el-table-column prop="productionDate" label="生产日期" width="120" />
            <el-table-column prop="status" label="状态" width="80" />
          </el-table>
          <el-empty v-else-if="searched && !batchResult.length" description="未找到相关记录" />
        </el-tab-pane>

        <el-tab-pane label="按序列号追溯" name="serial">
          <el-form :model="searchForm" inline style="margin-bottom: 16px">
            <el-form-item label="序列号">
              <el-input v-model="searchForm.serialNo" placeholder="请输入序列号" style="width: 280px" />
            </el-form-item>
            <el-form-item>
              <el-button type="primary" @click="searchBySerial" :loading="loading">查询</el-button>
            </el-form-item>
          </el-form>
          <el-table v-if="serialResult.length" :data="serialResult" border stripe v-loading="loading">
            <el-table-column type="index" label="#" width="50" />
            <el-table-column prop="serialNo" label="序列号" min-width="150" />
            <el-table-column prop="materialName" label="物料名称" min-width="140" />
            <el-table-column prop="batchNo" label="批次号" min-width="120" />
            <el-table-column prop="workOrderNo" label="工单号" min-width="120" />
            <el-table-column prop="productionDate" label="生产日期" width="120" />
            <el-table-column prop="status" label="状态" width="80" />
          </el-table>
          <el-empty v-else-if="searched && !serialResult.length" description="未找到相关记录" />
        </el-tab-pane>

        <el-tab-pane label="正向追溯" name="forward">
          <el-form :model="forwardForm" inline style="margin-bottom: 16px">
            <el-form-item label="物料ID">
              <el-input-number v-model="forwardForm.materialId" :min="1" style="width: 160px" />
            </el-form-item>
            <el-form-item label="批次号">
              <el-input v-model="forwardForm.batchNo" placeholder="请输入批次号" style="width: 200px" />
            </el-form-item>
            <el-form-item>
              <el-button type="primary" @click="searchForward" :loading="loading">查询</el-button>
            </el-form-item>
          </el-form>
          <el-table v-if="forwardResult.length" :data="forwardResult" border stripe v-loading="loading">
            <el-table-column type="index" label="#" width="50" />
            <el-table-column prop="materialName" label="物料名称" min-width="140" />
            <el-table-column prop="batchNo" label="批次号" min-width="140" />
            <el-table-column prop="workOrderNo" label="工单号" min-width="120" />
            <el-table-column prop="qty" label="用量" width="80" align="right" />
            <el-table-column prop="resultBatchNo" label="产出批次" min-width="140" />
            <el-table-column prop="resultSerialNo" label="产出序列号" min-width="150" />
          </el-table>
          <el-empty v-else-if="searched && !forwardResult.length" description="未找到相关记录" />
        </el-tab-pane>

        <el-tab-pane label="反向追溯" name="backward">
          <el-form :model="searchForm" inline style="margin-bottom: 16px">
            <el-form-item label="序列号">
              <el-input v-model="searchForm.serialNo" placeholder="请输入序列号" style="width: 280px" />
            </el-form-item>
            <el-form-item>
              <el-button type="primary" @click="searchBackward" :loading="loading">查询</el-button>
            </el-form-item>
          </el-form>
          <el-table v-if="backwardResult.length" :data="backwardResult" border stripe v-loading="loading">
            <el-table-column type="index" label="#" width="50" />
            <el-table-column prop="materialName" label="物料名称" min-width="140" />
            <el-table-column prop="batchNo" label="批次号" min-width="140" />
            <el-table-column prop="supplier" label="供应商" min-width="120" />
            <el-table-column prop="inboundDate" label="入库日期" width="120" />
            <el-table-column prop="qty" label="数量" width="80" align="right" />
          </el-table>
          <el-empty v-else-if="searched && !backwardResult.length" description="未找到相关记录" />
        </el-tab-pane>
      </el-tabs>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { traceApi } from '../../api/trace'

const activeTab = ref('batch')
const loading = ref(false)
const searched = ref(false)

const searchForm = reactive({
  batchNo: '',
  serialNo: ''
})

const forwardForm = reactive({
  materialId: 1,
  batchNo: ''
})

const batchResult = ref<any[]>([])
const serialResult = ref<any[]>([])
const forwardResult = ref<any[]>([])
const backwardResult = ref<any[]>([])

async function searchByBatch() {
  if (!searchForm.batchNo) return
  loading.value = true
  searched.value = true
  try {
    const res: any = await traceApi.byBatch(searchForm.batchNo)
    batchResult.value = res.data || []
  } finally {
    loading.value = false
  }
}

async function searchBySerial() {
  if (!searchForm.serialNo) return
  loading.value = true
  searched.value = true
  try {
    const res: any = await traceApi.bySerial(searchForm.serialNo)
    serialResult.value = res.data || []
  } finally {
    loading.value = false
  }
}

async function searchForward() {
  if (!forwardForm.materialId || !forwardForm.batchNo) return
  loading.value = true
  searched.value = true
  try {
    const res: any = await traceApi.forward(forwardForm.materialId, forwardForm.batchNo)
    forwardResult.value = res.data || []
  } finally {
    loading.value = false
  }
}

async function searchBackward() {
  if (!searchForm.serialNo) return
  loading.value = true
  searched.value = true
  try {
    const res: any = await traceApi.backward(searchForm.serialNo)
    backwardResult.value = res.data || []
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.page-container { padding: 16px; }
</style>
