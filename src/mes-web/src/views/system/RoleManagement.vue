<template>
  <div class="page-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span>角色管理</span>
          <el-button type="primary" @click="openDialog()">新增角色</el-button>
        </div>
      </template>

      <el-table :data="list" border stripe v-loading="loading">
        <el-table-column prop="id" label="ID" width="80" />
        <el-table-column prop="name" label="角色名称" min-width="160" />
        <el-table-column prop="description" label="描述" min-width="250" show-overflow-tooltip />
        <el-table-column label="操作" width="180" fixed="right">
          <template #default="{ row }">
            <el-button size="small" @click="openDialog(row)">编辑</el-button>
            <el-button size="small" type="danger" @click="handleDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-dialog v-model="dialogVisible" :title="dialogTitle" width="500px">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="100px">
        <el-form-item label="角色名称" prop="name">
          <el-input v-model="form.name" placeholder="例如：admin, operator" />
        </el-form-item>
        <el-form-item label="描述" prop="description">
          <el-input v-model="form.description" type="textarea" :rows="3" placeholder="角色描述" />
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
import http from '../../api/index'

interface Role {
  id?: number
  name: string
  description?: string
}

const list = ref<Role[]>([])
const loading = ref(false)
const dialogVisible = ref(false)
const submitting = ref(false)
const isEdit = ref(false)
const formRef = ref()

const form = ref<Role>({ name: '', description: '' })
const rules = {
  name: [{ required: true, message: '请输入角色名称', trigger: 'blur' }]
}

const dialogTitle = () => (isEdit.value ? '编辑角色' : '新增角色')

async function loadData() {
  loading.value = true
  try {
    const res: any = await http.get('/roles')
    list.value = res.data || []
  } finally {
    loading.value = false
  }
}

function openDialog(row?: Role) {
  isEdit.value = !!row
  form.value = row ? { ...row } : { name: '', description: '' }
  dialogVisible.value = true
}

async function handleSubmit() {
  const valid = await formRef.value.validate().catch(() => false)
  if (!valid) return
  submitting.value = true
  try {
    if (isEdit.value) {
      await http.put(`/roles/${form.value.id}`, form.value)
      ElMessage.success('更新成功')
    } else {
      await http.post('/roles', form.value)
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    await loadData()
  } finally {
    submitting.value = false
  }
}

async function handleDelete(row: Role) {
  await ElMessageBox.confirm('确定删除该角色吗？', '提示', { type: 'warning' })
  await http.delete(`/roles/${row.id}`)
  ElMessage.success('删除成功')
  await loadData()
}

onMounted(loadData)
</script>

<style scoped>
.page-container { padding: 16px; }
.card-header { display: flex; justify-content: space-between; align-items: center; }
</style>
