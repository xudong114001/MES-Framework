import http from './index'

export interface KnowledgeEntry {
  id?: number
  category: number
  title: string
  content: string
  keywords?: string
  materialId?: number
  equipmentId?: number
}

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
    return http.get('/ai/quality/history', { params: { page, pageSize } })
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

export const knowledgeBaseApi = {
  search(params: { q?: string; category?: number; page?: number; pageSize?: number }) {
    return http.get('/ai/knowledge/search', { params })
  },
  list(params: { category?: number; page?: number; pageSize?: number }) {
    return http.get('/ai/knowledge/entries', { params })
  },
  add(data: KnowledgeEntry) { return http.post('/ai/knowledge/entries', data) },
  update(id: number, data: KnowledgeEntry) { return http.put(`/ai/knowledge/entries/${id}`, data) },
  delete(id: number) { return http.delete(`/ai/knowledge/entries/${id}`) }
}
