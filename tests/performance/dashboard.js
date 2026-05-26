/**
 * MES Framework — 看板数据查询性能测试
 *
 * 测试场景：看板各类统计接口并发查询
 * 配置：5 VU，持续 30 秒
 * 阈值：P95 < 500ms（所有看板接口）
 */
import { check, sleep } from 'k6';
import http from 'k6/http';
import { Rate, Trend } from 'k6/metrics';

// ─── 自定义指标 ───────────────────────────────────────────
const errorRate = new Rate('errors');
const dashboardOrderTodayDuration = new Trend('dashboard_order_today_duration', true);
const dashboardOrderStatusDuration = new Trend('dashboard_order_status_duration', true);
const dashboardOutputDuration = new Trend('dashboard_output_duration', true);
const dashboardEquipmentDuration = new Trend('dashboard_equipment_duration', true);

// ─── 配置 ─────────────────────────────────────────────────
const BASE_URL = __ENV.BASE_URL || 'http://localhost:5180';

export const options = {
  vus: 5,
  duration: '30s',
  thresholds: {
    http_req_duration: ['p(95)<500'],
    errors: ['rate<0.01'],
    dashboard_order_today_duration: ['p(95)<500'],
    dashboard_order_status_duration: ['p(95)<500'],
    dashboard_output_duration: ['p(95)<500'],
    dashboard_equipment_duration: ['p(95)<500'],
  },
  summaryTrendStats: ['avg', 'min', 'med', 'p(90)', 'p(95)', 'p(99)', 'max'],
};

// ─── 看板 API 端点 ─────────────────────────────────────────
const DASHBOARD_ENDPOINTS = [
  {
    name: 'orders/today',
    url: '/api/v1/dashboard/orders/today',
    metric: dashboardOrderTodayDuration,
    checkName: 'dashboard orders/today status 200',
  },
  {
    name: 'orders/status',
    url: '/api/v1/dashboard/orders/status',
    metric: dashboardOrderStatusDuration,
    checkName: 'dashboard orders/status status 200',
  },
  {
    name: 'output',
    url: '/api/v1/dashboard/output',
    metric: dashboardOutputDuration,
    checkName: 'dashboard output status 200',
  },
  {
    name: 'equipment',
    url: '/api/v1/dashboard/equipment',
    metric: dashboardEquipmentDuration,
    checkName: 'dashboard equipment status 200',
  },
];

// ─── setup：登录获取 Token ─────────────────────────────────
export function setup() {
  const res = http.post(
    `${BASE_URL}/api/v1/auth/login`,
    JSON.stringify({ username: 'admin', password: 'Admin@2026!' }),
    { headers: { 'Content-Type': 'application/json' } },
  );

  if (res.status !== 200) {
    throw new Error(`Setup login failed: HTTP ${res.status}`);
  }

  const body = JSON.parse(res.body);
  if (body.code !== 0 || !body.data?.token) {
    throw new Error('Setup login failed: no token returned');
  }

  return { token: body.data.token };
}

// ─── 主测试流程 ────────────────────────────────────────────
export default function (data) {
  const { token } = data;
  const params = {
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
    },
  };

  // 依次调用所有看板端点，模拟前端同时拉取看板数据
  for (const endpoint of DASHBOARD_ENDPOINTS) {
    const res = http.get(`${BASE_URL}${endpoint.url}`, params);

    const ok = check(res, {
      [endpoint.checkName]: (r) => r.status === 200,
      [`${endpoint.name} has data`]: (r) => {
        try {
          const body = JSON.parse(r.body);
          return body.code === 0;
        } catch {
          return false;
        }
      },
    });

    endpoint.metric.add(res.timings.duration);
    errorRate.add(!ok);
  }

  // 模拟看板自动刷新间隔（前端通常 5~10 秒轮询）
  sleep(3);
}
