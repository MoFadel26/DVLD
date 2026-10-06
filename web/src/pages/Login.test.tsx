import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Route, Routes } from 'react-router-dom'
import { describe, expect, it } from 'vitest'
import { getSession } from '../api'
import { json, mockApi, renderApp, testSession } from '../test/helpers'
import { Login } from './Login'

function app() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route path="/licenses" element={<p>Licenses page</p>} />
    </Routes>
  )
}

describe('Login', () => {
  it('shows the rule-style error for a wrong password', async () => {
    mockApi({
      'POST /auth/login': () =>
        json({ title: 'Invalid Credentials', detail: 'The username or password is incorrect.', status: 401 }, 401),
    })
    renderApp(app(), { route: '/login' })

    await userEvent.type(screen.getByLabelText('Username'), 'admin')
    await userEvent.type(screen.getByLabelText('Password'), 'wrong')
    await userEvent.click(screen.getByRole('button', { name: 'Sign in' }))

    expect(await screen.findByRole('alert')).toHaveTextContent('Wrong username or password')
    expect(getSession()).toBeNull()
  })

  it('stores the session and returns to the page that was requested', async () => {
    mockApi({ 'POST /auth/login': () => json(testSession) })
    renderApp(app(), { route: { pathname: '/login', state: { from: '/licenses' } } })

    await userEvent.type(screen.getByLabelText('Username'), 'admin')
    await userEvent.type(screen.getByLabelText('Password'), 'Admin@12345')
    await userEvent.click(screen.getByRole('button', { name: 'Sign in' }))

    expect(await screen.findByText('Licenses page')).toBeInTheDocument()
    expect(getSession()?.user.username).toBe('admin')
  })
})
