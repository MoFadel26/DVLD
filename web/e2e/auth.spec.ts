import { expect, test } from '@playwright/test'
import { ADMIN, signIn } from './helpers.ts'

test('signed-out visitors are sent to sign in, and wrong passwords are refused', async ({ page }) => {
  await page.goto('/licenses')
  await expect(page).toHaveURL(/\/login$/)

  await page.getByLabel('Username').fill(ADMIN.username)
  await page.getByLabel('Password').fill('wrong-password')
  await page.getByRole('button', { name: 'Sign in' }).click()
  await expect(page.getByRole('alert')).toContainText('Wrong username or password')

  await page.getByLabel('Password').fill(ADMIN.password)
  await page.getByRole('button', { name: 'Sign in' }).click()
  await expect(page).toHaveURL(/\/licenses$/)
  await expect(page.getByRole('heading', { name: 'Licenses' })).toBeVisible()
})

test('signing out revokes the token on the server', async ({ page }) => {
  await signIn(page)
  const token = await page.evaluate(() => JSON.parse(localStorage.getItem('dvld.session') ?? '{}').token as string)
  const me = () => page.request.get('/api/auth/me', { headers: { Authorization: `Bearer ${token}` } })
  expect((await me()).status()).toBe(200)

  await page.getByRole('button', { name: 'Sign out' }).click()
  await expect(page).toHaveURL(/\/login$/)

  expect((await me()).status()).toBe(401)
  await page.goto('/people')
  await expect(page).toHaveURL(/\/login$/)
})
