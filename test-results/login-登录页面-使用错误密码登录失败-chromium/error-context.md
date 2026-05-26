# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: login.spec.ts >> 登录页面 >> 使用错误密码登录失败
- Location: tests\e2e\login.spec.ts:27:3

# Error details

```
Error: expect(locator).toBeVisible() failed

Locator: getByText(/登录失败|用户名或密码错误|请求失败/)
Expected: visible
Timeout: 10000ms
Error: element(s) not found

Call log:
  - Expect "toBeVisible" with timeout 10000ms
  - waiting for getByText(/登录失败|用户名或密码错误|请求失败/)

```

```yaml
- img
- heading "MES 制造执行系统" [level=1]
- paragraph: Manufacturing Execution System
- img
- textbox "用户名": admin
- img
- textbox "密码": wrong_password
- img
- button "登 录"
```

# Test source

```ts
  1  | import { test, expect } from '@playwright/test'
  2  | 
  3  | test.describe('登录页面', () => {
  4  |   test.beforeEach(async ({ page }) => {
  5  |     await page.goto('/login')
  6  |   })
  7  | 
  8  |   test('能正常打开登录页面', async ({ page }) => {
  9  |     await expect(page).toHaveTitle(/MES/i)
  10 |     await expect(page.getByText('MES 制造执行系统')).toBeVisible()
  11 |     await expect(page.getByPlaceholder('用户名')).toBeVisible()
  12 |     await expect(page.getByPlaceholder('密码')).toBeVisible()
  13 |   })
  14 | 
  15 |   test('使用正确凭据登录成功', async ({ page }) => {
  16 |     await page.getByPlaceholder('用户名').fill('admin')
  17 |     await page.getByPlaceholder('密码').fill('Admin@2026!')
  18 |     await page.getByRole('button', { name: '登 录' }).click()
  19 | 
  20 |     // 登录成功后应跳转到首页看板
  21 |     await expect(page).toHaveURL(/\/dashboard/, { timeout: 15_000 })
  22 |     // token 应已存入 localStorage
  23 |     const token = await page.evaluate(() => localStorage.getItem('mes_token'))
  24 |     expect(token).toBeTruthy()
  25 |   })
  26 | 
  27 |   test('使用错误密码登录失败', async ({ page }) => {
  28 |     await page.getByPlaceholder('用户名').fill('admin')
  29 |     await page.getByPlaceholder('密码').fill('wrong_password')
  30 |     await page.getByRole('button', { name: '登 录' }).click()
  31 | 
  32 |     // 应显示错误提示（Element Plus ElMessage）
> 33 |     await expect(page.getByText(/登录失败|用户名或密码错误|请求失败/)).toBeVisible({ timeout: 10_000 })
     |                                                        ^ Error: expect(locator).toBeVisible() failed
  34 |     // 应仍停留在登录页
  35 |     await expect(page).toHaveURL(/\/login/)
  36 |   })
  37 | 
  38 |   test('空表单提交时显示验证提示', async ({ page }) => {
  39 |     await page.getByRole('button', { name: '登 录' }).click()
  40 | 
  41 |     // Element Plus 表单验证提示
  42 |     await expect(page.getByText('请输入用户名')).toBeVisible()
  43 |     await expect(page.getByText('请输入密码')).toBeVisible()
  44 |   })
  45 | })
  46 | 
```