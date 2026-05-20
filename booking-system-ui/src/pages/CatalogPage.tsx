import { useCallback, useEffect, useState, type FormEvent } from 'react'
import { listApartments } from '../api/apartments'
import { createBooking } from '../api/bookings'
import { ApartmentCard } from '../components/ApartmentCard'
import { useAuth } from '../context/AuthContext'
import { ApiError, type ApartmentListItem } from '../types/api'

function toIsoLocalInput(date: Date): string {
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`
}

function localInputToIso(value: string): string {
  return new Date(value).toISOString()
}

export function CatalogPage() {
  const { token, isClient } = useAuth()
  const [apartments, setApartments] = useState<ApartmentListItem[]>([])
  const [from, setFrom] = useState('')
  const [to, setTo] = useState('')
  const [availableOnly, setAvailableOnly] = useState(false)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [bookingApartmentId, setBookingApartmentId] = useState<string | null>(null)
  const [bookStart, setBookStart] = useState('')
  const [bookEnd, setBookEnd] = useState('')
  const [bookError, setBookError] = useState<string | null>(null)
  const [bookSuccess, setBookSuccess] = useState<string | null>(null)
  const [isBooking, setIsBooking] = useState(false)

  const load = useCallback(async () => {
    setIsLoading(true)
    setError(null)
    try {
      const data = await listApartments({
        from: from ? localInputToIso(from) : undefined,
        to: to ? localInputToIso(to) : undefined,
        availableOnly: availableOnly && Boolean(from && to),
      })
      setApartments(data)
    } catch (err) {
      setApartments([])
      if (err instanceof ApiError) setError(err.message)
      else setError('Failed to load apartments.')
    } finally {
      setIsLoading(false)
    }
  }, [from, to, availableOnly])

  useEffect(() => {
    void load()
  }, [load])

  const openBook = (apartment: ApartmentListItem) => {
    setBookingApartmentId(apartment.id)
    setBookStart(from || toIsoLocalInput(new Date(Date.now() + 86400000)))
    setBookEnd(to || toIsoLocalInput(new Date(Date.now() + 86400000 * 3)))
    setBookError(null)
    setBookSuccess(null)
  }

  const handleBook = async (event: FormEvent) => {
    event.preventDefault()
    if (!token || !bookingApartmentId) return
    setIsBooking(true)
    setBookError(null)
    setBookSuccess(null)
    try {
      await createBooking(
        {
          apartmentId: bookingApartmentId,
          start: localInputToIso(bookStart),
          end: localInputToIso(bookEnd),
        },
        token,
      )
      setBookSuccess('Booking created.')
      setBookingApartmentId(null)
      await load()
    } catch (err) {
      if (err instanceof ApiError) {
        if (err.status === 409) setBookError('This apartment is not available for the selected dates.')
        else setBookError(err.message)
      } else {
        setBookError('Booking failed.')
      }
    } finally {
      setIsBooking(false)
    }
  }

  return (
    <section className="panel">
      <div className="panel-header">
        <div>
          <p className="eyebrow">Browse</p>
          <h1>Apartments</h1>
          <p className="panel-lead">
            {isClient
              ? 'Filter by dates and book available apartments.'
              : 'Public catalog. Sign in as a Client to make a booking.'}
          </p>
        </div>
        <button type="button" className="btn btn-secondary" onClick={() => void load()} disabled={isLoading}>
          Refresh
        </button>
      </div>

      <form
        className="filter-bar"
        onSubmit={(e) => {
          e.preventDefault()
          void load()
        }}
      >
        <label className="field">
          <span>From</span>
          <input type="datetime-local" value={from} onChange={(e) => setFrom(e.target.value)} />
        </label>
        <label className="field">
          <span>To</span>
          <input type="datetime-local" value={to} onChange={(e) => setTo(e.target.value)} />
        </label>
        <label className="field field-checkbox">
          <input
            type="checkbox"
            checked={availableOnly}
            onChange={(e) => setAvailableOnly(e.target.checked)}
            disabled={!from || !to}
          />
          <span>Available only</span>
        </label>
        <button type="submit" className="btn btn-primary" disabled={isLoading}>
          Apply filters
        </button>
      </form>

      {bookSuccess && (
        <div className="alert alert-success" role="status">
          {bookSuccess}
        </div>
      )}

      {error && (
        <div className="alert alert-error" role="alert">
          {error}
        </div>
      )}

      {isLoading && <p className="status-message">Loading apartments…</p>}

      {!isLoading && !error && (
        <div className="apartment-grid">
          {apartments.length === 0 ? (
            <p className="status-message">No apartments found.</p>
          ) : (
            apartments.map((apt) => (
              <ApartmentCard key={apt.id} apartment={apt}>
                {isClient &&
                  (apt.isAvailable === undefined || apt.isAvailable === null || apt.isAvailable) && (
                    <button type="button" className="btn btn-primary btn-sm" onClick={() => openBook(apt)}>
                      Book
                    </button>
                  )}
              </ApartmentCard>
            ))
          )}
        </div>
      )}

      {bookingApartmentId && isClient && (
        <div className="modal-backdrop" role="presentation" onClick={() => setBookingApartmentId(null)}>
          <div
            className="modal"
            role="dialog"
            aria-labelledby="book-title"
            onClick={(e) => e.stopPropagation()}
          >
            <h2 id="book-title">Book apartment</h2>
            <form className="auth-form" onSubmit={(e) => void handleBook(e)}>
              {bookError && (
                <div className="alert alert-error" role="alert">
                  {bookError}
                </div>
              )}
              <label className="field">
                <span>Start</span>
                <input
                  type="datetime-local"
                  value={bookStart}
                  onChange={(e) => setBookStart(e.target.value)}
                  required
                />
              </label>
              <label className="field">
                <span>End</span>
                <input
                  type="datetime-local"
                  value={bookEnd}
                  onChange={(e) => setBookEnd(e.target.value)}
                  required
                />
              </label>
              <div className="modal-actions">
                <button type="button" className="btn btn-secondary" onClick={() => setBookingApartmentId(null)}>
                  Cancel
                </button>
                <button type="submit" className="btn btn-primary" disabled={isBooking}>
                  {isBooking ? 'Booking…' : 'Confirm booking'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </section>
  )
}
