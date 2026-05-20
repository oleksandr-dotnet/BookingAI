import type {
  ActiveHostAnalytics,
  ApartmentOccupancyAnalytics,
  BookingsByApartmentAnalytics,
  BookingSummaryAnalytics,
  PriceQuantilesAnalytics,
} from '../types/api'
import { apiFetch } from './client'

export function getBookingSummary(token: string) {
  return apiFetch<BookingSummaryAnalytics>('/analytics/bookings/summary', {}, token)
}

export function getBookingsByApartment(token: string) {
  return apiFetch<BookingsByApartmentAnalytics[]>('/analytics/bookings/by-apartment', {}, token)
}

export function getActiveHosts(token: string, minBookings = 1) {
  return apiFetch<ActiveHostAnalytics[]>(
    `/analytics/hosts/active?minBookings=${minBookings}`,
    {},
    token,
  )
}

export function getPriceQuantiles(token: string) {
  return apiFetch<PriceQuantilesAnalytics>('/analytics/apartments/price-quantiles', {}, token)
}

export function getApartmentOccupancy(token: string, minAvgNights = 0) {
  return apiFetch<ApartmentOccupancyAnalytics[]>(
    `/analytics/apartments/occupancy?minAvgNights=${minAvgNights}`,
    {},
    token,
  )
}
