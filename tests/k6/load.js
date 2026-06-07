import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '2m', target: 50 },
    { duration: '5m', target: 100 },
    { duration: '3m', target: 0 }
  ],
  thresholds: { http_req_duration: ['p(95)<1000'], http_req_failed: ['rate<0.01'] }
};

const BASE_URL = 'http://localhost:5180';

export default function() {
  const rand = Math.random();
  
  if (rand < 0.7) {
    // 70% - Work order queries
    check(http.get(`${BASE_URL}/api/v1/work-orders?page=1&page_size=20`), {
      'WO list': (r) => r.status === 200
    });
  } else if (rand < 0.9) {
    // 20% - Reporting
    check(http.get(`${BASE_URL}/api/v1/dashboard/output`), {
      'Dashboard': (r) => r.status === 200
    });
  } else {
    // 10% - Equipment queries
    check(http.get(`${BASE_URL}/api/v1/equipment`), {
      'Equipment': (r) => r.status === 200
    });
  }
  sleep(0.5);
}
