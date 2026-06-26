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

export interface RoutingStep {
  id?: number
  routingId?: number
  stepNo: number
  stepName: string
  workstationType?: string
  standardTime: number
  isQcPoint?: boolean
  isScrapPoint?: boolean
}

export const routingApi = {
  list() { return http.get('/routings') },
  getById(id: number) { return http.get(`/routings/${id}`) },
  create(data: Routing) { return http.post('/routings', data) },
  update(id: number, data: Routing) { return http.put(`/routings/${id}`, data) },
  delete(id: number) { return http.delete(`/routings/${id}`) },
  listByMaterial(materialId: number) { return http.get(`/routings/by-material/${materialId}`) },
  getSteps(id: number) { return http.get(`/routings/${id}/steps`) },
  addStep(routingId: number, data: RoutingStep) { return http.post(`/routings/${routingId}/steps`, data) },
  updateStep(routingId: number, stepId: number, data: RoutingStep) { return http.put(`/routings/${routingId}/steps/${stepId}`, data) },
  deleteStep(routingId: number, stepId: number) { return http.delete(`/routings/${routingId}/steps/${stepId}`) }
}
