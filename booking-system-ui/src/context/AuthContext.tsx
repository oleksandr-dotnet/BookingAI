import {
  createContext,
  useCallback,
  useContext,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import { login as loginApi } from '../api/auth'
import type { LoginRequest } from '../types/api'
import { getRolesFromToken } from '../utils/jwt'

const TOKEN_KEY = 'booking_system_access_token'

interface AuthContextValue {
  token: string | null
  roles: string[]
  isAuthenticated: boolean
  isHost: boolean
  isClient: boolean
  login: (request: LoginRequest) => Promise<string[]>
  logout: () => void
  setToken: (token: string) => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

function readStoredToken(): string | null {
  return localStorage.getItem(TOKEN_KEY)
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setTokenState] = useState<string | null>(() => readStoredToken())

  const roles = useMemo(() => (token ? getRolesFromToken(token) : []), [token])

  const setToken = useCallback((value: string) => {
    localStorage.setItem(TOKEN_KEY, value)
    setTokenState(value)
  }, [])

  const logout = useCallback(() => {
    localStorage.removeItem(TOKEN_KEY)
    setTokenState(null)
  }, [])

  const login = useCallback(
    async (request: LoginRequest) => {
      const response = await loginApi(request)
      setToken(response.accessToken)
      return getRolesFromToken(response.accessToken)
    },
    [setToken],
  )

  const value = useMemo(
    () => ({
      token,
      roles,
      isAuthenticated: token !== null,
      isHost: roles.includes('Host'),
      isClient: roles.includes('Client'),
      login,
      logout,
      setToken,
    }),
    [token, roles, login, logout, setToken],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider')
  }
  return context
}
