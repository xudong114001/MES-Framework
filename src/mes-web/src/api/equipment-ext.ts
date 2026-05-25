import http from './index'

export const equipmentExtApi = {
  maintain(equipmentId: number) { return http.post(`/equipment/${equipmentId}/maintain`) },
  fault(equipmentId: number) { return http.post(`/equipment/${equipmentId}/fault`) },
  oee(equipmentId: number) { return http.get(`/equipment/${equipmentId}/oee`) },
  // 保养计划
  createMaintenancePlan(equipmentId: number, data: { planName: string; cycleDays: number; description?: string }) {
    return http.post(`/equipment/${equipmentId}/maintenance-plan`, data)
  },
  getMaintenancePlans(equipmentId: number) {
    return http.get(`/equipment/${equipmentId}/maintenance-plans`)
  },
  completeMaintenance(equipmentId: number, planId: number) {
    return http.post(`/equipment/${equipmentId}/maintenance-plan/${planId}/complete`)
  }
}
