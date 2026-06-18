<template>
  <div class="page-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span>用户管理</span>
        </div>
      </template>

      <el-table :data="list" border stripe v-loading="loading">
        <el-table-column prop="id" label="ID" width="80" />
        <el-table-column prop="username" label="用户名" min-width="120" />
        <el-table-column prop="displayName" label="显示名称" min-width="120" />
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
            <el-tag :type="row.status === 1 || row.status === 'active' ? 'success' : 'info'">
              {{ row.status === 1 || row.status === 'active' ? '启用' : '停用' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="最后登录" min-width="170">
          <template #default="{ row }">
            {{ row.lastLoginTime || '-' }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="120" fixed="right">
          <template #default="{ row }">
            <el-button size="small" @click="openRoleDialog(row)">分配角色</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

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
        <el-button type="primary" @click="handleAssignRoles" :loading="submitting">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import http from '../../api/index'

interface User {
  id: number
  username: string
  displayName: string
  roles?: string[]
  status?: number | string
  lastLoginTime?: string
}

interface Role {
  id?: number
  name: string
  description?: string
}

const list = ref<User[]>([])
const loading = ref(false)
const roleDialogVisible = ref(false)
const submitting = ref(false)
const allRoles = ref<Role[]>([])
const currentUser = ref<User | null>(null)
const selectedRoles = ref<string[]>([])

async function loadData() {
  loading.value = true
  try {
    const res: any = await http.get('/users')
    list.value = res.data || []
  } finally {
    loading.value = false
  }
}

async function loadRoles() {
  try {
    const res: any = await http.get('/roles')
    allRoles.value = res.data || []
  } catch {
    // 角色列表加载失败不影响用户列表
  }
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
  submitting.value = true
  try {
    await http.put(`/users/${currentUser.value.id}/roles`, {
      roles: selectedRoles.value
    })
    ElMessage.success('角色分配成功')
    roleDialogVisible.value = false
    await loadData()
  } finally {
    submitting.value = false
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
