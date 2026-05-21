import type { AdminBookingListResponse } from '../types/api'
import { apiFetch } from './client'

export interface ListAdminBookingsParams {
  userId?: string
  page?: number
  pageSize?: number
}

export function listAdminBookings(token: string, params: ListAdminBookingsParams = {}) {
  const query = new URLSearchParams()
  if (params.userId?.trim()) query.set('userId', params.userId.trim())
  if (params.page) query.set('page', String(params.page))
  if (params.pageSize) query.set('pageSize', String(params.pageSize))
  const qs = query.toString()
  return apiFetch<AdminBookingListResponse>(`/admin/bookings${qs ? `?${qs}` : ''}`, {}, token)
}
