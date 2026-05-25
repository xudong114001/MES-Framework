import http from './index'

export interface Equipment {
  id?: number
  code: string
  name: string
  model?: string
  factoryId?: number
  factoryName?: string
  workshopId?: number
  workshopName?: string
  lineId?: number
  lineName?: string
  installDate?: string
  status?: string
  theoreticalCycleTime?: number
  plannedRunTime?: number
  lastMaintainDate?: string
  nextMaintainDate?: string
  maintainCycle?: number
}

export const equipmentApi = {
  list() { return http.get('/equipment') },
  getById(id: number) { return http.get(`/equipment/${id}`) },
  create(data: Equipment) { return http.post('/equipment', data) },
  update(id: number, data: Equipment) { return http.put(`/equipment/${id}`, data) },
  delete(id: number) { return http.delete(`/equipment/${id}`) }
}
