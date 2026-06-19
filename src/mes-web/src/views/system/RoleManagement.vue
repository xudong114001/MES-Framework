<template>
  <div class="page-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span>角色管理</span>
          <el-button type="primary" @click="openDialog()" v-role="'admin'">新增角色</el-button>
        </div>
      </template>

      <el-table :data="list" border stripe v-loading="loading">
        <el-table-column prop="id" label="ID" width="80" />
        <el-table-column prop="name" label="角色名称" min-width="160" />
        <el-table-column prop="description" label="描述" min-width="250" show-overflow-tooltip />
        <el-table-column prop="permissionCount" label="权限数量" width="100" />
        <el-table-column label="操作" width="240" fixed="right">
          <template #default="{ row }">
            <el-button size="small" @click="openDialog(row)" v-permission="'role:edit'">编辑</el-button>
            <el-button size="small" type="warning" @click="openPermissionDialog(row)" v-role="'admin'">分配权限</el-button>
            <el-button size="small" type="danger" @click="handleDelete(row)" v-permission="'role:delete'">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 角色新增/编辑对话框 -->
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

    <!-- 权限分配对话框 -->
    <el-dialog v-model="permissionDialogVisible" title="分配权限" width="600px">
      <div class="permission-dialog-content">
        <div class="role-name">角色：{{ currentRole?.name }}</div>
        <el-checkbox-group v-model="selectedPermissions">
          <div v-for="group in permissionGroups" :key="group.label" class="permission-group">
            <div class="permission-group-title">{{ group.label }}</div>
            <div class="permission-group-items">
              <el-checkbox
                v-for="perm in group.permissions"
                :key="perm.value"
                :value="perm.value"
                :label="perm.value"
              >
                {{ perm.label }}
              </el-checkbox>
            </div>
          </div>
        </el-checkbox-group>
      </div>
      <template #footer>
        <el-button @click="permissionDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handlePermissionSubmit" :loading="permissionSubmitting">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import http from '../../api/index'

interface Role {
  id?: number
  name: string
  description?: string
  permissionCount?: number
}

// 权限列表定义
const permissionGroups = [
  {
    label: '用户管理',
    permissions: [
      { value: 'user:view', label: '查看用户' },
      { value: 'user:create', label: '创建用户' },
      { value: 'user:edit', label: '编辑用户' },
      { value: 'user:delete', label: '删除用户' }
    ]
  },
  {
    label: '角色管理',
    permissions: [
      { value: 'role:view', label: '查看角色' },
      { value: 'role:create', label: '创建角色' },
      { value: 'role:edit', label: '编辑角色' },
      { value: 'role:delete', label: '删除角色' }
    ]
  },
  {
    label: '工单管理',
    permissions: [
      { value: 'workorder:view', label: '查看工单' },
      { value: 'workorder:create', label: '创建工单' },
      { value: 'workorder:edit', label: '编辑工单' },
      { value: 'workorder:delete', label: '删除工单' }
    ]
  },
  {
    label: '物料管理',
    permissions: [
      { value: 'material:view', label: '查看物料' },
      { value: 'material:create', label: '创建物料' },
      { value: 'material:edit', label: '编辑物料' },
      { value: 'material:delete', label: '删除物料' }
    ]
  },
  {
    label: '设备管理',
    permissions: [
      { value: 'equipment:view', label: '查看设备' },
      { value: 'equipment:create', label: '创建设备' },
      { value: 'equipment:edit', label: '编辑设备' },
      { value: 'equipment:delete', label: '删除设备' },
      { value: 'equipment:maintain', label: '设备保养' }
    ]
  },
  {
    label: '报表管理',
    permissions: [
      { value: 'report:view', label: '查看报表' },
      { value: 'report:export', label: '导出报表' }
    ]
  },
  {
    label: '安灯管理',
    permissions: [
      { value: 'andon:view', label: '查看安灯' },
      { value: 'andon:manage', label: '安灯管理' }
    ]
  },
  {
    label: '质量管理',
    permissions: [
      { value: 'qc:view', label: '查看质检' },
      { value: 'qc:manage', label: '质检管理' }
    ]
  },
  {
    label: 'AI 知识库',
    permissions: [
      { value: 'ai:view', label: '查看知识库' },
      { value: 'ai:manage', label: '知识库管理' }
    ]
  }
]

const list = ref<Role[]>([])
const loading = ref(false)
const dialogVisible = ref(false)
const permissionDialogVisible = ref(false)
const submitting = ref(false)
const permissionSubmitting = ref(false)
const isEdit = ref(false)
const formRef = ref()

const form = ref<Role>({ name: '', description: '' })
const rules = {
  name: [{ required: true, message: '请输入角色名称', trigger: 'blur' }]
}

// 权限分配相关
const currentRole = ref<Role | null>(null)
const selectedPermissions = ref<string[]>([])

const dialogTitle = computed(() => (isEdit.value ? '编辑角色' : '新增角色'))

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

async function openPermissionDialog(row: Role) {
  currentRole.value = row
  selectedPermissions.value = []

  try {
    const res: any = await http.get(`/roles/${row.id}/permissions`)
    selectedPermissions.value = res.data || []
  } catch (e) {
    // 如果获取失败，使用空列表
    selectedPermissions.value = []
  }

  permissionDialogVisible.value = true
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

async function handlePermissionSubmit() {
  if (!currentRole.value?.id) return

  permissionSubmitting.value = true
  try {
    await http.put(`/roles/${currentRole.value.id}/permissions`, {
      permissions: selectedPermissions.value
    })
    ElMessage.success('权限分配成功')
    permissionDialogVisible.value = false
    await loadData()
  } finally {
    permissionSubmitting.value = false
  }
}

onMounted(loadData)
</script>

<style scoped>
.page-container { padding: 16px; }
.card-header { display: flex; justify-content: space-between; align-items: center; }

.permission-dialog-content {
  max-height: 400px;
  overflow-y: auto;
}

.role-name {
  margin-bottom: 16px;
  font-size: 16px;
  font-weight: 500;
  color: #409eff;
}

.permission-group {
  margin-bottom: 16px;
}

.permission-group-title {
  font-weight: 600;
  margin-bottom: 8px;
  padding-left: 8px;
  border-left: 3px solid #409eff;
}

.permission-group-items {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  padding-left: 20px;
}

.permission-group-items .el-checkbox {
  width: 140px;
}
</style>