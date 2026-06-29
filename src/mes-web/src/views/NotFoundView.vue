<script setup lang="ts">
import { useRouter } from 'vue-router'

const router = useRouter()

const goHome = () => {
  router.push('/')
}
</script>

<template>
  <div class="not-found">
    <div class="content">
      <h1>404</h1>
      <p>抱歉，您访问的页面不存在</p>
      <el-button type="primary" @click="goHome">返回首页</el-button>
    </div>
  </div>
</template>

<style scoped>
.not-found {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 100vh;
}
.content {
  text-align: center;
}
h1 {
  font-size: 72px;
  color: #409eff;
  margin-bottom: 16px;
}
p {
  font-size: 18px;
  color: #909399;
  margin-bottom: 24px;
}
</style>
