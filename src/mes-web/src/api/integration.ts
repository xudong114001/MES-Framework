import http from './index'

export interface Adapter {
  name: string
  displayName: string
  connected: boolean
  lastSyncTime?: string
}

export interface IntegrationEvent {
  id?: number
  createdAt?: string
  eventType: string
  direction: string
  status: string
  message?: string
}

export const integrationApi = {
  getAdapters() { return http.get('/integration/adapters') },
  testAdapter(name: string) { return http.post(`/integration/adapters/${name}/test`) },
  syncAdapter(name: string, direction?: string) {
    return http.post(`/integration/adapters/${name}/sync`, { direction })
  },
  getLogs(params?: any) {
    return http.get('/integration/events', { params })
  }
}
