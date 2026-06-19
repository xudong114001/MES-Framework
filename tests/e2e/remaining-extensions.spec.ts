import { test, expect } from '@playwright/test'

const BASE = 'http://localhost:5180/api/v1'

// Helper: get auth token
async function getToken() {
  const res = await fetch(`${BASE}/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username: 'admin', password: 'Admin@2026!' })
  })
  const data = await res.json()
  return data.data?.token
}

test.describe('RBAC 权限管理', () => {
  test('登录返回角色信息', async () => {
    const res = await fetch(`${BASE}/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username: 'admin', password: 'Admin@2026!' })
    })
    const data = await res.json()
    expect(res.status()).toBe(200)
    expect(data.data?.userInfo?.roles).toContain('admin')
    expect(data.data?.token).toBeTruthy()
  })

  test('错误密码返回401', async () => {
    const res = await fetch(`${BASE}/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username: 'admin', password: 'wrong' })
    })
    expect(res.status()).toBe(401)
  })

  test('无效Token访问受保护接口返回401', async () => {
    const res = await fetch(`${BASE}/factory`, {
      headers: { 'Authorization': 'Bearer invalid-token' }
    })
    expect(res.status()).toBe(401)
  })
})

test.describe('LLM 知识库 API', () => {
  let token: string

  test.beforeAll(async () => {
    token = await getToken()
  })

  test('新增知识条目', async () => {
    const res = await fetch(`${BASE}/ai/knowledge/entries`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
      body: JSON.stringify({ category: 0, title: 'SMT工艺标准', content: 'SMT贴片温度控制在215-225°C', keywords: 'SMT,贴片,温度' })
    })
    expect(res.status()).toBe(200)
  })

  test('搜索知识库', async () => {
    const res = await fetch(`${BASE}/ai/knowledge/search?q=SMT&page=1&pageSize=20`, {
      headers: { 'Authorization': `Bearer ${token}` }
    })
    const data = await res.json()
    expect(res.status()).toBe(200)
    expect(data.data?.items).toBeDefined()
  })

  test('获取知识条目列表', async () => {
    const res = await fetch(`${BASE}/ai/knowledge/entries?page=1&pageSize=20`, {
      headers: { 'Authorization': `Bearer ${token}` }
    })
    expect(res.status()).toBe(200)
  })

  test('删除知识条目', async () => {
    const res = await fetch(`${BASE}/ai/knowledge/entries/999999`, {
      method: 'DELETE',
      headers: { 'Authorization': `Bearer ${token}` }
    })
    expect(res.status()).toBe(200)
  })
})

test.describe('角色访问控制', () => {
  let token: string

  test.beforeAll(async () => {
    token = await getToken()
  })

  test('管理员可访问集成管理', async () => {
    const res = await fetch(`${BASE}/integration/adapters/status`, {
      headers: { 'Authorization': `Bearer ${token}` }
    })
    expect(res.status()).toBe(200)
  })
})
