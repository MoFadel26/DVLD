import { createContext, useContext } from 'react'
import type { User } from './api'

export type Auth = {
  user: User | null
  signIn: (username: string, password: string) => Promise<void>
  signOut: () => Promise<void>
}

export const AuthContext = createContext<Auth | null>(null)

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used inside AuthProvider')
  return ctx
}
