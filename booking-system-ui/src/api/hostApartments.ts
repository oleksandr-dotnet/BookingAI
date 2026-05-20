import type { ApartmentResponse, CreateApartmentRequest } from '../types/api'
import { apiFetch } from './client'

export function listHostApartments(token: string) {
  return apiFetch<ApartmentResponse[]>('/host/apartments', {}, token)
}

export function createHostApartment(request: CreateApartmentRequest, token: string) {
  return apiFetch<ApartmentResponse>('/host/apartments', {
    method: 'POST',
    body: JSON.stringify(request),
  }, token)
}
