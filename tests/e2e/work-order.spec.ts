import { test as base, expect } from '@playwright/test'

// 扩展 fixture：提供已登录的 page
const test = base.extend<{ authenticatedPage: import('@playwright/test').Page }>({
  authenticatedPage: async ({ page }, use) => {
    // 通过 UI 登录
    await page.goto('/login')
    await page.getByPlaceholder('用户名').fill('admin')
    await page.getByPlaceholder('密码').fill('Admin@2026!')
    await page.getByRole('button', { name: '登 录' }).click()
    await expect(page).toHaveURL(/\/dashboard/, { timeout: 15_000 })
    await use(page)
  },
})

test.describe('工单管理页面', () => {
  test('能正常打开工单列表页面', async ({ authenticatedPage: page }) => {
    await page.goto('/work-order')
    await expect(page.getByText('工单管理')).toBeVisible()
    // 表格应该可见（即使没有数据也有表头）
    await expect(page.locator('table')).toBeVisible()
  })

  test('工单列表包含表头列', async ({ authenticatedPage: page }) => {
    await page.goto('/work-order')
    await expect(page.getByRole('columnheader', { name: '工单号' })).toBeVisible()
    await expect(page.getByRole('columnheader', { name: '物料名称' })).toBeVisible()
    await expect(page.getByRole('columnheader', { name: '状态' })).toBeVisible()
  })

  test('筛选条件可以交互', async ({ authenticatedPage: page }) => {
    await page.goto('/work-order')

    // 状态下拉框
    const statusSelect = page.locator('.filter-form').getByPlaceholder('全部状态')
    await statusSelect.click()
    await page.getByText('待下达').click()

    // 查询按钮
    await page.getByRole('button', { name: '查询' }).click()

    // 页面应仍在工单管理页面
    await expect(page.getByText('工单管理')).toBeVisible()
  })

  test('点击新增工单按钮跳转', async ({ authenticatedPage: page }) => {
    await page.goto('/work-order')

    const createBtn = page.getByRole('button', { name: '新增工单' })
    await expect(createBtn).toBeVisible()
    await createBtn.click()

    // 跳转到新建工单页面
    await expect(page).toHaveURL(/\/work-order\/new/, { timeout: 10_000 })
  })
})
