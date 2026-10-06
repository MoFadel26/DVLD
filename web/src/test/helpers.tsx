import { render } from '@testing-library/react'
import type { ReactElement } from 'react'
import { MemoryRouter } from 'react-router-dom'
import { vi } from 'vitest'
import type { Session } from '../api'
import { AuthProvider } from '../components/AuthProvider'
import { I18nProvider } from '../components/I18nProvider'

export function json(body: unknown, status = 200) {
  return new Response(status === 204 ? null : JSON.stringify(body), {
    status,
    headers: { 'Content-Type': 'application/json' },
  })
}

/** Routes fetch calls by "METHOD /path" (without the /api prefix) to canned responses. */
export function mockApi(routes: Record<string, () => Response>) {
  const fetchMock = vi.fn(async (input: RequestInfo | URL, init?: RequestInit) => {
    const path = String(input).replace(/^\/api/, '')
    const key = `${init?.method ?? 'GET'} ${path}`
    const handler = routes[key]
    if (!handler) throw new Error(`Unexpected request: ${key}`)
    return handler()
  })
  vi.stubGlobal('fetch', fetchMock)
  return fetchMock
}

export const testSession: Session = {
  token: 'test-token',
  expiresAt: new Date(Date.now() + 60 * 60 * 1000).toISOString(),
  user: { userId: 1, username: 'admin' },
}

export function signIn(session: Session = testSession) {
  localStorage.setItem('dvld.session', JSON.stringify(session))
}

export function renderApp(ui: ReactElement, { route = '/', lang = 'en' }: { route?: string | { pathname: string; state?: unknown }; lang?: 'en' | 'ar' } = {}) {
  localStorage.setItem('dvld.lang', lang)
  return render(
    <MemoryRouter initialEntries={[route]}>
      <I18nProvider>
        <AuthProvider>{ui}</AuthProvider>
      </I18nProvider>
    </MemoryRouter>,
  )
}
