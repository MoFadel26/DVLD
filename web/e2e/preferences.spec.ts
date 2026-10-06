import { expect, test } from '@playwright/test'
import { signIn } from './helpers.ts'

test('Arabic switches the whole app to right-to-left and is remembered', async ({ page }) => {
  await signIn(page)
  await page.getByRole('button', { name: /Interface language/ }).click()

  await expect(page.locator('html')).toHaveAttribute('dir', 'rtl')
  await expect(page.getByRole('link', { name: 'الأشخاص' })).toBeVisible()

  await page.reload()
  await expect(page.locator('html')).toHaveAttribute('dir', 'rtl')
})

test('the theme toggle switches to dark and back', async ({ page }) => {
  await signIn(page)
  const html = page.locator('html')
  const start = await html.getAttribute('data-theme')
  const other = start === 'dark' ? 'light' : 'dark'

  await page.getByRole('button', { name: /theme/i }).click()
  await expect(html).toHaveAttribute('data-theme', other)
  await page.getByRole('button', { name: /theme/i }).click()
  await expect(html).toHaveAttribute('data-theme', start!)
})
