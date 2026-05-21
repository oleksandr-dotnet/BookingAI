import type { FormEvent, ReactNode } from 'react'
import { Link } from 'react-router-dom'
import { formatCurrency } from '../utils/format'

interface ApartmentBookingCardProps {
  pricePerNight: number
  checkIn: string
  checkOut: string
  onCheckInChange: (value: string) => void
  onCheckOutChange: (value: string) => void
  minCheckOut?: string
  dateRangeValid: boolean
  nights: number
  total: number
  isClient: boolean
  canReserve: boolean
  isLoading: boolean
  onReserveClick: () => void
  onSubmit?: (event: FormEvent) => void
  children?: ReactNode
}

export function ApartmentBookingCard({
  pricePerNight,
  checkIn,
  checkOut,
  onCheckInChange,
  onCheckOutChange,
  minCheckOut,
  dateRangeValid,
  nights,
  total,
  isClient,
  canReserve,
  isLoading,
  onReserveClick,
  children,
}: ApartmentBookingCardProps) {
  return (
    <aside className="apartment-booking-card">
      <p className="apartment-booking-card-price">
        <strong>{formatCurrency(pricePerNight)}</strong> <span>night</span>
      </p>
      <div className="apartment-detail-dates">
        <label className="field">
          <span>Check-in</span>
          <input type="date" value={checkIn} onChange={(e) => onCheckInChange(e.target.value)} />
        </label>
        <label className="field">
          <span>Check-out</span>
          <input
            type="date"
            value={checkOut}
            min={minCheckOut}
            onChange={(e) => onCheckOutChange(e.target.value)}
          />
        </label>
      </div>

      {checkIn && checkOut && !dateRangeValid && (
        <p className="status-message">Check-out must be after check-in.</p>
      )}

      {dateRangeValid && (
        <div className="apartment-detail-total">
          <p>
            <strong>{nights}</strong> {nights === 1 ? 'night' : 'nights'} × {formatCurrency(pricePerNight)}
          </p>
          <p className="apartment-detail-total-price">
            Total <strong>{formatCurrency(total)}</strong>
          </p>
        </div>
      )}

      {isClient ? (
        <button
          type="button"
          className="btn btn-primary btn-block"
          disabled={!canReserve || isLoading}
          onClick={onReserveClick}
        >
          Reserve
        </button>
      ) : (
        <p className="status-message">
          <Link to="/login">Sign in</Link> as a Client to reserve.
        </p>
      )}
      {children}
    </aside>
  )
}
