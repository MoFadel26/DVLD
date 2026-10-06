import { expect, test, type Page } from '@playwright/test'
import { registerPerson, signIn, uniqueNationalNo } from './helpers.ts'

async function testPanel(page: Page, name: string) {
  return page.getByRole('region', { name })
}

async function schedule(page: Page, test: string) {
  const panel = await testPanel(page, test)
  await panel.getByRole('button', { name: /^Schedule/ }).click()
  await expect(panel.getByText('Awaiting result')).toBeVisible()
}

async function record(page: Page, test: string, result: 'Pass' | 'Fail', notes?: string) {
  const panel = await testPanel(page, test)
  if (notes) await panel.getByLabel('Examiner notes (optional)').fill(notes)
  await panel.getByRole('button', { name: result, exact: true }).click()
  await expect(panel.getByText('Awaiting result')).toHaveCount(0)
}

test('a new person goes from registration to an issued, serviced license', async ({ page }) => {
  await signIn(page)
  const nationalNo = uniqueNationalNo()
  const personId = await registerPerson(page, {
    nationalNo,
    firstName: 'Lina',
    lastName: 'Haddad',
    dateOfBirth: '1995-03-02',
  })
  await expect(page.getByRole('heading', { name: 'Lina Test Haddad' })).toBeVisible()
  await expect(page.getByText('No licenses yet.', { exact: false })).toBeVisible()

  // Apply for a Class 3 license
  await page.getByRole('link', { name: 'New application for this person' }).click()
  await page.getByRole('radio', { name: /Class 3 - Standard Driving License/ }).check()
  await page.getByRole('button', { name: 'Submit application' }).click()
  await expect(page).toHaveURL(/\/applications\/\d+$/)
  await expect(page.getByText('In progress').first()).toBeVisible()

  // Vision passes, Theory fails once and then passes, Practical passes
  await schedule(page, 'Vision test')
  await record(page, 'Vision test', 'Pass')
  await schedule(page, 'Theory test')
  await record(page, 'Theory test', 'Fail', 'Missed 3 of 10 sign questions')
  const theory = await testPanel(page, 'Theory test')
  await expect(theory.getByText('Failed')).toBeVisible()
  await expect(theory.getByText('Missed 3 of 10 sign questions')).toBeVisible()
  await schedule(page, 'Theory test')
  await record(page, 'Theory test', 'Pass')
  await schedule(page, 'Practical test')
  await record(page, 'Practical test', 'Pass')

  // Issue the license
  await page.getByLabel('Notes on the license (optional)').fill('Issued by e2e test')
  await page.getByRole('button', { name: 'Issue license' }).click()
  await expect(page).toHaveURL(/\/licenses\/\d+$/)
  const firstLicenseUrl = page.url()
  await expect(page.getByRole('heading', { name: 'Lina Test Haddad' })).toBeVisible()
  await expect(page.getByText('Active').first()).toBeVisible()

  // Detain, then release
  await page.getByLabel('Fine amount').fill('40')
  await page.getByRole('button', { name: 'Detain' }).click()
  await expect(page.getByRole('status')).toContainText('Detained with a fine of $40.00')
  await expect(page.getByText('Detained').first()).toBeVisible()
  await page.getByRole('button', { name: 'Release' }).click()
  await expect(page.getByRole('status')).toContainText('$40.00 fine collected')

  // Renew: a new license replaces the old one
  await page.getByRole('button', { name: 'Renew', exact: true }).click()
  await expect(page).not.toHaveURL(firstLicenseUrl)
  await expect(page.getByText('Renewal').first()).toBeVisible()

  // The person, the drivers list, and the licenses list all show it
  await page.goto(`/people/${personId}`)
  const licenses = page.getByRole('region', { name: 'Licenses' })
  await expect(licenses.getByRole('row')).toHaveCount(3) // header + renewed + original
  await page.goto('/drivers')
  await expect(page.getByRole('row', { name: new RegExp(nationalNo) })).toContainText('2')
  await page.goto('/licenses')
  await page.getByRole('searchbox').fill(nationalNo)
  await expect(page.getByRole('row')).toHaveCount(3)
})
