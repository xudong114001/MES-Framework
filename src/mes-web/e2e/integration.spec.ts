import { test, expect } from '@playwright/test';

test.describe('系统集成页面', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/integration/dashboard');
  });

  test('集成仪表板页面加载', async ({ page }) => {
    await expect(page.locator('text=系统集成')).toBeVisible();
  });

  test('显示三个适配器卡片', async ({ page }) => {
    await expect(page.locator('text=ERP 系统')).toBeVisible();
    await expect(page.locator('text=WMS 系统')).toBeVisible();
    await expect(page.locator('text=PLC 设备')).toBeVisible();
  });

  test('显示测试连接按钮', async ({ page }) => {
    await expect(page.locator('text=测试连接')).toHaveCount(3);
  });

  test('显示同步按钮', async ({ page }) => {
    await expect(page.locator('text=手动同步')).toHaveCount(3);
  });

  test('导航到日志页面', async ({ page }) => {
    await page.click('text=同步日志');
    await expect(page.locator('text=同步���志')).toBeVisible();
  });
});

test.describe('集成日志页面', () => {
  test('日志页面加载', async ({ page }) => {
    await page.goto('/integration/logs');
    await expect(page.locator('text=同步日志')).toBeVisible();
  });

  test('显示日志表格', async ({ page }) => {
    await page.goto('/integration/logs');
    await expect(page.locator('text=时间')).toBeVisible();
    await expect(page.locator('text=事件类型')).toBeVisible();
    await expect(page.locator('text=状态')).toBeVisible();
  });

  test('类型筛选功能', async ({ page }) => {
    await page.goto('/integration/logs');
    await page.click('.el-select');
    await expect(page.locator('.el-select-dropdown')).toBeVisible();
  });
});

test.describe('适配器交互', () => {
  test('测试连接按钮可点击', async ({ page }) => {
    await page.goto('/integration/dashboard');
    const testButton = page.locator('button:has-text("测试连接")').first();
    await expect(testButton).toBeEnabled();
  });

  test('手动同步按钮可点击', async ({ page }) => {
    await page.goto('/integration/dashboard');
    const syncButton = page.locator('button:has-text("手动同步")').first();
    await expect(syncButton).toBeEnabled();
  });

  test('查看日志链接存在', async ({ page }) => {
    await page.goto('/integration/dashboard');
    const viewLogLink = page.locator('a:has-text("查看日志")').first();
    await expect(viewLogLink).toBeVisible();
  });
});