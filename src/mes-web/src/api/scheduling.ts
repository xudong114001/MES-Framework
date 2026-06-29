import http from './index'

export interface ScheduleItem {
  workOrderId: number
  orderNo: string
  materialName: string
  plannedQty: number
  completedQty: number
  status: number
  lineId: number | null
  lineName: string | null
  priority: number
  planStartTime: string | null
  planEndTime: string | null
}

export interface DispatchTask {
  workOrderStepId: number
  workOrderId: number
  orderNo: string
  stepNo: number
  stepName: string
  workstationId: number | null
  workstationName: string | null
  status: number
}

export const schedulingApi = {
  unscheduledOrders() { return http.get('/scheduling/unscheduled-orders') },
  schedule(workOrderId: number, lineId: number) { return http.post('/scheduling/schedule', { workOrderId, lineId }) },
  batchSchedule(workOrderIds: number[], lineId: number) { return http.post('/scheduling/batch-schedule', { workOrderIds, lineId }) },
  autoSchedule() { return http.post('/scheduling/auto-schedule') },
  lineScheduledOrders(lineId: number) { return http.get(`/scheduling/line/${lineId}/scheduled-orders`) },
  unschedule(workOrderId: number) { return http.post(`/scheduling/unschedule/${workOrderId}`) },
  swapOrder(orderId1: number, orderId2: number) { return http.post('/scheduling/swap-order', { orderId1, orderId2 }) },
  productionLines() { return http.get('/scheduling/production-lines') }
}

export const dispatchApi = {
  dispatch(workOrderStepId: number, workstationId: number) { return http.post('/dispatch/dispatch-step', { workOrderStepId, workstationId }) },
  undispatch(workOrderStepId: number) { return http.post(`/dispatch/undispatch-step/${workOrderStepId}`) },
  lineTasks(lineId: number) { return http.get(`/dispatch/line/${lineId}/today-tasks`) },
  availableWorkstations(lineId: number) { return http.get(`/dispatch/line/${lineId}/workstations`) }
}
