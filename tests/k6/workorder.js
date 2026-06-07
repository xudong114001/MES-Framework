import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '1m', target: 20 },
    { duration: '3m', target: 50 },
    { duration: '1m', target: 0 }
  ],
  thresholds: { http_req_duration: ['p(95)<500'], http_req_failed: ['rate<0.01'] }
};

const BASE_URL = 'http://localhost:5180';
let token = '';

export function setup() {
  const login = http.post(`${BASE_URL}/api/v1/auth/login`, 
    JSON.stringify({ username: 'admin', password: 'Admin@2026!' }),
    { headers: { 'Content-Type': 'application/json' } });
  return { token: login.json('data.token') };
}

export default function(data) {
  const headers = { 'Authorization': `Bearer ${data.token}`, 'Content-Type': 'application/json' };
  
  // List work orders
  check(http.get(`${BASE_URL}/api/v1/work-orders`, { headers }), {
    'list work orders': (r) => r.status === 200
  });
  sleep(0.5);
}
