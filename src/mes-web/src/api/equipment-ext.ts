import http from './index'

export interface OeeData {
  equipmentId: number
  equipmentName: string
  status: number
  oeeValue: number
  availability: number
  performance: number
  quality: number
  goodQty: number
  badQty: number
  actualRunMinutes: number
  plannedRunMinutes: number
  lastMaintainTime: string | null
  nextMaintainTime: string | null
  maintainCycleDays: number | null
  theoreticalCycleTime: number | null
  plannedRunTime: number | null
}

export interface MaintenancePlan {
  id?: number
  equipmentId?: number
  equipmentName?: string
  planName: string
  cycleDays: number
  lastCompletedDate?: string
  nextDueDate?: string
  status?: string
  description?: string
  createdAt?: string
}

export const equipmentExtApi = {
  maintain(equipmentId: number) { return http.post(`/equipment/${equipmentId}/maintain`) },
  fault(equipmentId: number) { return http.post(`/equipment/${equipmentId}/fault`) },
  oee(equipmentId: number) { return http.get(`/equipment/${equipmentId}/oee`) },
  oeeBatch() { return http.get('/equipment/oee-batch') },
  // 保养计划 - 针对单个设备
  createMaintenancePlan(equipmentId: number, data: { planName: string; cycleDays: number; description?: string }) {
    return http.post(`/equipment/${equipmentId}/maintenance-plan`, data)
  },
  getMaintenancePlans(equipmentId: number) {
    return http.get(`/equipment/${equipmentId}/maintenance-plans`)
  },
  completeMaintenance(equipmentId: number, planId: number) {
    return http.post(`/equipment/${equipmentId}/maintenance-plan/${planId}/complete`)
  },
  // 保养计划 - 全局管理
  getAllMaintenancePlans(params?: { equipmentName?: string; status?: string }) {
    return http.get('/equipment/maintenance-plans', { params })
  },
  updateMaintenancePlan(id: number, data: { planName: string; cycleDays: number; description?: string }) {
    return http.put(`/equipment/maintenance-plans/${id}`, data)
  },
  deleteMaintenancePlan(id: number) {
    return http.delete(`/equipment/maintenance-plans/${id}`)
  },
  // 设备列表
  getEquipmentList() {
    return http.get('/equipment/list')
  }
}
