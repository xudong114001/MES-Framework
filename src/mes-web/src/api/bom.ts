import http from './index'

export interface Bom {
  id?: number
  productId: number
  productName?: string
  materialId: number
  materialName?: string
  materialCode?: string
  quantity: number
  scrapRate?: number
  status?: number
}

export const bomApi = {
  list() { return http.get('/boms') },
  getById(id: number) { return http.get(`/boms/${id}`) },
  create(data: Bom) { return http.post('/boms', data) },
  update(id: number, data: Bom) { return http.put(`/boms/${id}`, data) },
  delete(id: number) { return http.delete(`/boms/${id}`) },
  listByProduct(productId: number) { return http.get(`/boms/byProduct/${productId}`) }
}
