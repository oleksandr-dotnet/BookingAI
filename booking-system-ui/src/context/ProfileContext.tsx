import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import { getMyProfile } from '../api/profile'
import { useAuth } from './AuthContext'
import type { UserProfile } from '../types/api'

interface ProfileContextValue {
  profile: UserProfile | null
  isLoading: boolean
  refreshProfile: () => Promise<void>
  setProfile: (profile: UserProfile) => void
}

const ProfileContext = createContext<ProfileContextValue | null>(null)

export function ProfileProvider({ children }: { children: ReactNode }) {
  const { token, isAuthenticated } = useAuth()
  const [profile, setProfile] = useState<UserProfile | null>(null)
  const [isLoading, setIsLoading] = useState(false)

  const refreshProfile = useCallback(async () => {
    if (!token) {
      setProfile(null)
      return
    }
    setIsLoading(true)
    try {
      setProfile(await getMyProfile(token))
    } catch {
      setProfile(null)
    } finally {
      setIsLoading(false)
    }
  }, [token])

  useEffect(() => {
    if (isAuthenticated && token) {
      void refreshProfile()
    } else {
      setProfile(null)
    }
  }, [isAuthenticated, token, refreshProfile])

  const value = useMemo(
    () => ({
      profile,
      isLoading,
      refreshProfile,
      setProfile,
    }),
    [profile, isLoading, refreshProfile],
  )

  return <ProfileContext.Provider value={value}>{children}</ProfileContext.Provider>
}

export function useProfile(): ProfileContextValue {
  const context = useContext(ProfileContext)
  if (!context) {
    throw new Error('useProfile must be used within ProfileProvider')
  }
  return context
}
