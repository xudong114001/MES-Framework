import http from './index'

export interface Workshop {
  id?: number
  code: string
  name: string
  factoryId?: number
  factoryName?: string
  address?: string
  status?: number
}

export const workshopApi = {
  list() { return http.get('/workshops') },
  getById(id: number) { return http.get(`/workshops/${id}`) },
  create(data: Workshop) { return http.post('/workshops', data) },
  update(id: number, data: Workshop) { return http.put(`/workshops/${id}`, data) },
  delete(id: number) { return http.delete(`/workshops/${id}`) },
  listByFactory(factoryId: number) { return http.get(`/workshops/byFactory/${factoryId}`) }
}
