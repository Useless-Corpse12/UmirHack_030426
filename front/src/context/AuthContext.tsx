import { createContext, useContext, useState, type ReactNode } from 'react'
import type { AuthUser, Role } from '../types'

interface AuthCtx {
  user: AuthUser | null
  login: (u: AuthUser) => void
  logout: () => void
  setEmailConfirmed: () => void
}

const Ctx = createContext<AuthCtx>(null!)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(() => {
    const token = localStorage.getItem('token')
    const role = localStorage.getItem('role') as Role | null
    const userId = localStorage.getItem('userId')
    const displayName = localStorage.getItem('displayName')
    const isEmailConfirmed = localStorage.getItem('isEmailConfirmed') === 'true'
    if (token && role && userId && displayName)
      return { token, role, userId, displayName, isEmailConfirmed }
    return null
  })

  const login = (u: AuthUser) => {
    localStorage.setItem('token', u.token)
    localStorage.setItem('role', u.role)
    localStorage.setItem('userId', u.userId)
    localStorage.setItem('displayName', u.displayName)
    localStorage.setItem('isEmailConfirmed', String(u.isEmailConfirmed))
    setUser(u)
  }

  const logout = () => {
    localStorage.clear()
    setUser(null)
  }

  const setEmailConfirmed = () => {
    localStorage.setItem('isEmailConfirmed', 'true')
    setUser(prev => prev ? { ...prev, isEmailConfirmed: true } : prev)
  }

  return (
    <Ctx.Provider value={{ user, login, logout, setEmailConfirmed }}>
      {children}
    </Ctx.Provider>
  )
}

export const useAuth = () => useContext(Ctx)
