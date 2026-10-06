import { expect, type Page } from '@playwright/test'

export const ADMIN = { username: 'admin', password: 'Admin@12345' }

export async function signIn(page: Page, path = '/') {
  await page.goto(path)
  await expect(page).toHaveURL(/\/login$/)
  await page.getByLabel('Username').fill(ADMIN.username)
  await page.getByLabel('Password').fill(ADMIN.password)
  await page.getByRole('button', { name: 'Sign in' }).click()
  // Wait for the signed-in shell (the sign-in page also shows "admin" in its demo hint).
  await expect(page).not.toHaveURL(/\/login$/)
  await expect(page.getByRole('button', { name: 'Sign out' })).toBeVisible()
}

/** A national number no other test run has used, so tests can share one database. */
export function uniqueNationalNo() {
  return `E2E${Date.now().toString(36)}${Math.floor(Math.random() * 1000)}`.toUpperCase()
}

export async function registerPerson(page: Page, person: { nationalNo: string; firstName: string; lastName: string; dateOfBirth: string }) {
  await page.goto('/people/new')
  await page.getByLabel('National number').fill(person.nationalNo)
  await page.getByLabel('First name').fill(person.firstName)
  await page.getByLabel('Second name').fill('Test')
  await page.getByLabel('Last name').fill(person.lastName)
  await page.getByLabel('Date of birth').fill(person.dateOfBirth)
  await page.getByLabel('Nationality').selectOption({ label: 'Jordan' })
  await page.getByLabel('Phone').fill('+962790000000')
  await page.getByLabel('Email').fill(`${person.nationalNo.toLowerCase()}@example.com`)
  await page.getByLabel('Address').fill('Amman')
  await page.getByRole('button', { name: 'Save person' }).click()
  await expect(page).toHaveURL(/\/people\/\d+$/)
  return Number(page.url().split('/').pop())
}
