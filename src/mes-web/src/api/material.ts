import http from './index'

export interface Material {
  id?: number
  code: string
  name: string
  spec?: string
  unit?: string
  category?: string
  bomLevel?: number
  status?: number
}

export const materialApi = {
  list() { return http.get('/materials') },
  getById(id: number) { return http.get(`/materials/${id}`) },
  create(data: Material) { return http.post('/materials', data) },
  update(id: number, data: Material) { return http.put(`/materials/${id}`, data) },
  delete(id: number) { return http.delete(`/materials/${id}`) }
}
