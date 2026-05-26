/**
 * MES Framework — 基础业务流程性能测试
 *
 * 测试场景：用户登录 → 获取工单列表 → 创建报工
 * 配置：10 VU，持续 30 秒
 * 阈值：P95 < 500ms，错误率 < 1%
 */
import { check, sleep } from "k6";
import http from "k6/http";
import { Rate, Trend } from "k6/metrics";

// ─── 自定义指标 ───────────────────────────────────────────
const errorRate = new Rate("errors");
const loginDuration = new Trend("login_duration", true);
const workOrderListDuration = new Trend("work_order_list_duration", true);
const workReportCreateDuration = new Trend("work_report_create_duration", true);

// ─── 配置 ─────────────────────────────────────────────────
const BASE_URL = __ENV.BASE_URL || "http://localhost:5180";

export const options = {
  vus: 10,
  duration: "30s",
  thresholds: {
    http_req_duration: ["p(95)<500"],
    errors: ["rate<0.01"],
    login_duration: ["p(95)<500"],
    work_order_list_duration: ["p(95)<500"],
    work_report_create_duration: ["p(95)<800"],
  },
  summaryTrendStats: ["avg", "min", "med", "p(90)", "p(95)", "p(99)", "max"],
};

// ─── HTTP 工具函数 ─────────────────────────────────────────
function get(url, token) {
  const params = {
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    },
  };
  return http.get(url, params);
}

function post(url, body, token) {
  const params = {
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    },
  };
  return http.post(url, body, params);
}

// ─── 登录 ─────────────────────────────────────────────────
function login(username, password) {
  const res = post(
    `${BASE_URL}/api/v1/auth/login`,
    JSON.stringify({ username, password }),
  );

  const ok = check(res, {
    "login status 200": (r) => r.status === 200,
    "login has token": (r) => {
      try {
        const body = JSON.parse(r.body);
        return body.code === 0 && body.data?.token != null;
      } catch {
        return false;
      }
    },
  });

  loginDuration.add(res.timings.duration);
  errorRate.add(!ok);

  if (!ok) {
    return null;
  }

  return JSON.parse(res.body).data.token;
}

// ─── 获取工单列表 ──────────────────────────────────────────
function getWorkOrders(token) {
  const res = get(`${BASE_URL}/api/v1/work-orders`, token);

  const ok = check(res, {
    "work-orders status 200": (r) => r.status === 200,
    "work-orders has data": (r) => {
      try {
        const body = JSON.parse(r.body);
        return body.code === 0 && Array.isArray(body.data);
      } catch {
        return false;
      }
    },
  });

  workOrderListDuration.add(res.timings.duration);
  errorRate.add(!ok);

  if (!ok) {
    return [];
  }

  return JSON.parse(res.body).data || [];
}

// ─── PDA 扫码报工 ──────────────────────────────────────────
function createWorkReport(token, workOrder) {
  const payload = JSON.stringify({
    scanCode: workOrder.orderNo,
    stepName: "上板",
    workstationCode: "SMT01-LOADER",
    operatorCode: "operator",
    goodQty: 1,
    scrapQty: 0,
    reworkQty: 0,
  });

  // pda-scan 接口为 AllowAnonymous，但仍传 token 以兼容未来鉴权变更
  const res = post(`${BASE_URL}/api/v1/work-reports/pda-scan`, payload, token);

  const ok = check(res, {
    "report status 2xx or 4xx": (r) => r.status >= 200 && r.status < 500,
  });

  workReportCreateDuration.add(res.timings.duration);
  errorRate.add(!ok);

  return ok;
}

// ─── 主测试流程 ────────────────────────────────────────────
export default function () {
  // Step 1: 登录
  const token = login("admin", "Admin@2026!");
  if (!token) {
    errorRate.add(1);
    sleep(1);
    return;
  }

  // Step 2: 获取工单列表
  const workOrders = getWorkOrders(token);
  if (workOrders.length === 0) {
    errorRate.add(1);
    sleep(1);
    return;
  }

  // Step 3: 选择一个可报工的工单（IN_PROGRESS=3 或 RELEASED=1），创建报工
  const reportable =
    workOrders.find((wo) => wo.status === 3 || wo.status === 1) ||
    workOrders[0];

  createWorkReport(token, reportable);

  // 模拟用户思考时间
  sleep(1);
}
