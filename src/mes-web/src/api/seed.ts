import http from './index'

export const seedApi = {
  initSeed() { return http.post('/seed') }
}
