<template>
  <div class="page-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span>用户管理</span>
          <el-button type="primary" @click="openDialog()" v-role="'admin'">新增用户</el-button>
        </div>
      </template>

      <el-table :data="list" border stripe v-loading="loading">
        <el-table-column prop="id" label="ID" width="80" />
        <el-table-column prop="username" label="用户名" min-width="120" />
        <el-table-column prop="displayName" label="显示名称" min-width="120" />
        <el-table-column prop="email" label="邮箱" min-width="160" show-overflow-tooltip />
        <el-table-column prop="phone" label="手机" min-width="130" />
        <el-table-column label="角色" min-width="200">
          <template #default="{ row }">
            <el-tag
              v-for="role in (row.roles || [])"
              :key="role"
              type="warning"
              style="margin-right: 4px"
            >
              {{ role }}
            </el-tag>
            <span v-if="!row.roles || row.roles.length === 0" style="color: #999">无</span>
          </template>
        </el-table-column>
        <el-table-column label="状态" width="90">
          <template #default="{ row }">
            <el-tag :type="row.status ? 'success' : 'info'">
              {{ row.status ? '启用' : '停用' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="最后登录" min-width="170">
          <template #default="{ row }">
            {{ row.lastLoginTime || '-' }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="220" fixed="right">
          <template #default="{ row }">
            <el-button size="small" @click="openDialog(row)" v-permission="'user:edit'">编辑</el-button>
            <el-button size="small" type="danger" @click="handleDelete(row)" v-permission="'user:delete'">删除</el-button>
            <el-button size="small" @click="openRoleDialog(row)" v-role="'admin'">分配角色</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-dialog v-model="dialogVisible" :title="dialogTitle" width="500px">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="100px">
        <el-form-item label="用户名" prop="username">
          <el-input v-model="form.username" placeholder="请输入用户名" />
        </el-form-item>
        <el-form-item v-if="!isEdit" label="密码" prop="password">
          <el-input v-model="form.password" type="password" show-password placeholder="请输入密码" />
        </el-form-item>
        <el-form-item label="显示名称" prop="displayName">
          <el-input v-model="form.displayName" placeholder="请输入显示名称" />
        </el-form-item>
        <el-form-item label="邮箱" prop="email">
          <el-input v-model="form.email" placeholder="请输入邮箱" />
        </el-form-item>
        <el-form-item label="手机" prop="phone">
          <el-input v-model="form.phone" placeholder="请输入手机号" />
        </el-form-item>
        <el-form-item label="状态">
          <el-switch v-model="form.status" active-text="启用" inactive-text="停用" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSubmit" :loading="submitting">保存</el-button>
      </template>
    </el-dialog>

    <el-dialog v-model="roleDialogVisible" title="分配角色" width="500px">
      <el-form label-width="100px">
        <el-form-item label="用户名">
          <span>{{ currentUser?.username }}</span>
        </el-form-item>
        <el-form-item label="角色">
          <el-checkbox-group v-model="selectedRoles">
            <el-checkbox
              v-for="role in allRoles"
              :key="role.name"
              :label="role.name"
              :value="role.name"
            >
              {{ role.name }}
              <span v-if="role.description" style="color: #999; font-size: 12px; margin-left: 4px">
                ({{ role.description }})
              </span>
            </el-checkbox>
          </el-checkbox-group>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="roleDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleAssignRoles" :loading="roleSubmitting">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { userApi } from '../../api/user'
import { roleApi } from '../../api/role'

interface User {
  id: number
  username: string
  displayName: string
  email?: string
  phone?: string
  roles?: string[]
  status?: boolean
  lastLoginTime?: string
}

interface Role {
  id?: number
  name: string
  description?: string
}

interface UserForm {
  username: string
  password: string
  displayName: string
  email: string
  phone: string
  status: boolean
}

const list = ref<User[]>([])
const loading = ref(false)

// 新增/编辑对话框
const dialogVisible = ref(false)
const submitting = ref(false)
const isEdit = ref(false)
const formRef = ref()
const editId = ref<number | null>(null)

const form = ref<UserForm>({
  username: '',
  password: '',
  displayName: '',
  email: '',
  phone: '',
  status: true
})

const rules = computed(() => ({
  username: [{ required: true, message: '请输入用户名', trigger: 'blur' }],
  password: isEdit.value
    ? []
    : [{ required: true, message: '请输入密码', trigger: 'blur' }],
  displayName: [{ required: true, message: '请输入显示名称', trigger: 'blur' }]
}))

const dialogTitle = computed(() => (isEdit.value ? '编辑用户' : '新增用户'))

// 角色分配对话框
const roleDialogVisible = ref(false)
const roleSubmitting = ref(false)
const allRoles = ref<Role[]>([])
const currentUser = ref<User | null>(null)
const selectedRoles = ref<string[]>([])

async function loadData() {
  loading.value = true
  try {
    const res: any = await userApi.list()
    list.value = res.data || []
  } finally {
    loading.value = false
  }
}

async function loadRoles() {
  try {
    const res: any = await roleApi.list()
    allRoles.value = res.data || []
  } catch {
    // 角色列表加载失败不影响用户列表
  }
}

function openDialog(row?: User) {
  isEdit.value = !!row
  if (row) {
    editId.value = row.id
    form.value = {
      username: row.username,
      password: '',
      displayName: row.displayName,
      email: row.email || '',
      phone: row.phone || '',
      status: row.status ?? true
    }
  } else {
    editId.value = null
    form.value = {
      username: '',
      password: '',
      displayName: '',
      email: '',
      phone: '',
      status: true
    }
  }
  dialogVisible.value = true
}

async function handleSubmit() {
  const valid = await formRef.value.validate().catch(() => false)
  if (!valid) return
  submitting.value = true
  try {
    if (isEdit.value && editId.value) {
      await userApi.update(editId.value, {
        username: form.value.username,
        displayName: form.value.displayName,
        email: form.value.email || null,
        phone: form.value.phone || null,
        status: form.value.status
      })
      ElMessage.success('更新成功')
    } else {
      await userApi.create({
        username: form.value.username,
        password: form.value.password,
        displayName: form.value.displayName,
        email: form.value.email || null,
        phone: form.value.phone || null,
        status: form.value.status
      })
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    await loadData()
  } finally {
    submitting.value = false
  }
}

async function handleDelete(row: User) {
  await ElMessageBox.confirm('确定删除该用户吗？', '提示', { type: 'warning' })
  await userApi.delete(row.id)
  ElMessage.success('删除成功')
  await loadData()
}

async function openRoleDialog(row: User) {
  currentUser.value = row
  selectedRoles.value = [...(row.roles || [])]
  roleDialogVisible.value = true
  if (allRoles.value.length === 0) {
    await loadRoles()
  }
}

async function handleAssignRoles() {
  if (!currentUser.value) return
  roleSubmitting.value = true
  try {
    await userApi.assignRoles(currentUser.value.id, selectedRoles.value)
    ElMessage.success('角色分配成功')
    roleDialogVisible.value = false
    await loadData()
  } finally {
    roleSubmitting.value = false
  }
}

onMounted(() => {
  loadData()
  loadRoles()
})
</script>

<style scoped>
.page-container { padding: 16px; }
.card-header { display: flex; justify-content: space-between; align-items: center; }
</style>
