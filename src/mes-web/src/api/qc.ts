import http from './index'

export interface QcInspection {
  id?: number
  inspectNo?: string
  sourceType: string
  sourceRef?: string
  workOrderId?: number
  materialId?: number
  inspector?: string
  inspectResult?: string
  inspectTime?: string
  remark?: string
  handlingAction?: string
  handlingRemark?: string
  handledAt?: string
  items?: QcInspectionItem[]
}

export interface QcInspectionItem {
  id?: number
  inspectionId?: number
  itemName: string
  specValue?: string
  actualValue?: string
  result?: string
}

export interface QcDashboardStats {
  total: number
  passed: number
  failed: number
  pending: number
}

export const qcApi = {
  list() { return http.get('/qc/inspections') },
  getById(id: number) { return http.get(`/qc/inspections/${id}`) },
  create(data: QcInspection) { return http.post('/qc/inspections', data) },
  addItem(inspectionId: number, data: QcInspectionItem) { return http.post(`/qc/inspections/${inspectionId}/items`, data) },
  verify(inspectionId: number, result: string) { return http.post(`/qc/inspections/${inspectionId}/verify`, { result }) },
  handleNonconforming(inspectionId: number, action: string, remark?: string) { return http.post(`/qc/inspections/${inspectionId}/handle-nonconforming`, { action, remark }) },
  dashboardStats() { return http.get('/qc/dashboard/stats') },
  dashboardPending() { return http.get('/qc/dashboard/pending') },
  recentFailed() { return http.get('/qc/dashboard/recent-failed') }
}
