import http from './index'

export const andonApi = {
  active() { return http.get('/andon/active') },
  all() { return http.get('/andon/events') },
  trigger(data: { eventType: string; workstation?: string; description?: string }) { return http.post('/andon/trigger', data) },
  resolve(id: number, handler?: string) { return http.post(`/andon/${id}/resolve`, { handler }) }
}
