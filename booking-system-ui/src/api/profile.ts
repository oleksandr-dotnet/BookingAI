import type { UpdateUserProfileRequest, UserDisplay, UserProfile } from '../types/api'
import { apiFetch } from './client'

export function getMyProfile(token: string) {
  return apiFetch<UserProfile>('/profile/me', {}, token)
}

export function updateMyProfile(request: UpdateUserProfileRequest, token: string) {
  return apiFetch<UserProfile>('/profile/me', {
    method: 'PUT',
    body: JSON.stringify(request),
  }, token)
}

export function getUserDisplay(userId: string, token: string) {
  return apiFetch<UserDisplay>(`/users/${encodeURIComponent(userId)}/display`, {}, token)
}
