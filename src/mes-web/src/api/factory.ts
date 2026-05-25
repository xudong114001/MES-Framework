import http from './index'

export interface Factory {
  id?: number
  code: string
  name: string
  address?: string
  status?: number
}

export const factoryApi = {
  list() { return http.get('/factories') },
  getById(id: number) { return http.get(`/factories/${id}`) },
  create(data: Factory) { return http.post('/factories', data) },
  update(id: number, data: Factory) { return http.put(`/factories/${id}`, data) },
  delete(id: number) { return http.delete(`/factories/${id}`) }
}
