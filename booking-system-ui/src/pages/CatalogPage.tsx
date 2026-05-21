import { useCallback, useEffect, useMemo, useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { listApartments } from '../api/apartments'
import { ApartmentCard } from '../components/ApartmentCard'
import { useAuth } from '../context/AuthContext'
import { ApiError, type ApartmentListItem } from '../types/api'
import {
  addDaysToDateInput,
  dateInputToEndIso,
  dateInputToStartIso,
  todayDateInputValue,
} from '../utils/dates'

export function CatalogPage() {
  const navigate = useNavigate()
  const { token, isClient } = useAuth()
  const [apartments, setApartments] = useState<ApartmentListItem[]>([])
  const [cityQuery, setCityQuery] = useState('')
  const [from, setFrom] = useState('')
  const [to, setTo] = useState('')
  const [availableOnly, setAvailableOnly] = useState(false)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [bookingApartment, setBookingApartment] = useState<ApartmentListItem | null>(null)
  const [bookStart, setBookStart] = useState('')
  const [bookEnd, setBookEnd] = useState('')
  const [bookError, setBookError] = useState<string | null>(null)

  const load = useCallback(async () => {
    setIsLoading(true)
    setError(null)
    try {
      const data = await listApartments({
        from: from ? dateInputToStartIso(from) : undefined,
        to: to ? dateInputToEndIso(to) : undefined,
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

  const filteredApartments = useMemo(() => {
    const query = cityQuery.trim().toLowerCase()
    if (!query) return apartments
    return apartments.filter((apt) => apt.city.toLowerCase().includes(query))
  }, [apartments, cityQuery])

  const openBook = (apartment: ApartmentListItem) => {
    setBookingApartment(apartment)
    setBookStart(from || addDaysToDateInput(todayDateInputValue(), 1))
    setBookEnd(to || addDaysToDateInput(todayDateInputValue(), 3))
    setBookError(null)
  }

  const closeBook = () => setBookingApartment(null)

  const handleBook = (event: FormEvent) => {
    event.preventDefault()
    if (!token || !bookingApartment) return
    setBookError(null)
    const start = dateInputToStartIso(bookStart)
    const end = dateInputToEndIso(bookEnd)
    if (new Date(end).getTime() <= new Date(start).getTime()) {
      setBookError('Check-out must be after check-in.')
      return
    }
    closeBook()
    navigate('/bookings/pay', {
      state: {
        apartmentId: bookingApartment.id,
        apartmentVersion: bookingApartment.version,
        apartmentName: bookingApartment.name,
        city: bookingApartment.city,
        thumbnailUrl: bookingApartment.thumbnailUrl,
        pricePerNight: bookingApartment.pricePerNight,
        guestCount: bookingApartment.guestCount,
        checkIn: bookStart,
        checkOut: bookEnd,
        start,
        end,
      },
    })
  }

  return (
    <section className="panel catalog-panel">
      <div className="panel-header">
        <div>
          <p className="eyebrow">Stays</p>
          <h1>Find your next place</h1>
          <p className="panel-lead">
            {isClient
              ? 'Search by location and dates, then book available homes.'
              : 'Browse listings like Airbnb. Sign in as a Client to book.'}
          </p>
        </div>
      </div>

      <form
        className="catalog-search"
        onSubmit={(e) => {
          e.preventDefault()
          void load()
        }}
      >
        <label className="catalog-search-field">
          <span>Where</span>
          <input
            type="text"
            placeholder="Search by city"
            value={cityQuery}
            onChange={(e) => setCityQuery(e.target.value)}
          />
        </label>
        <label className="catalog-search-field">
          <span>Check-in</span>
          <input type="date" value={from} onChange={(e) => setFrom(e.target.value)} />
        </label>
        <label className="catalog-search-field">
          <span>Check-out</span>
          <input
            type="date"
            value={to}
            min={from ? addDaysToDateInput(from, 1) : undefined}
            onChange={(e) => setTo(e.target.value)}
          />
        </label>
        <label className="catalog-search-field catalog-search-field-checkbox">
          <input
            type="checkbox"
            checked={availableOnly}
            onChange={(e) => setAvailableOnly(e.target.checked)}
            disabled={!from || !to}
          />
          <span>Available only</span>
        </label>
        <button type="submit" className="btn btn-primary catalog-search-btn" disabled={isLoading}>
          Search
        </button>
      </form>

      {error && (
        <div className="alert alert-error" role="alert">
          {error}
        </div>
      )}

      {isLoading && <p className="status-message">Loading apartments…</p>}

      {!isLoading && !error && (
        <div className="apartment-grid catalog-grid">
          {filteredApartments.length === 0 ? (
            <p className="status-message">No apartments found.</p>
          ) : (
            filteredApartments.map((apt) => (
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

      {bookingApartment && isClient && (
        <div className="modal-backdrop" role="presentation" onClick={closeBook}>
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
                <span>Check-in</span>
                <input
                  type="date"
                  value={bookStart}
                  onChange={(e) => setBookStart(e.target.value)}
                  required
                />
              </label>
              <label className="field">
                <span>Check-out</span>
                <input
                  type="date"
                  value={bookEnd}
                  min={bookStart ? addDaysToDateInput(bookStart, 1) : undefined}
                  onChange={(e) => setBookEnd(e.target.value)}
                  required
                />
              </label>
              <div className="modal-actions">
                <button type="button" className="btn btn-secondary" onClick={closeBook}>
                  Cancel
                </button>
                <button type="submit" className="btn btn-primary">
                  Continue to payment
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </section>
  )
}
