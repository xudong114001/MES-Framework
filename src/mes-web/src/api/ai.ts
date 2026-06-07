import http from './index'

export const aiApi = {
  // Quality Alerts
  getActiveAlerts() {
    return http.get('/ai/quality/alerts')
  },
  analyzeQuality(workOrderId?: number) {
    return http.post('/ai/quality/analyze', { workOrderId })
  },
  processAlert(id: number, processedBy: string) {
    return http.post(`/ai/quality/alerts/${id}/process`, { processedBy })
  },
  getAlertHistory(page = 1, pageSize = 20) {
    return http.get('/ai/quality/history', { params: { page, page_size: pageSize } })
  },

  // Scheduling
  getSchedulingRecommendation(workOrderId: number) {
    return http.get(`/ai/scheduling/recommend/${workOrderId}`)
  },

  // Equipment Health
  getAllEquipmentHealth() {
    return http.get('/ai/equipment/health')
  },
  getEquipmentHealth(equipmentId: number) {
    return http.get(`/ai/equipment/health/${equipmentId}`)
  },
  getHighRiskEquipment() {
    return http.get('/ai/equipment/high-risk')
  }
}
