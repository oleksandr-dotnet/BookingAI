import { useCallback, useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { listMyBookings } from '../api/bookings'
import { useAuth } from '../context/AuthContext'
import { ApiError, type BookingResponse } from '../types/api'

export function ClientBookingsPage() {
  const { token } = useAuth()
  const [bookings, setBookings] = useState<BookingResponse[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    if (!token) return
    setIsLoading(true)
    setError(null)
    try {
      setBookings(await listMyBookings(token))
    } catch (err) {
      setBookings([])
      if (err instanceof ApiError) setError(err.message)
      else setError('Failed to load bookings.')
    } finally {
      setIsLoading(false)
    }
  }, [token])

  useEffect(() => {
    void load()
  }, [load])

  return (
    <section className="panel">
      <div className="panel-header">
        <div>
          <p className="eyebrow">Client</p>
          <h1>My bookings</h1>
          <p className="panel-lead">Reservations you created as a client.</p>
        </div>
        <Link to="/apartments" className="btn btn-primary">
          Browse apartments
        </Link>
      </div>

      {isLoading && <p className="status-message">Loading bookings…</p>}
      {error && (
        <div className="alert alert-error" role="alert">
          {error}
        </div>
      )}

      {!isLoading && !error && bookings.length === 0 && (
        <p className="status-message">
          No bookings yet. <Link to="/apartments">Find an apartment</Link>
        </p>
      )}

      {!isLoading && !error && bookings.length > 0 && (
        <ul className="booking-list">
          {bookings.map((b) => (
            <li key={b.id} className="booking-item">
              <div>
                <p className="booking-item-id">Apartment {b.apartmentId}</p>
                <p className="booking-item-dates">
                  {new Date(b.start).toLocaleString()} — {new Date(b.end).toLocaleString()}
                </p>
                <p className="booking-item-meta">
                  ${b.pricePerNight}/night · {b.guestCount} guests
                </p>
                {b.amenities.length > 0 && (
                  <ul className="amenity-tags">
                    {b.amenities.map((a) => (
                      <li key={a}>{a}</li>
                    ))}
                  </ul>
                )}
              </div>
            </li>
          ))}
        </ul>
      )}
    </section>
  )
}
