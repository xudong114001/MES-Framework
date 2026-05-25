import http from './index'

export const dashboardApi = {
  todayOrders() { return http.get('/dashboard/orders/today') },
  orderStatus() { return http.get('/dashboard/orders/status') },
  output() { return http.get('/dashboard/output') },
  equipment() { return http.get('/dashboard/equipment') }
}
