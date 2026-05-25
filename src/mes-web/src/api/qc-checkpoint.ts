import http from './index'

export interface QcCheckpoint {
  id?: number
  stepId: number
  stepName?: string
  checkType: string
  isMandatory: boolean
  remark?: string
}

export const qcCheckpointApi = {
  list() { return http.get('/qc-checkpoints') },
  getById(id: number) { return http.get(`/qc-checkpoints/${id}`) },
  create(data: QcCheckpoint) { return http.post('/qc-checkpoints', data) },
  update(id: number, data: QcCheckpoint) { return http.put(`/qc-checkpoints/${id}`, data) },
  delete(id: number) { return http.delete(`/qc-checkpoints/${id}`) },
  listByStep(stepId: number) { return http.get(`/qc-checkpoints/by-step/${stepId}`) }
}
