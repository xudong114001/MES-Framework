import { defineStore } from 'pinia'
import { ref } from 'vue'
import { authApi, type UserInfo } from '../api/auth'

export const useAuthStore = defineStore('auth', () => {
  const user = ref<UserInfo | null>(JSON.parse(localStorage.getItem('mes_user') || 'null'))
  const token = ref(localStorage.getItem('mes_token') || '')

  async function login(username: string, password: string) {
    const res = await authApi.login({ username, password })
    const data = res.data as { token: string; expiresAt: string; userInfo: UserInfo }
    token.value = data.token
    user.value = data.userInfo
    localStorage.setItem('mes_token', data.token)
    localStorage.setItem('mes_user', JSON.stringify(data.userInfo))
    return data
  }

  function logout() {
    token.value = ''
    user.value = null
    localStorage.removeItem('mes_token')
    localStorage.removeItem('mes_user')
  }

  return { user, token, login, logout }
})
