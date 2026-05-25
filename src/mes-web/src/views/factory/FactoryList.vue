<template>
  <div class="page-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span>工厂管理</span>
          <el-button type="primary" @click="openDialog()">新增工厂</el-button>
        </div>
      </template>

      <el-table :data="list" border stripe v-loading="loading">
        <el-table-column prop="id" label="ID" width="80" />
        <el-table-column prop="code" label="工厂编码" min-width="120" />
        <el-table-column prop="name" label="工厂名称" min-width="200" />
        <el-table-column prop="address" label="地址" min-width="250" show-overflow-tooltip />
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

    <el-dialog v-model="dialogVisible" :title="dialogTitle" width="500px">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="100px">
        <el-form-item label="工厂编码" prop="code">
          <el-input v-model="form.code" />
        </el-form-item>
        <el-form-item label="工厂名称" prop="name">
          <el-input v-model="form.name" />
        </el-form-item>
        <el-form-item label="地址" prop="address">
          <el-input v-model="form.address" type="textarea" :rows="3" />
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
import { factoryApi, type Factory } from '../../api/factory'

const list = ref<Factory[]>([])
const loading = ref(false)
const dialogVisible = ref(false)
const submitting = ref(false)
const isEdit = ref(false)
const formRef = ref()

const form = ref<Factory>({ code: '', name: '', address: '', status: 1 })
const rules = {
  code: [{ required: true, message: '请输入工厂编码', trigger: 'blur' }],
  name: [{ required: true, message: '请输入工厂名称', trigger: 'blur' }]
}

const dialogTitle = () => isEdit.value ? '编辑工厂' : '新增工厂'

async function loadData() {
  loading.value = true
  try {
    const res: any = await factoryApi.list()
    list.value = res.data || []
  } finally {
    loading.value = false
  }
}

function openDialog(row?: Factory) {
  isEdit.value = !!row
  form.value = row ? { ...row } : { code: '', name: '', address: '', status: 1 }
  dialogVisible.value = true
}

async function handleSubmit() {
  const valid = await formRef.value.validate().catch(() => false)
  if (!valid) return
  submitting.value = true
  try {
    if (isEdit.value) {
      await factoryApi.update(form.value.id!, form.value)
      ElMessage.success('更新成功')
    } else {
      await factoryApi.create(form.value)
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    await loadData()
  } finally {
    submitting.value = false
  }
}

async function handleDelete(row: Factory) {
  await ElMessageBox.confirm('确定删除该工厂吗？', '提示', { type: 'warning' })
  await factoryApi.delete(row.id!)
  ElMessage.success('删除成功')
  await loadData()
}

onMounted(loadData)
</script>

<style scoped>
.page-container { padding: 16px; }
.card-header { display: flex; justify-content: space-between; align-items: center; }
</style>
