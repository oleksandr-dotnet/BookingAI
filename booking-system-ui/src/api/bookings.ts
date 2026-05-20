import type { BookingResponse, CreateBookingRequest } from '../types/api'
import { apiFetch } from './client'

export function listMyBookings(token: string) {
  return apiFetch<BookingResponse[]>('/bookings', {}, token)
}

export function createBooking(request: CreateBookingRequest, token: string) {
  return apiFetch<BookingResponse>('/bookings', {
    method: 'POST',
    body: JSON.stringify(request),
  }, token)
}
