<template>
  <div class="page-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span>BOM管理</span>
        </div>
      </template>

      <div style="margin-bottom: 16px; display: flex; gap: 12px; align-items: center;">
        <span>选择产品：</span>
        <el-select
          v-model="selectedProductId"
          placeholder="请选择产品（成品/半成品）"
          filterable
          style="width: 300px"
          @change="onProductChange"
        >
          <el-option
            v-for="m in materials"
            :key="m.id"
            :label="`[${m.code}] ${m.name}`"
            :value="m.id"
          />
        </el-select>
        <el-button type="primary" :disabled="!selectedProductId" @click="openDialog()">
          新增BOM条目
        </el-button>
      </div>

      <el-table :data="bomList" border stripe v-loading="loading">
        <el-table-column prop="id" label="ID" width="80" />
        <el-table-column prop="materialCode" label="子件编码" min-width="130" />
        <el-table-column prop="materialName" label="子件名称" min-width="200" />
        <el-table-column prop="quantity" label="用量" width="100" />
        <el-table-column prop="scrapRate" label="损耗率(%)" width="120">
          <template #default="{ row }">
            {{ row.scrapRate != null ? row.scrapRate + '%' : '-' }}
          </template>
        </el-table-column>
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
      <div v-if="!selectedProductId" style="text-align: center; padding: 40px; color: #999;">
        请先在上方选择一个产品以查看其 BOM 明细
      </div>
    </el-card>

    <el-dialog v-model="dialogVisible" :title="dialogTitle" width="500px">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="120px">
        <el-form-item label="子件物料" prop="materialId">
          <el-select v-model="form.materialId" placeholder="请选择物料" filterable style="width: 100%">
            <el-option
              v-for="m in allMaterials"
              :key="m.id"
              :label="`[${m.code}] ${m.name}`"
              :value="m.id"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="用量" prop="quantity">
          <el-input-number v-model="form.quantity" :min="0.01" :step="0.01" :precision="2" style="width: 100%" />
        </el-form-item>
        <el-form-item label="损耗率(%)" prop="scrapRate">
          <el-input-number v-model="form.scrapRate" :min="0" :max="100" :step="0.1" :precision="1" style="width: 100%" />
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
import { bomApi, type Bom } from '../../api/bom'
import { materialApi, type Material } from '../../api/material'

const selectedProductId = ref<number | undefined>(undefined)
const materials = ref<Material[]>([])
const allMaterials = ref<Material[]>([])
const bomList = ref<Bom[]>([])
const loading = ref(false)
const dialogVisible = ref(false)
const submitting = ref(false)
const isEdit = ref(false)
const formRef = ref()

const form = ref<Bom>({ productId: 0, materialId: 0, quantity: 1, scrapRate: 0, status: 1 })
const rules = {
  materialId: [{ required: true, message: '请选择子件物料', trigger: 'change' }],
  quantity: [{ required: true, message: '请输入用量', trigger: 'blur' }]
}

const dialogTitle = () => isEdit.value ? '编辑BOM条目' : '新增BOM条目'

async function loadMaterials() {
  try {
    const res: any = await materialApi.list()
    materials.value = (res.data || []).filter((m: Material) => m.category === '成品' || m.category === '半成品')
    allMaterials.value = res.data || []
  } catch { /* ignore */ }
}

async function onProductChange(productId: number) {
  if (!productId) {
    bomList.value = []
    return
  }
  loading.value = true
  try {
    const res: any = await bomApi.listByProduct(productId)
    bomList.value = res.data || []
  } finally {
    loading.value = false
  }
}

function openDialog(row?: Bom) {
  isEdit.value = !!row
  form.value = row
    ? { ...row }
    : { productId: selectedProductId.value!, materialId: 0, quantity: 1, scrapRate: 0, status: 1 }
  dialogVisible.value = true
}

async function handleSubmit() {
  const valid = await formRef.value.validate().catch(() => false)
  if (!valid) return
  submitting.value = true
  try {
    const data = { ...form.value, productId: selectedProductId.value! }
    if (isEdit.value) {
      await bomApi.update(data.id!, data)
      ElMessage.success('更新成功')
    } else {
      await bomApi.create(data)
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    await onProductChange(selectedProductId.value!)
  } finally {
    submitting.value = false
  }
}

async function handleDelete(row: Bom) {
  await ElMessageBox.confirm('确定删除该BOM条目吗？', '提示', { type: 'warning' })
  await bomApi.delete(row.id!)
  ElMessage.success('删除成功')
  await onProductChange(selectedProductId.value!)
}

onMounted(loadMaterials)
</script>

<style scoped>
.page-container { padding: 16px; }
.card-header { display: flex; justify-content: space-between; align-items: center; }
</style>
