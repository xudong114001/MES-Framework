import http from './index'

export interface User {
  id?: number
  username: string
  displayName?: string
  email?: string
  isActive?: boolean
}

export const userApi = {
  list() { return http.get('/users') },
  getById(id: number) { return http.get(`/users/${id}`) },
  create(data: User) { return http.post('/users', data) },
  update(id: number, data: User) { return http.put(`/users/${id}`, data) },
  delete(id: number) { return http.delete(`/users/${id}`) }
}
