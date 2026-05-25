import http from './index'

export const traceApi = {
  byBatch(batchNo: string) { return http.get(`/trace/by-batch/${batchNo}`) },
  bySerial(serialNo: string) { return http.get(`/trace/by-serial/${serialNo}`) },
  forward(materialId: number, batchNo: string) { return http.get(`/trace/forward/${materialId}/${batchNo}`) },
  backward(serialNo: string) { return http.get(`/trace/backward/${serialNo}`) }
}
