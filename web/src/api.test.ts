import { describe, expect, it, vi } from 'vitest'
import { api, ApiError, getSession, SESSION_EXPIRED_EVENT, setSession } from './api'
import { json, mockApi, signIn, testSession } from './test/helpers'

describe('api client', () => {
  it('sends the stored token as a bearer header', async () => {
    signIn()
    const fetchMock = mockApi({ 'GET /people': () => json([]) })

    await api.people()

    const headers = fetchMock.mock.calls[0][1]?.headers as Record<string, string>
    expect(headers.Authorization).toBe('Bearer test-token')
  })

  it('sends no token when signed out', async () => {
    const fetchMock = mockApi({ 'GET /license-classes': () => json([]) })

    await api.licenseClasses()

    const headers = fetchMock.mock.calls[0][1]?.headers as Record<string, string>
    expect(headers.Authorization).toBeUndefined()
  })

  it('ignores an expired session', () => {
    setSession({ ...testSession, expiresAt: new Date(Date.now() - 1000).toISOString() })
    expect(getSession()).toBeNull()
  })

  it('clears the session and announces it when the API answers 401', async () => {
    signIn()
    mockApi({ 'GET /people': () => json({ title: 'Unauthorized' }, 401) })
    const expired = vi.fn()
    window.addEventListener(SESSION_EXPIRED_EVENT, expired)

    await expect(api.people()).rejects.toBeInstanceOf(ApiError)

    expect(getSession()).toBeNull()
    expect(expired).toHaveBeenCalledOnce()
    window.removeEventListener(SESSION_EXPIRED_EVENT, expired)
  })

  it('does not treat a failed sign-in as an expired session', async () => {
    mockApi({ 'POST /auth/login': () => json({ title: 'Invalid Credentials' }, 401) })
    const expired = vi.fn()
    window.addEventListener(SESSION_EXPIRED_EVENT, expired)

    await expect(api.login('admin', 'wrong')).rejects.toMatchObject({ status: 401, title: 'Invalid Credentials' })

    expect(expired).not.toHaveBeenCalled()
    window.removeEventListener(SESSION_EXPIRED_EVENT, expired)
  })

  it('turns problem details into an ApiError with the rule title and detail', async () => {
    signIn()
    mockApi({
      'POST /applications/local-license': () =>
        json({ title: 'Age Requirement Not Met', detail: 'Applicant age is 16 years.', status: 400 }, 400),
    })

    await expect(api.createApplication(2, 1)).rejects.toMatchObject({
      status: 400,
      title: 'Age Requirement Not Met',
      detail: 'Applicant age is 16 years.',
    })
  })

  it('reports an unreachable API as a network error', async () => {
    vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new TypeError('Failed to fetch')))

    await expect(api.people()).rejects.toMatchObject({ status: 0, title: 'Network' })
  })

  it('sends request bodies without a user id', async () => {
    signIn()
    const fetchMock = mockApi({ 'POST /licenses/detain': () => json({ detainId: 1, licenseId: 4, fineFees: 40 }) })

    await api.detain(4, 40)

    expect(JSON.parse(String(fetchMock.mock.calls[0][1]?.body))).toEqual({ licenseId: 4, fineFees: 40 })
  })
})
