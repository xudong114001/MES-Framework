import { test as base, expect } from '@playwright/test'

// 扩展 fixture：提供已登录的 page
const test = base.extend<{ authenticatedPage: import('@playwright/test').Page }>({
  authenticatedPage: async ({ page }, use) => {
    await page.goto('/login')
    await page.getByPlaceholder('用户名').fill('admin')
    await page.getByPlaceholder('密码').fill('Admin@2026!')
    await page.getByRole('button', { name: '登 录' }).click()
    await expect(page).toHaveURL(/\/dashboard/, { timeout: 15_000 })
    await use(page)
  },
})

test.describe('质量管理页面', () => {
  test('能正常打开质检列表页面', async ({ authenticatedPage: page }) => {
    await page.goto('/qc')
    await expect(page.getByText('质量管理')).toBeVisible()
    await expect(page.locator('table')).toBeVisible()
  })

  test('质检列表包含表头列', async ({ authenticatedPage: page }) => {
    await page.goto('/qc')
    await expect(page.getByRole('columnheader', { name: '质检单号' })).toBeVisible()
    await expect(page.getByRole('columnheader', { name: '来源类型' })).toBeVisible()
    await expect(page.getByRole('columnheader', { name: '结果' })).toBeVisible()
  })

  test('新增质检单弹窗可以打开', async ({ authenticatedPage: page }) => {
    await page.goto('/qc')

    const createBtn = page.getByRole('button', { name: '新增质检单' })
    await expect(createBtn).toBeVisible()
    await createBtn.click()

    // 弹窗应出现
    await expect(page.getByRole('dialog').getByText('新增质检单')).toBeVisible()
    await expect(page.getByText('来源类型')).toBeVisible()
    await expect(page.getByText('质检员')).toBeVisible()
  })
})
