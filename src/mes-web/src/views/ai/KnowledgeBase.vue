<template>
  <div class="knowledge-manage">
    <div class="page-header">
      <h2>知识库管理</h2>
      <el-button type="primary" @click="showAddDialog">新增条目</el-button>
    </div>

    <el-table :data="entries" stripe v-loading="loading">
      <el-table-column prop="title" label="标题" min-width="200" />
      <el-table-column label="分类" width="120">
        <template #default="{ row }">{{ categoryMap[row.category] || '通用' }}</template>
      </el-table-column>
      <el-table-column prop="keywords" label="关键词" width="200" />
      <el-table-column prop="createdAt" label="创建时间" width="180" />
      <el-table-column label="操作" width="180">
        <template #default="{ row }">
          <el-button size="small" @click="showEditDialog(row)">编辑</el-button>
          <el-button size="small" type="danger" @click="handleDelete(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-pagination
      v-if="total > pageSize"
      v-model:current-page="page"
      :page-size="pageSize"
      :total="total"
      layout="prev, pager, next"
      @current-change="loadEntries"
      class="pagination"
    />

    <el-dialog v-model="dialogVisible" :title="isEdit ? '编辑条目' : '新增条目'" width="600px">
      <el-form :model="form" label-width="80px">
        <el-form-item label="标题" required>
          <el-input v-model="form.title" placeholder="请输入标题" />
        </el-form-item>
        <el-form-item label="分类" required>
          <el-select v-model="form.category" placeholder="选择分类" style="width: 100%">
            <el-option v-for="(name, key) in categoryMap" :key="key" :label="name" :value="Number(key)" />
          </el-select>
        </el-form-item>
        <el-form-item label="内容" required>
          <el-input v-model="form.content" type="textarea" :rows="6" placeholder="请输入内容" />
        </el-form-item>
        <el-form-item label="关键词">
          <el-input v-model="form.keywords" placeholder="逗号分隔的关键词" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSave" :loading="saving">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { knowledgeBaseApi } from '../../api/ai'

const categoryMap: Record<number, string> = { 0: '工艺标准', 1: '质检规范', 2: '设备手册', 3: '安全规程', 4: '通用' }

const entries = ref<any[]>([])
const loading = ref(false)
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)
const dialogVisible = ref(false)
const isEdit = ref(false)
const saving = ref(false)
const editId = ref<number | null>(null)
const form = ref({ title: '', category: 0, content: '', keywords: '' })

async function loadEntries() {
  loading.value = true
  const res: any = await knowledgeBaseApi.list({ page: page.value, pageSize: pageSize.value })
  entries.value = res.data || []
  total.value = res.total || res.data?.length || 0
  loading.value = false
}

function showAddDialog() {
  isEdit.value = false
  editId.value = null
  form.value = { title: '', category: 0, content: '', keywords: '' }
  dialogVisible.value = true
}

function showEditDialog(row: any) {
  isEdit.value = true
  editId.value = row.id
  form.value = { title: row.title, category: row.category, content: row.content, keywords: row.keywords || '' }
  dialogVisible.value = true
}

async function handleSave() {
  if (!form.value.title || !form.value.content) {
    ElMessage.warning('请填写标题和内容')
    return
  }
  saving.value = true
  try {
    if (isEdit.value && editId.value) {
      await knowledgeBaseApi.update(editId.value, form.value)
      ElMessage.success('更新成功')
    } else {
      await knowledgeBaseApi.add(form.value)
      ElMessage.success('添加成功')
    }
    dialogVisible.value = false
    await loadEntries()
  } catch { /* handled by interceptor */ }
  saving.value = false
}

async function handleDelete(row: any) {
  await ElMessageBox.confirm('确定删除该条目？', '确认')
  await knowledgeBaseApi.delete(row.id)
  ElMessage.success('删��成功')
  await loadEntries()
}

onMounted(loadEntries)
</script>

<style scoped>
.knowledge-manage { padding: 20px; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
.page-header h2 { margin: 0; }
.pagination { margin-top: 16px; justify-content: center; }
</style>
