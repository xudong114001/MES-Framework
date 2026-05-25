import http from './index'

export interface WorkReport {
  id?: number
  reportNo?: string
  workOrderId: number
  stepId?: number
  workstationId?: number
  operatorId?: string
  reportType: string
  goodQty: number
  scrapQty: number
  reworkQty: number
  durationMin?: number
  reportTime?: string
  remark?: string
}

export const workReportApi = {
  list() { return http.get('/work-reports') },
  getById(id: number) { return http.get(`/work-reports/${id}`) },
  create(data: WorkReport) { return http.post('/work-reports', data) },
  update(id: number, data: WorkReport) { return http.put(`/work-reports/${id}`, data) },
  pdaScan(data: PdaScanReportRequest) { return http.post('/work-reports/pda-scan', data) }
}

// PDA 扫码报工请求参数
export interface PdaScanReportRequest {
  scanCode: string
  stepName: string
  workstationCode: string
  operatorCode: string
  goodQty: number
  scrapQty: number
  reworkQty: number
}
