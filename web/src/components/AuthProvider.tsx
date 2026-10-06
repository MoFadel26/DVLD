import { useEffect, useMemo, useState, type ReactNode } from 'react'
import { api, getSession, SESSION_EXPIRED_EVENT, setSession, type Session } from '../api'
import { AuthContext, type Auth } from '../auth'

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSessionState] = useState<Session | null>(getSession)

  useEffect(() => {
    const expire = () => setSessionState(null)
    window.addEventListener(SESSION_EXPIRED_EVENT, expire)
    return () => window.removeEventListener(SESSION_EXPIRED_EVENT, expire)
  }, [])

  const value = useMemo<Auth>(
    () => ({
      user: session?.user ?? null,
      signIn: async (username, password) => {
        const next = await api.login(username, password)
        setSession(next)
        setSessionState(next)
      },
      signOut: () => {
        setSession(null)
        setSessionState(null)
      },
    }),
    [session],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
