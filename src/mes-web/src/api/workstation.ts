import http from './index'

export interface Workstation {
  id?: number
  code: string
  name: string
  lineId?: number
  lineName?: string
  sortOrder?: number
  status?: number
}

export const workstationApi = {
  list() { return http.get('/workstations') },
  getById(id: number) { return http.get(`/workstations/${id}`) },
  create(data: Workstation) { return http.post('/workstations', data) },
  update(id: number, data: Workstation) { return http.put(`/workstations/${id}`, data) },
  delete(id: number) { return http.delete(`/workstations/${id}`) },
  listByLine(lineId: number) { return http.get(`/workstations/byLine/${lineId}`) }
}
