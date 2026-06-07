<template>
  <el-container class="layout-container">
    <!-- 左侧菜单 -->
    <el-aside width="220px" class="layout-aside">
      <div class="aside-logo">
        <el-icon :size="28" color="#fff"><Monitor /></el-icon>
        <span class="aside-title">MES 系统</span>
      </div>
      <el-menu
        :default-active="activeMenu"
        background-color="#304156"
        text-color="#bfcbd9"
        active-text-color="#409eff"
        router
      >
        <el-menu-item index="/dashboard">
          <el-icon><HomeFilled /></el-icon>
          <span>首页看板</span>
        </el-menu-item>
        <el-sub-menu index="basic">
          <template #title>
            <el-icon><Folder /></el-icon>
            <span>基础数据</span>
          </template>
          <el-menu-item index="/factory">工厂管理</el-menu-item>
          <el-menu-item index="/workshop">车间管理</el-menu-item>
          <el-menu-item index="/production-line">产线管理</el-menu-item>
          <el-menu-item index="/workstation">工位管理</el-menu-item>
          <el-menu-item index="/material">物料管理</el-menu-item>
          <el-menu-item index="/bom">BOM 管理</el-menu-item>
          <el-menu-item index="/routing">工艺路线</el-menu-item>
          <el-menu-item index="/equipment">设备管理</el-menu-item>
        </el-sub-menu>
        <el-menu-item index="/work-order">
          <el-icon><List /></el-icon>
          <span>工单管理</span>
        </el-menu-item>
        <el-menu-item index="/work-report">
          <el-icon><Document /></el-icon>
          <span>报工管理</span>
        </el-menu-item>
        <el-menu-item index="/qc">
          <el-icon><CircleCheck /></el-icon>
          <span>质量管理</span>
        </el-menu-item>
        <el-menu-item index="/trace">
          <el-icon><Search /></el-icon>
          <span>追溯查询</span>
        </el-menu-item>
        <el-menu-item index="/andon">
          <el-icon><WarningFilled /></el-icon>
          <span>异常看板</span>
        </el-menu-item>
        <el-menu-item index="/scheduling">
          <el-icon><TrendCharts /></el-icon>
          <span>排产管理</span>
        </el-menu-item>
        <el-menu-item index="/line-task-board">
          <el-icon><Grid /></el-icon>
          <span>产线任务看板</span>
        </el-menu-item>
        <el-menu-item index="/qc-dashboard">
          <el-icon><DataAnalysis /></el-icon>
          <span>质检看板</span>
        </el-menu-item>
        <el-menu-item index="/qc-checkpoint">
          <el-icon><CircleCheckFilled /></el-icon>
          <span>质检检查点</span>
        </el-menu-item>
        <el-menu-item index="/big-screen">
          <el-icon><Monitor /></el-icon>
          <span>大屏看板</span>
        </el-menu-item>
        <el-sub-menu index="integration">
          <template #title>
            <el-icon><Link /></el-icon>
            <span>系统集成</span>
          </template>
          <el-menu-item index="/integration/dashboard">系统集成</el-menu-item>
          <el-menu-item index="/integration/logs">同步日志</el-menu-item>
        </el-sub-menu>
        <el-sub-menu index="system">
          <template #title>
            <el-icon><Setting /></el-icon>
            <span>系统设置</span>
          </template>
          <el-menu-item index="/system/seed">种子数据</el-menu-item>
        </el-sub-menu>
      </el-menu>
    </el-aside>

    <!-- 右侧内容 -->
    <el-container>
      <!-- 顶部 Header -->
      <el-header class="layout-header">
        <span class="header-breadcrumb">MES 制造执行系统</span>
        <div class="header-right">
          <el-dropdown @command="handleCommand">
            <span class="header-user">
              <el-icon :size="18"><UserFilled /></el-icon>
              {{ authStore.user?.displayName || authStore.user?.username }}
              <el-icon><ArrowDown /></el-icon>
            </span>
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item command="logout">退出登录</el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
        </div>
      </el-header>

      <!-- 主内容区 -->
      <el-main class="layout-main">
        <router-view />
      </el-main>
    </el-container>
  </el-container>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import {
  Monitor, HomeFilled, Folder, List, Document, CircleCheck, CircleCheckFilled, Tools, Search, WarningFilled,
  UserFilled, ArrowDown, TrendCharts, Grid, DataAnalysis, Setting, Link
} from '@element-plus/icons-vue'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const activeMenu = computed(() => route.path)

function handleCommand(command: string) {
  if (command === 'logout') {
    authStore.logout()
    router.push('/login')
  }
}
</script>

<style scoped>
.layout-container {
  height: 100%;
}

.layout-aside {
  background-color: #304156;
  overflow-y: auto;
}

.aside-logo {
  height: 60px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

.aside-title {
  color: #fff;
  font-size: 18px;
  font-weight: bold;
}

.layout-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: #fff;
  border-bottom: 1px solid #e6e6e6;
  padding: 0 20px;
  height: 60px;
}

.header-breadcrumb {
  font-size: 16px;
  color: #303133;
}

.header-right {
  display: flex;
  align-items: center;
}

.header-user {
  display: flex;
  align-items: center;
  gap: 6px;
  cursor: pointer;
  color: #303133;
  font-size: 14px;
}

.layout-main {
  background: #f0f2f5;
  padding: 0;
}
</style>
