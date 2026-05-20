import type { ApartmentListItem } from '../types/api'
import { apiFetch } from './client'

export interface ListApartmentsParams {
  from?: string
  to?: string
  availableOnly?: boolean
}

export function listApartments(params: ListApartmentsParams = {}) {
  const search = new URLSearchParams()
  if (params.from) search.set('from', params.from)
  if (params.to) search.set('to', params.to)
  if (params.availableOnly) search.set('availableOnly', 'true')
  const query = search.toString()
  const path = query ? `/apartments?${query}` : '/apartments'
  return apiFetch<ApartmentListItem[]>(path)
}
