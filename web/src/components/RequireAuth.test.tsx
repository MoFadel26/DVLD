import { screen } from '@testing-library/react'
import { Route, Routes } from 'react-router-dom'
import { describe, expect, it } from 'vitest'
import { renderApp, signIn } from '../test/helpers'
import { RequireAuth } from './RequireAuth'

function app() {
  return (
    <Routes>
      <Route path="/login" element={<p>Sign-in page</p>} />
      <Route
        path="/people"
        element={
          <RequireAuth>
            <p>People page</p>
          </RequireAuth>
        }
      />
    </Routes>
  )
}

describe('RequireAuth', () => {
  it('sends a signed-out visitor to the sign-in page', () => {
    renderApp(app(), { route: '/people' })
    expect(screen.getByText('Sign-in page')).toBeInTheDocument()
  })

  it('shows the page to a signed-in user', () => {
    signIn()
    renderApp(app(), { route: '/people' })
    expect(screen.getByText('People page')).toBeInTheDocument()
  })
})
