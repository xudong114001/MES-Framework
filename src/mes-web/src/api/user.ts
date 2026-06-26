import http from './index'

export interface User {
  id?: number
  username: string
  displayName?: string
  email?: string
  isActive?: boolean
}

export const userApi = {
  list() { return http.get('/user') },
  getById(id: number) { return http.get(`/user/${id}`) },
  create(data: any) { return http.post('/user', data) },
  update(id: number, data: any) { return http.put(`/user/${id}`, data) },
  delete(id: number) { return http.delete(`/user/${id}`) },
  resetPassword(id: number, newPassword: string) { return http.post(`/user/${id}/reset-password`, { newPassword }) },
  assignRoles(id: number, roles: string[]) { return http.put(`/user/${id}/roles`, { roles }) }
}
