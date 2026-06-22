import http from './index'

export interface Role {
  id?: number
  name: string
  description?: string
}

export const roleApi = {
  list() { return http.get('/role') },
  getById(id: number) { return http.get(`/role/${id}`) },
  create(data: Role) { return http.post('/role', data) },
  update(id: number, data: Role) { return http.put(`/role/${id}`, data) },
  delete(id: number) { return http.delete(`/role/${id}`) },
  getPermissions(id: number) { return http.get(`/role/${id}/permissions`) },
  assignPermissions(id: number, permissions: string[]) { return http.put(`/role/${id}/permissions`, { permissions }) }
}
