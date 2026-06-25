import http from './index'

export interface Routing {
  id?: number
  materialId: number
  materialName?: string
  stepOrder?: number
  stepName?: string
  workstationId?: number
  workstationName?: string
  durationMinutes?: number
  status?: number
}

export const routingApi = {
  list() { return http.get('/routings') },
  getById(id: number) { return http.get(`/routings/${id}`) },
  create(data: Routing) { return http.post('/routings', data) },
  update(id: number, data: Routing) { return http.put(`/routings/${id}`, data) },
  delete(id: number) { return http.delete(`/routings/${id}`) },
  listByMaterial(materialId: number) { return http.get(`/routings/by-material/${materialId}`) },
  getSteps(id: number) { return http.get(`/routings/${id}/steps`) }
}
