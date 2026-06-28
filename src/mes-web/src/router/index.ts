import { createRouter, createWebHistory } from 'vue-router'

// 扩展路由 meta 类型，添加角色和权限字段
declare module 'vue-router' {
  interface RouteMeta {
    requiresAuth?: boolean
    title?: string
    icon?: string
    displayInMenu?: boolean
    roles?: string[]
    permissions?: string[]
  }
}

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/login',
      name: 'Login',
      component: () => import('../views/login/LoginView.vue'),
      meta: { requiresAuth: false }
    },
    {
      path: '/big-screen',
      name: 'BigScreen',
      component: () => import('../views/dashboard/BigScreenView.vue'),
      meta: { requiresAuth: true, title: '大屏看板' }
    },
    {
      path: '/',
      component: () => import('../layouts/MainLayout.vue'),
      meta: { requiresAuth: true },
      children: [
        {
          path: '',
          redirect: '/dashboard'
        },
        {
          path: 'dashboard',
          name: 'Dashboard',
          component: () => import('../views/dashboard/DashboardView.vue'),
          meta: { title: '首页看板' }
        },
        {
          path: 'factory',
          name: 'Factory',
          component: () => import('../views/factory/FactoryList.vue'),
          meta: { title: '工厂管理' }
        },
        {
          path: 'workshop',
          name: 'Workshop',
          component: () => import('../views/workshop/WorkshopList.vue'),
          meta: { title: '车间管理' }
        },
        {
          path: 'production-line',
          name: 'ProductionLine',
          component: () => import('../views/production-line/ProductionLineList.vue'),
          meta: { title: '产线管理' }
        },
        {
          path: 'workstation',
          name: 'Workstation',
          component: () => import('../views/workstation/WorkstationList.vue'),
          meta: { title: '工位管理' }
        },
        {
          path: 'material',
          name: 'Material',
          component: () => import('../views/material/MaterialList.vue'),
          meta: { title: '物料管理' }
        },
        {
          path: 'bom',
          name: 'Bom',
          component: () => import('../views/bom/BomList.vue'),
          meta: { title: 'BOM管理' }
        },
        {
          path: 'routing',
          name: 'Routing',
          component: () => import('../views/routing/RoutingList.vue'),
          meta: { title: '工艺路线' }
        },
        {
          path: 'equipment',
          name: 'Equipment',
          component: () => import('../views/equipment/EquipmentList.vue'),
          meta: { title: '设备管理' }
        },
        {
          path: 'equipment/maintenance',
          name: 'EquipmentMaintenance',
          component: () => import('../views/equipment/MaintenancePlanList.vue'),
          meta: { title: '设备保养', permissions: ['equipment:maintain'] }
        },
        {
          path: 'work-order',
          name: 'WorkOrder',
          component: () => import('../views/work-order/WorkOrderList.vue'),
          meta: { title: '工单管理' }
        },
        {
          path: 'work-order/:id',
          name: 'WorkOrderDetail',
          component: () => import('../views/work-order/WorkOrderDetail.vue'),
          meta: { title: '工单详情' }
        },
        {
          path: 'andon',
          name: 'Andon',
          component: () => import('../views/andon/AndonBoard.vue'),
          meta: { title: '异常看板' }
        },
        {
          path: 'work-report',
          name: 'WorkReport',
          component: () => import('../views/work-report/WorkReportList.vue'),
          meta: { title: '报工管理' }
        },
        {
          path: 'qc',
          name: 'Qc',
          component: () => import('../views/qc/QcInspectionList.vue'),
          meta: { title: '质量管理' }
        },
        {
          path: 'qc-dashboard',
          name: 'QcDashboard',
          component: () => import('../views/qc/QcDashboard.vue'),
          meta: { title: '质检看板' }
        },
        {
          path: 'trace',
          name: 'Trace',
          component: () => import('../views/trace/TraceView.vue'),
          meta: { title: '追溯查询' }
        },
        {
          path: 'scheduling',
          name: 'Scheduling',
          component: () => import('../views/scheduling/SchedulingView.vue'),
          meta: { title: '排产管理' }
        },
        {
          path: 'scheduling/gantt',
          name: 'SchedulingGantt',
          component: () => import('../views/scheduling/GanttView.vue'),
          meta: { title: '甘特图排产' }
        },
        {
          path: 'line-task-board',
          name: 'LineTaskBoard',
          component: () => import('../views/scheduling/LineTaskBoard.vue'),
          meta: { title: '产线任务看板' }
        },
        {
          path: 'pda-report',
          name: 'PdaReport',
          component: () => import('../views/work-report/PdaReportView.vue'),
          meta: { title: 'PDA扫码报工', displayInMenu: false }
        },
        {
          path: 'pda-offline-queue',
          name: 'PdaOfflineQueue',
          component: () => import('../views/work-report/PdaOfflineQueue.vue'),
          meta: { title: '离线队列', displayInMenu: false }
        },
        {
          path: 'qc-checkpoint',
          name: 'QcCheckpoint',
          component: () => import('../views/qc/QcCheckpointList.vue'),
          meta: { title: '质检检查点' }
        },
        {
          path: 'system/seed',
          name: 'SeedData',
          component: () => import('../views/system/SeedData.vue'),
          meta: { title: '种子数据' }
        },
        {
          path: 'system/users',
          name: 'UserManagement',
          component: () => import('../views/system/UserManagement.vue'),
          meta: { title: '用户管理', roles: ['Admin'] }
        },
        {
          path: 'system/roles',
          name: 'RoleManagement',
          component: () => import('../views/system/RoleManagement.vue'),
          meta: { title: '角色管理', roles: ['Admin'] }
        },
        {
          path: 'integration',
          redirect: '/integration/dashboard',
          children: [
            {
              path: 'dashboard',
              name: 'IntegrationDashboard',
              component: () => import('../views/integration/IntegrationDashboard.vue'),
              meta: { title: '系统集成', icon: 'Link' }
            },
            {
              path: 'logs',
              name: 'IntegrationLogs',
              component: () => import('../views/integration/IntegrationLogs.vue'),
              meta: { title: '同步日志', icon: 'Document' }
            }
          ]
        },
        {
          path: 'ai',
          redirect: '/ai/dashboard',
          children: [
            {
              path: 'dashboard',
              name: 'AiDashboard',
              component: () => import('../views/ai/AiDashboard.vue'),
              meta: { title: 'AI智能分析', icon: 'Cpu' }
            },
            {
              path: 'knowledge',
              name: 'KnowledgeSearch',
              component: () => import('../views/ai/KnowledgeSearch.vue'),
              meta: { title: '知识库查询' }
            },
            {
              path: 'knowledge/manage',
              name: 'KnowledgeManage',
              component: () => import('../views/ai/KnowledgeBase.vue'),
              meta: { title: '知识库管理' }
            }
          ]
        }
      ]
    },
    {
      path: '/:pathMatch(.*)*',
      name: 'NotFound',
      component: () => import('../views/login/LoginView.vue'),
      meta: { requiresAuth: false, title: '页面不存在' }
    }
  ]
})

// 路由守卫
router.beforeEach((to, _from, next) => {
  const token = localStorage.getItem('mes_token')
  if (to.meta.requiresAuth !== false && !token) {
    next('/login')
    return
  }

  // 页面级权限检查
  if (to.meta.roles || to.meta.permissions) {
    const roles = JSON.parse(localStorage.getItem('mes_roles') || '[]') as string[]
    const permissions = JSON.parse(localStorage.getItem('mes_permissions') || '[]') as string[]

    const hasRole = to.meta.roles?.some((r: string) => roles.includes(r))
    const hasPermission = to.meta.permissions?.some((p: string) => permissions.includes(p))

    if (!hasRole && !hasPermission) {
      next('/dashboard')
      return
    }
  }

  if (to.path === '/login' && token) {
    next('/dashboard')
  } else {
    next()
  }
})

export default router
