export type UserRole = 'Host' | 'Client'

export type Amenity = 'LargeBed' | 'Microwave' | 'Bath' | 'Shower'

export const AMENITY_OPTIONS: { value: Amenity; label: string }[] = [
  { value: 'LargeBed', label: 'Large bed' },
  { value: 'Microwave', label: 'Microwave' },
  { value: 'Bath', label: 'Bath (full tub)' },
  { value: 'Shower', label: 'Shower' },
]

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
  pricePerNight: number
  guestCount: number
  amenities: string[]
  version: number
  isAvailable?: boolean | null
}

export interface ApartmentResponse {
  id: string
  name: string
  description: string
  pricePerNight: number
  guestCount: number
  amenities: string[]
  version: number
  metadata?: Record<string, unknown> | null
}

export interface CreateApartmentRequest {
  name: string
  description: string
  pricePerNight: number
  guestCount: number
  amenities: string[]
  metadata?: Record<string, unknown> | null
}

export interface UpdateApartmentRequest {
  name: string
  description: string
  pricePerNight: number
  guestCount: number
  amenities: string[]
  version: number
  metadata?: Record<string, unknown> | null
}

export interface CreateBookingRequest {
  apartmentId: string
  apartmentVersion: number
  start: string
  end: string
}

export interface BookingResponse {
  id: string
  apartmentId: string
  start: string
  end: string
  pricePerNight: number
  guestCount: number
  amenities: string[]
}

export interface BookingSummaryAnalytics {
  totalBookings: number
  totalRevenue: number
  averagePricePerNight: number
}

export interface BookingsByApartmentAnalytics {
  apartmentId: string
  bookingCount: number
  revenueSum: number
}

export interface ActiveHostAnalytics {
  hostId: string
  bookingCount: number
}

export interface PriceQuantilesAnalytics {
  p25: number | null
  p50: number | null
  p75: number | null
}

export interface ApartmentOccupancyAnalytics {
  apartmentId: string
  bookingCount: number
  averageNightsBooked: number
}

export interface UserProfile {
  userId: string
  email: string
  userName: string | null
  roles: string[]
  firstName: string | null
  lastName: string | null
  dateOfBirth: string | null
  profileImageUrl: string | null
  displayName: string
  initials: string
  profileComplete: boolean
}

export interface UpdateUserProfileRequest {
  firstName: string
  lastName: string
  dateOfBirth: string | null
  profileImageUrl: string | null
}

export interface UserDisplay {
  userId: string
  displayName: string
  profileImageUrl: string | null
  initials: string
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
  readonly code?: string

  constructor(
    message: string,
    status: number,
    validationErrors?: Record<string, string[]>,
    code?: string,
  ) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.validationErrors = validationErrors
    this.code = code
  }
}
