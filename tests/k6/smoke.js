import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  vus: 10,
  duration: '30s',
  thresholds: { http_req_duration: ['p(95)<500'] }
};

export default function() {
  const res = http.get('http://localhost:5180/api/v1/factories');
  check(res, { 'status 200': (r) => r.status === 200 });
  sleep(1);
}
