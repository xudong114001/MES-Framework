<template>
  <div class="knowledge-search">
    <div class="search-bar">
      <el-input v-model="query" placeholder="搜索知识库..." @keyup.enter="doSearch" clearable>
        <template #append>
          <el-button @click="doSearch">搜索</el-button>
        </template>
      </el-input>
      <el-select v-model="category" placeholder="全部分类" clearable @change="doSearch" style="width: 160px">
        <el-option v-for="(name, key) in categoryMap" :key="key" :label="name" :value="Number(key)" />
      </el-select>
    </div>

    <div class="result-info" v-if="searched">
      共找到 <strong>{{ total }}</strong> 条结果
    </div>

    <div class="result-list" v-loading="loading">
      <el-card v-for="item in items" :key="item.id" class="result-card" shadow="hover" @click="showDetail(item)">
        <div class="card-header">
          <span class="card-title">{{ item.title }}</span>
          <el-tag size="small">{{ categoryMap[item.category] || '通用' }}</el-tag>
        </div>
        <p class="card-preview">{{ (item.content || '').slice(0, 150) }}{{ item.content?.length > 150 ? '...' : '' }}</p>
        <div class="card-keywords" v-if="item.keywords">
          <el-tag v-for="kw in item.keywords.split(',')" :key="kw" size="small" type="info" class="kw-tag">{{ kw.trim() }}</el-tag>
        </div>
      </el-card>
    </div>

    <el-empty v-if="searched && !items.length && !loading" description="未找到相关内容" />

    <el-pagination
      v-if="total > pageSize"
      v-model:current-page="page"
      :page-size="pageSize"
      :total="total"
      layout="prev, pager, next"
      @current-change="doSearch"
      class="pagination"
    />

    <el-dialog v-model="detailVisible" :title="detailItem?.title || ''" width="700px">
      <div class="detail-meta">
        <el-tag>{{ categoryMap[detailItem?.category] || '通用' }}</el-tag>
        <span v-if="detailItem?.keywords" class="detail-keywords">关键词: {{ detailItem.keywords }}</span>
      </div>
      <div class="detail-content">{{ detailItem?.content }}</div>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { knowledgeBaseApi } from '../../api/ai'

const categoryMap: Record<number, string> = { 0: '工艺标准', 1: '质检规范', 2: '设备手册', 3: '安全规程', 4: '通用' }

const query = ref('')
const category = ref<number | undefined>(undefined)
const items = ref<any[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = ref(20)
const loading = ref(false)
const searched = ref(false)
const detailVisible = ref(false)
const detailItem = ref<any>(null)

async function doSearch() {
  page.value = 1
  searched.value = true
  loading.value = true
  try {
    if (query.value.trim()) {
      const res: any = await knowledgeBaseApi.search({ q: query.value.trim(), category: category.value, page: page.value, pageSize: pageSize.value })
      items.value = res.data?.items || res.data || []
      total.value = res.data?.totalCount || res.total || 0
    } else {
      const res: any = await knowledgeBaseApi.list({ category: category.value, page: page.value, pageSize: pageSize.value })
      items.value = res.data || []
      total.value = res.total || 0
    }
  } catch { /* handled */ }
  loading.value = false
}

function showDetail(item: any) {
  detailItem.value = item
  detailVisible.value = true
}
</script>

<style scoped>
.knowledge-search { padding: 20px; }
.search-bar { display: flex; gap: 12px; margin-bottom: 16px; }
.search-bar .el-input { flex: 1; }
.result-info { margin-bottom: 12px; color: #909399; font-size: 14px; }
.result-list { display: flex; flex-direction: column; gap: 12px; }
.result-card { cursor: pointer; }
.card-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 8px; }
.card-title { font-size: 16px; font-weight: 600; color: #303133; }
.card-preview { color: #606266; font-size: 14px; line-height: 1.6; margin: 8px 0; }
.card-keywords { display: flex; gap: 4px; flex-wrap: wrap; }
.kw-tag { font-size: 12px; }
.pagination { margin-top: 16px; justify-content: center; }
.detail-meta { margin-bottom: 16px; display: flex; gap: 12px; align-items: center; }
.detail-keywords { color: #909399; font-size: 14px; }
.detail-content { white-space: pre-wrap; line-height: 1.8; color: #303133; font-size: 15px; }
</style>
