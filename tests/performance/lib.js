/**
 * MES Framework — k6 性能测试共享工具库
 *
 * 提供登录、请求封装等通用函数，供各测试脚本引用
 */
import http from 'k6/http';
import { check } from 'k6';
import { Rate } from 'k6/metrics';

const errorRate = new Rate('errors');

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5180';

/**
 * 登录并返回 JWT Token
 * @param {string} username
 * @param {string} password
 * @returns {string|null} token 或 null（登录失败时）
 */
export function login(username = 'admin', password = 'Admin@2026!') {
  const res = http.post(
    `${BASE_URL}/api/v1/auth/login`,
    JSON.stringify({ username, password }),
    { headers: { 'Content-Type': 'application/json' } },
  );

  const ok = check(res, {
    'login status 200': (r) => r.status === 200,
    'login has token': (r) => {
      try {
        const body = JSON.parse(r.body);
        return body.code === 0 && body.data?.token != null;
      } catch {
        return false;
      }
    },
  });

  if (!ok) {
    return null;
  }

  return JSON.parse(res.body).data.token;
}

/**
 * 构造带鉴权的请求参数
 * @param {string} token
 * @returns {object} k6 请求参数对象
 */
export function authParams(token) {
  return {
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    },
  };
}

/**
 * 带 check 的 GET 请求
 * @param {string} url - 完整 URL
 * @param {string} token - JWT Token
 * @param {object} checks - k6 check 定义
 * @returns {{ res: object, ok: boolean }}
 */
export function checkedGet(url, token, checks) {
  const res = http.get(url, authParams(token));
  const ok = check(res, checks);
  errorRate.add(!ok);
  return { res, ok };
}

/**
 * 带 check 的 POST 请求
 * @param {string} url - 完整 URL
 * @param {string} body - JSON 字符串
 * @param {string} token - JWT Token（可选）
 * @param {object} checks - k6 check 定义
 * @returns {{ res: object, ok: boolean }}
 */
export function checkedPost(url, body, token, checks) {
  const res = http.post(url, body, authParams(token));
  const ok = check(res, checks);
  errorRate.add(!ok);
  return { res, ok };
}

/**
 * 获取基础 URL
 * @returns {string}
 */
export function getBaseUrl() {
  return BASE_URL;
}

/**
 * 生成唯一标识（基于时间戳 + 随机数）
 * @param {string} prefix - 前缀
 * @returns {string}
 */
export function uniqueId(prefix = 'PERF') {
  return `${prefix}-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`;
}

export { errorRate, BASE_URL };
