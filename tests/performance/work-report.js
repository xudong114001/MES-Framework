/**
 * MES Framework — 报工接口性能测试
 *
 * 测试场景：集中压测 PDA 扫码报工 & 报工列表查询
 * 配置：20 VU，持续 1 分钟，目标 100 TPS
 * 阈值：P95 < 1000ms
 */
import { check, sleep } from 'k6';
import http from 'k6/http';
import { Rate, Trend, Counter } from 'k6/metrics';

// ─── 自定义指标 ───────────────────────────────────────────
const errorRate = new Rate('errors');
const reportSubmitDuration = new Trend('report_submit_duration', true);
const reportListDuration = new Trend('report_list_duration', true);
const reportDetailDuration = new Trend('report_detail_duration', true);
const reportTps = new Counter('report_tps');

// ─── 配置 ─────────────────────────────────────────────────
const BASE_URL = __ENV.BASE_URL || 'http://localhost:5180';

export const options = {
  scenarios: {
    // 主要场景：PDA 扫码报工压测
    pda_report: {
      executor: 'constant-arrival-rate',
      rate: 70,              // 每秒 70 次请求
      timeUnit: '1s',
      duration: '1m',
      preAllocatedVUs: 10,
      maxVUs: 20,
      exec: 'pdaReport',
    },
    // 辅助场景：报工列表查询
    report_query: {
      executor: 'constant-arrival-rate',
      rate: 30,              // 每秒 30 次请求，合计 100 TPS
      timeUnit: '1s',
      duration: '1m',
      preAllocatedVUs: 5,
      maxVUs: 10,
      exec: 'reportQuery',
      startTime: '5s',       // 稍微延迟启动，避免冷启动叠加
    },
  },
  thresholds: {
    http_req_duration: ['p(95)<1000'],
    errors: ['rate<0.01'],
    report_submit_duration: ['p(95)<1000'],
    report_list_duration: ['p(95)<500'],
  },
  summaryTrendStats: ['avg', 'min', 'med', 'p(90)', 'p(95)', 'p(99)', 'max'],
};

// ─── 共享 Token（setup 阶段获取，所有 VU 复用）──────────────
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

  // 获取工单列表以获得可用工单号
  const woRes = http.get(`${BASE_URL}/api/v1/work-orders`, {
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${body.data.token}`,
    },
  });

  let workOrders = [];
  if (woRes.status === 200) {
    const woBody = JSON.parse(woRes.body);
    workOrders = woBody.data || [];
  }

  // 获取已有报工列表以测试详情查询
  const reportRes = http.get(`${BASE_URL}/api/v1/work-reports`, {
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${body.data.token}`,
    },
  });

  let reportIds = [];
  if (reportRes.status === 200) {
    const rBody = JSON.parse(reportRes.body);
    const reports = rBody.data || [];
    reportIds = reports.map((r) => r.id).slice(0, 20);
  }

  return {
    token: body.data.token,
    workOrders,
    reportIds,
  };
}

// ─── HTTP 工具 ─────────────────────────────────────────────
function authHeaders(token) {
  return {
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
    },
  };
}

// ─── 场景 1：PDA 扫码报工 ──────────────────────────────────
export function pdaReport(data) {
  const { token, workOrders } = data;

  // 选择可报工的工单（IN_PROGRESS=3 或 RELEASED=1）
  const reportable = workOrders.find((wo) => wo.status === 3 || wo.status === 1)
    || workOrders[0];

  if (!reportable) {
    errorRate.add(1);
    return;
  }

  const payload = JSON.stringify({
    scanCode: reportable.orderNo,
    stepName: '上板',
    workstationCode: 'SMT01-LOADER',
    operatorCode: 'operator',
    goodQty: 1,
    scrapQty: 0,
    reworkQty: 0,
  });

  const res = http.post(
    `${BASE_URL}/api/v1/work-reports/pda-scan`,
    payload,
    { headers: { 'Content-Type': 'application/json' } },
  );

  const ok = check(res, {
    'pda-report status 2xx or 4xx': (r) => r.status >= 200 && r.status < 500,
  });

  reportSubmitDuration.add(res.timings.duration);
  reportTps.add(1);
  errorRate.add(!ok);
}

// ─── 场景 2：报工列表 & 详情查询 ───────────────────────────
export function reportQuery(data) {
  const { token, reportIds } = data;

  // 2a: 查询报工列表
  const listRes = http.get(`${BASE_URL}/api/v1/work-reports`, authHeaders(token));

  const listOk = check(listRes, {
    'report-list status 200': (r) => r.status === 200,
  });

  reportListDuration.add(listRes.timings.duration);
  errorRate.add(!listOk);

  // 2b: 如果有报工 ID，查询详情
  if (reportIds.length > 0) {
    const randomId = reportIds[Math.floor(Math.random() * reportIds.length)];
    const detailRes = http.get(
      `${BASE_URL}/api/v1/work-reports/${randomId}`,
      authHeaders(token),
    );

    const detailOk = check(detailRes, {
      'report-detail status 200': (r) => r.status === 200,
    });

    reportDetailDuration.add(detailRes.timings.duration);
    errorRate.add(!detailOk);
  }
}

// ─── teardown ──────────────────────────────────────────────
export function teardown(data) {
  // 可在此处清理测试数据或输出汇总信息
  console.log(`Report test completed. Report IDs available: ${data.reportIds?.length || 0}`);
}
