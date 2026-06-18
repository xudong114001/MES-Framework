import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authApi, type UserInfo } from '../api/auth'

export const useAuthStore = defineStore('auth', () => {
  const user = ref<UserInfo | null>(JSON.parse(localStorage.getItem('mes_user') || 'null'))
  const token = ref(localStorage.getItem('mes_token') || '')
  const roles = ref<string[]>(JSON.parse(localStorage.getItem('mes_roles') || '[]'))

  async function login(username: string, password: string) {
    const res = await authApi.login({ username, password })
    const data = res.data as { token: string; expiresAt: string; userInfo: UserInfo }
    token.value = data.token
    user.value = data.userInfo
    roles.value = data.userInfo.roles || []
    localStorage.setItem('mes_token', data.token)
    localStorage.setItem('mes_user', JSON.stringify(data.userInfo))
    localStorage.setItem('mes_roles', JSON.stringify(roles.value))
    return data
  }

  function logout() {
    token.value = ''
    user.value = null
    roles.value = []
    localStorage.removeItem('mes_token')
    localStorage.removeItem('mes_user')
    localStorage.removeItem('mes_roles')
  }

  const hasRole = computed(() => (role: string) => {
    return roles.value.includes(role)
  })

  return { user, token, roles, login, logout, hasRole }
})
