<template>
  <div class="page-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span>物料管理</span>
          <el-button type="primary" @click="openDialog()">新增物料</el-button>
        </div>
      </template>

      <el-table :data="list" border stripe v-loading="loading">
        <el-table-column prop="id" label="ID" width="80" />
        <el-table-column prop="code" label="物料编码" min-width="130" />
        <el-table-column prop="name" label="物料名称" min-width="200" />
        <el-table-column prop="spec" label="规格型号" min-width="150" show-overflow-tooltip />
        <el-table-column prop="unit" label="单位" width="80" />
        <el-table-column prop="category" label="分类" min-width="100" />
        <el-table-column prop="bomLevel" label="BOM层级" width="100" />
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
    </el-card>

    <el-dialog v-model="dialogVisible" :title="dialogTitle" width="550px">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="100px">
        <el-form-item label="物料编码" prop="code">
          <el-input v-model="form.code" />
        </el-form-item>
        <el-form-item label="物料名称" prop="name">
          <el-input v-model="form.name" />
        </el-form-item>
        <el-form-item label="规格型号" prop="spec">
          <el-input v-model="form.spec" />
        </el-form-item>
        <el-form-item label="单位" prop="unit">
          <el-select v-model="form.unit" placeholder="请选择" style="width: 100%">
            <el-option label="个" value="个" />
            <el-option label="件" value="件" />
            <el-option label="箱" value="箱" />
            <el-option label="kg" value="kg" />
            <el-option label="m" value="m" />
            <el-option label="L" value="L" />
          </el-select>
        </el-form-item>
        <el-form-item label="分类" prop="category">
          <el-select v-model="form.category" placeholder="请选择" style="width: 100%">
            <el-option label="原材料" value="原材料" />
            <el-option label="半成品" value="半成品" />
            <el-option label="成品" value="成品" />
            <el-option label="辅料" value="辅料" />
            <el-option label="包材" value="包材" />
          </el-select>
        </el-form-item>
        <el-form-item label="BOM层级" prop="bomLevel">
          <el-input-number v-model="form.bomLevel" :min="0" :max="10" style="width: 100%" />
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
import { materialApi, type Material } from '../../api/material'

const list = ref<Material[]>([])
const loading = ref(false)
const dialogVisible = ref(false)
const submitting = ref(false)
const isEdit = ref(false)
const formRef = ref()

const form = ref<Material>({ code: '', name: '', spec: '', unit: '', category: '', bomLevel: 0, status: 1 })
const rules = {
  code: [{ required: true, message: '请输入物料编码', trigger: 'blur' }],
  name: [{ required: true, message: '请输入物料名称', trigger: 'blur' }]
}

const dialogTitle = () => isEdit.value ? '编辑物料' : '新增物料'

async function loadData() {
  loading.value = true
  try {
    const res: any = await materialApi.list()
    list.value = res.data || []
  } finally {
    loading.value = false
  }
}

function openDialog(row?: Material) {
  isEdit.value = !!row
  form.value = row ? { ...row } : { code: '', name: '', spec: '', unit: '', category: '', bomLevel: 0, status: 1 }
  dialogVisible.value = true
}

async function handleSubmit() {
  const valid = await formRef.value.validate().catch(() => false)
  if (!valid) return
  submitting.value = true
  try {
    if (isEdit.value) {
      await materialApi.update(form.value.id!, form.value)
      ElMessage.success('更新成功')
    } else {
      await materialApi.create(form.value)
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    await loadData()
  } finally {
    submitting.value = false
  }
}

async function handleDelete(row: Material) {
  await ElMessageBox.confirm('确定删除该物料吗？', '提示', { type: 'warning' })
  await materialApi.delete(row.id!)
  ElMessage.success('删除成功')
  await loadData()
}

onMounted(loadData)
</script>

<style scoped>
.page-container { padding: 16px; }
.card-header { display: flex; justify-content: space-between; align-items: center; }
</style>
