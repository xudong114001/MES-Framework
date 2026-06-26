import http from './index'

export interface TriggerAndonRequest {
  eventType: number
  level?: number
  title?: string
  description?: string
  workstationId?: number
  workstationName?: string
  workOrderId?: number
  workOrderNo?: string
}

export const andonApi = {
  active() { return http.get('/andon/active') },
  all() { return http.get('/andon/events') },
  getById(id: number) { return http.get(`/andon/${id}`) },
  list(params?: { page?: number; pageSize?: number; isResolved?: boolean; eventType?: number }) { return http.get('/andon', { params }) },
  getActiveCount() { return http.get('/andon/count') },
  getActiveCountByType() { return http.get('/andon/count-by-type') },
  trigger(data: TriggerAndonRequest) { return http.post('/andon/trigger', data) },
  resolve(id: number, handler?: string) { return http.post(`/andon/${id}/resolve`, { handler }) },
  deleteEvent(id: number) { return http.delete(`/andon/${id}`) }
}
