import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '30s', target: 30 },
    { duration: '1m', target: 100 },
    { duration: '2m', target: 100 },
    { duration: '30s', target: 0 }
  ],
  thresholds: { http_req_duration: ['p(95)<300'], http_req_failed: ['rate<0.001'] }
};

const BASE_URL = 'http://localhost:5180';

export default function() {
  const payload = JSON.stringify({
    workOrderId: 1,
    goodQty: Math.floor(Math.random() * 10),
    scrapQty: 0,
    reworkQty: 0,
    reportType: 0
  });
  
  const res = http.post(`${BASE_URL}/api/v1/work-reports`, payload, {
    headers: { 'Content-Type': 'application/json' }
  });
  
  check(res, { 'report submitted': (r) => [200, 409].includes(r.status) });
  sleep(0.1);
}
