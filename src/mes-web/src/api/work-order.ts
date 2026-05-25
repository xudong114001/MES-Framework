import http from './index'

export interface WorkOrder {
  id?: number
  orderNo: string
  sourceType?: string
  sourceRef?: string
  materialId: number
  routingId?: number
  plannedQty: number
  completedQty?: number
  scrapQty?: number
  status?: string
  planStartTime?: string
  planEndTime?: string
  priority?: number
  factoryId?: number
  workshopId?: number
  lineId?: number
  assignee?: string
  remark?: string
  materialName?: string
  steps?: WorkOrderStep[]
}

export type WorkOrderStep = {

  id?: number
  workOrderId?: number
  stepNo: number
  stepName: string
  workstationId?: number
  plannedQty: number
  completedQty?: number
  scrapQty?: number
  status?: string
}

export const workOrderApi = {
  list() { return http.get('/work-orders') },
  getById(id: number) { return http.get(`/work-orders/${id}`) },
  create(data: WorkOrder) { return http.post('/work-orders', data) },
  update(id: number, data: WorkOrder) { return http.put(`/work-orders/${id}`, data) },
  delete(id: number) { return http.delete(`/work-orders/${id}`) },
  release(id: number) { return http.post(`/work-orders/${id}/release`) },
  hold(id: number) { return http.post(`/work-orders/${id}/hold`) },
  resume(id: number) { return http.post(`/work-orders/${id}/resume`) },
  cancel(id: number) { return http.post(`/work-orders/${id}/cancel`) },
  close(id: number) { return http.post(`/work-orders/${id}/close`) },
  split(id: number, splitQty: number) { return http.post(`/work-orders/${id}/split`, { splitQty }) },
  scrap(id: number, data: { scrapQty: number; remark?: string }) { return http.post(`/work-orders/${id}/scrap`, data) },
  rework(id: number, data: { reworkQty: number; remark?: string }) { return http.post(`/work-orders/${id}/rework`, data) }
}
