import type { AdminUserDetail, AdminUserListResponse, BookingResponse, SetUserRolesRequest } from '../types/api'
import { apiFetch } from './client'

export type AdminUserRoleFilter = '' | 'Host' | 'Client' | 'Admin'

export interface ListAdminUsersParams {
  role?: AdminUserRoleFilter
  search?: string
  page?: number
  pageSize?: number
  sort?: 'email' | 'created'
}

export function listAdminUsers(token: string, params: ListAdminUsersParams = {}) {
  const query = new URLSearchParams()
  if (params.role) query.set('role', params.role)
  if (params.search?.trim()) query.set('search', params.search.trim())
  if (params.page) query.set('page', String(params.page))
  if (params.pageSize) query.set('pageSize', String(params.pageSize))
  if (params.sort) query.set('sort', params.sort)
  const qs = query.toString()
  return apiFetch<AdminUserListResponse>(`/admin/users${qs ? `?${qs}` : ''}`, {}, token)
}

export function getAdminUserById(token: string, userId: string) {
  return apiFetch<AdminUserDetail>(`/admin/users/${userId}`, {}, token)
}

export function listAdminUserBookings(token: string, userId: string) {
  return apiFetch<BookingResponse[]>(`/admin/users/${userId}/bookings`, {}, token)
}

export function lockAdminUser(token: string, userId: string) {
  return apiFetch<void>(`/admin/users/${userId}/lock`, { method: 'POST' }, token)
}

export function unlockAdminUser(token: string, userId: string) {
  return apiFetch<void>(`/admin/users/${userId}/unlock`, { method: 'POST' }, token)
}

export function setAdminUserRoles(token: string, userId: string, body: SetUserRolesRequest) {
  return apiFetch<AdminUserDetail>(
    `/admin/users/${userId}/roles`,
    { method: 'PUT', body: JSON.stringify(body) },
    token
  )
}
