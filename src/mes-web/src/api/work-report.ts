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
  submit(data: WorkReport) { return http.post('/work-reports/submit', data) },
  pdaScan(data: PdaScanRequest) { return http.post('/work-reports/pda-scan', data) },
  delete(id: number) { return http.delete(`/work-reports/${id}`) }
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
