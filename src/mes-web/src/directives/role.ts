import type { Directive, DirectiveBinding } from 'vue'
import { useAuthStore } from '../stores/auth'

/**
 * v-role 指令 — 基于角色控制元素显示/隐藏（大小写不敏感）
 *
 * 用法：
 *   <el-button v-role="'Admin'">删除</el-button>
 *   <el-button v-role="['Admin', 'ProductionManager']">管理</el-button>
 *
 * 逻辑：用户拥有任一指定角色即显示（OR 逻辑），否则隐藏（display: none）
 * 未登录用户默认隐藏。角色名比较大小写不敏感。
 */
export const vRole: Directive<HTMLElement, string | string[]> = {
  mounted(el: HTMLElement, binding: DirectiveBinding<string | string[]>) {
    updateVisibility(el, binding, 'role')
  },
  updated(el: HTMLElement, binding: DirectiveBinding<string | string[]>) {
    updateVisibility(el, binding, 'role')
  },
}

/**
 * v-permission 指令 — 基于细粒度权限控制元素显示/隐藏（大小写不敏感）
 *
 * 用法：
 *   <el-button v-permission="'user:delete'">删除用户</el-button>
 *   <el-button v-permission="['user:edit', 'user:delete']">编辑/删除</el-button>
 *
 * 逻辑：用户拥有任一指定权限即显示（OR 逻辑），否则隐藏（display: none）
 * 未登录用户默认隐藏
 */
export const vPermission: Directive<HTMLElement, string | string[]> = {
  mounted(el: HTMLElement, binding: DirectiveBinding<string | string[]>) {
    updateVisibility(el, binding, 'permission')
  },
  updated(el: HTMLElement, binding: DirectiveBinding<string | string[]>) {
    updateVisibility(el, binding, 'permission')
  },
}

/**
 * 统一更新元素可见性（角色/权限名大小写不敏感比较）
 */
function updateVisibility(
  el: HTMLElement,
  binding: DirectiveBinding<string | string[]>,
  mode: 'role' | 'permission'
) {
  const authStore = useAuthStore()

  // 未登录用户默认隐藏
  if (!authStore.token) {
    el.style.display = 'none'
    return
  }

  const required = normalizeBindingValue(binding.value)

  // 无绑定值时默认显示
  if (required.length === 0) {
    el.style.display = ''
    return
  }

  // Admin 角色始终拥有所有权限，直接放行（大小写不敏感）
  if (authStore.roles.some(r => r.toLowerCase() === 'admin')) {
    el.style.display = ''
    return
  }

  const userValues = mode === 'role' ? authStore.roles : authStore.permissions
  // 大小写不敏感比较
  const userValuesLower = userValues.map(v => v.toLowerCase())
  const hasAccess = required.some((item) => userValuesLower.includes(item.toLowerCase()))

  el.style.display = hasAccess ? '' : 'none'
}

/**
 * 将绑定值规范化为字符串数组
 * 支持字符串、字符串数组
 */
function normalizeBindingValue(value: string | string[]): string[] {
  if (Array.isArray(value)) {
    return value.filter((v) => typeof v === 'string' && v.length > 0)
  }
  if (typeof value === 'string' && value.length > 0) {
    return [value]
  }
  return []
}
