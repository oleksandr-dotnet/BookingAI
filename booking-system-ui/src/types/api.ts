export type UserRole = 'Host' | 'Client'

export interface RegisterRequest {
  email: string
  password: string
  role: UserRole
}

export interface LoginRequest {
  email: string
  password: string
}

export interface RegisterResponse {
  userId: string
}

export interface AuthResponse {
  accessToken: string
  expiresIn: number
}

export interface ApartmentListItem {
  id: string
  name: string
  description: string
  isAvailable?: boolean | null
}

export interface ApartmentResponse {
  id: string
  name: string
  description: string
}

export interface CreateApartmentRequest {
  name: string
  description: string
}

export interface CreateBookingRequest {
  apartmentId: string
  start: string
  end: string
}

export interface BookingResponse {
  id: string
  apartmentId: string
  start: string
  end: string
}

export interface ValidationProblemDetails {
  type?: string
  title?: string
  status?: number
  errors?: Record<string, string[]>
}

export class ApiError extends Error {
  readonly status: number
  readonly validationErrors?: Record<string, string[]>

  constructor(message: string, status: number, validationErrors?: Record<string, string[]>) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.validationErrors = validationErrors
  }
}
