import http from './index'

export interface ProductionLine {
  id?: number
  code: string
  name: string
  workshopId?: number
  workshopName?: string
  status?: number
}

export const productionLineApi = {
  list() { return http.get('/production-lines') },
  getById(id: number) { return http.get(`/production-lines/${id}`) },
  create(data: ProductionLine) { return http.post('/production-lines', data) },
  update(id: number, data: ProductionLine) { return http.put(`/production-lines/${id}`, data) },
  delete(id: number) { return http.delete(`/production-lines/${id}`) },
  listByWorkshop(workshopId: number) { return http.get(`/production-lines/byWorkshop/${workshopId}`) }
}
