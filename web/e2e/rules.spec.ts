import { expect, test } from '@playwright/test'
import { registerPerson, signIn, uniqueNationalNo } from './helpers.ts'

test('an under-age applicant is refused with the rule that stopped them', async ({ page }) => {
  await signIn(page)
  const personId = await registerPerson(page, {
    nationalNo: uniqueNationalNo(),
    firstName: 'Young',
    lastName: 'Driver',
    dateOfBirth: `${new Date().getFullYear() - 16}-01-01`,
  })

  await page.goto(`/applications/new?person=${personId}`)
  await expect(page.getByText('Needs 18. Applicant is 16.').first()).toBeVisible()
  await page.getByRole('radio', { name: /Class 1 - Small Motorcycle/ }).check()
  await page.getByRole('button', { name: 'Submit application' }).click()

  const alert = page.getByRole('alert')
  await expect(alert).toContainText('Rule: minimum age')
  await expect(alert).toContainText('minimum required age')
  await expect(page).toHaveURL(/\/applications\/new/)
})

test('a second pending application for the same class is refused', async ({ page }) => {
  await signIn(page)
  const personId = await registerPerson(page, {
    nationalNo: uniqueNationalNo(),
    firstName: 'Twice',
    lastName: 'Applied',
    dateOfBirth: '1990-01-01',
  })

  for (const attempt of [1, 2]) {
    await page.goto(`/applications/new?person=${personId}`)
    await page.getByRole('radio', { name: /Class 3 - Standard Driving License/ }).check()
    await page.getByRole('button', { name: 'Submit application' }).click()
    if (attempt === 1) await expect(page).toHaveURL(/\/applications\/\d+$/)
  }

  await expect(page.getByRole('alert')).toContainText('Rule: one pending application per class')
})
