import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it } from 'vitest'
import type { License } from '../api'
import { json, mockApi, renderApp, signIn } from '../test/helpers'
import { Licenses } from './Licenses'

function license(overrides: Partial<License>): License {
  return {
    licenseId: 1,
    applicationId: 1,
    driverId: 1,
    personId: 1,
    driverFullName: 'Ahmad Al-Sayed',
    nationalNo: 'N1001',
    licenseClassId: 3,
    className: 'Class 3 - Standard Driving License (Car License)',
    issueDate: '2026-10-06T00:00:00Z',
    expirationDate: '2036-10-06T00:00:00Z',
    notes: null,
    paidFees: 20,
    isActive: true,
    issueReason: 'FirstTime',
    isDetained: false,
    createdByUserId: 1,
    ...overrides,
  }
}

const licenses = [
  license({ licenseId: 3, driverFullName: 'Rami Najjar', nationalNo: 'N1003' }),
  license({ licenseId: 2, isDetained: true }),
  license({ licenseId: 1, isActive: false }),
]

describe('Licenses page', () => {
  it('lists every license with counts per status', async () => {
    signIn()
    mockApi({ 'GET /licenses': () => json(licenses) })
    renderApp(<Licenses />, { route: '/licenses' })

    expect(await screen.findAllByRole('row')).toHaveLength(4) // header + 3
    expect(screen.getByRole('button', { name: /Detained/ })).toHaveTextContent('1')
    expect(screen.getByRole('button', { name: /All/ })).toHaveTextContent('3')
  })

  it('filters by status and by search text', async () => {
    signIn()
    mockApi({ 'GET /licenses': () => json(licenses) })
    renderApp(<Licenses />, { route: '/licenses' })
    await screen.findAllByRole('row')

    await userEvent.click(screen.getByRole('button', { name: /Detained/ }))
    expect(screen.getAllByRole('row')).toHaveLength(2)
    expect(screen.getByText('LIC-2')).toBeInTheDocument()

    await userEvent.click(screen.getByRole('button', { name: /All/ }))
    await userEvent.type(screen.getByRole('searchbox'), 'n1003')
    expect(screen.getAllByRole('row')).toHaveLength(2)
    expect(screen.getByText('Rami Najjar')).toBeInTheDocument()

    await userEvent.clear(screen.getByRole('searchbox'))
    await userEvent.type(screen.getByRole('searchbox'), 'nobody')
    expect(screen.getByText('No licenses match this search.')).toBeInTheDocument()
  })
})
