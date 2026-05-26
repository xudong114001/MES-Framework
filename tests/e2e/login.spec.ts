import { test, expect } from '@playwright/test'

test.describe('登录页面', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login')
  })

  test('能正常打开登录页面', async ({ page }) => {
    await expect(page).toHaveTitle(/MES/i)
    await expect(page.getByText('MES 制造执行系统')).toBeVisible()
    await expect(page.getByPlaceholder('用户名')).toBeVisible()
    await expect(page.getByPlaceholder('密码')).toBeVisible()
  })

  test('使用正确凭据登录成功', async ({ page }) => {
    await page.getByPlaceholder('用户名').fill('admin')
    await page.getByPlaceholder('密码').fill('Admin@2026!')
    await page.getByRole('button', { name: '登 录' }).click()

    // 登录成功后应跳转到首页看板
    await expect(page).toHaveURL(/\/dashboard/, { timeout: 15_000 })
    // token 应已存入 localStorage
    const token = await page.evaluate(() => localStorage.getItem('mes_token'))
    expect(token).toBeTruthy()
  })

  test('使用错误密码登录失败', async ({ page }) => {
    await page.getByPlaceholder('用户名').fill('admin')
    await page.getByPlaceholder('密码').fill('wrong_password')
    await page.getByRole('button', { name: '登 录' }).click()

    // 应显示错误提示（Element Plus ElMessage）
    await expect(page.getByText(/登录失败|用户名或密码错误|请求失败/)).toBeVisible({ timeout: 10_000 })
    // 应仍停留在登录页
    await expect(page).toHaveURL(/\/login/)
  })

  test('空表单提交时显示验证提示', async ({ page }) => {
    await page.getByRole('button', { name: '登 录' }).click()

    // Element Plus 表单验证提示
    await expect(page.getByText('请输入用户名')).toBeVisible()
    await expect(page.getByText('请输入密码')).toBeVisible()
  })
})
