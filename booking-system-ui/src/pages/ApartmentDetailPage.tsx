import { useCallback, useEffect, useMemo, useState, type FormEvent } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { getApartment } from '../api/apartments'
import { useAuth } from '../context/AuthContext'
import { AmenityTags } from '../components/AmenityTags'
import { ApartmentBookingCard } from '../components/ApartmentBookingCard'
import { ApartmentGallery } from '../components/ApartmentGallery'
import { ApiError, type ApartmentDetail } from '../types/api'
import {
  addDaysToDateInput,
  dateInputToEndIso,
  dateInputToStartIso,
  nightsBetweenDateInputs,
  todayDateInputValue,
} from '../utils/dates'
import { formatCurrency } from '../utils/format'
import { capacitySummary, listingSubtitle } from '../utils/listingPresentation'

export function ApartmentDetailPage() {
  const navigate = useNavigate()
  const { id } = useParams<{ id: string }>()
  const { token, isClient } = useAuth()
  const [apartment, setApartment] = useState<ApartmentDetail | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [notFound, setNotFound] = useState(false)
  const [checkIn, setCheckIn] = useState(() => addDaysToDateInput(todayDateInputValue(), 1))
  const [checkOut, setCheckOut] = useState(() => addDaysToDateInput(todayDateInputValue(), 3))
  const [showReserve, setShowReserve] = useState(false)
  const [reserveError, setReserveError] = useState<string | null>(null)

  const dateRangeValid = useMemo(
    () => Boolean(checkIn && checkOut) && nightsBetweenDateInputs(checkIn, checkOut) > 0,
    [checkIn, checkOut],
  )

  const nights = dateRangeValid ? nightsBetweenDateInputs(checkIn, checkOut) : 0
  const total = apartment && nights > 0 ? nights * apartment.pricePerNight : 0

  const load = useCallback(async () => {
    if (!id) return
    setIsLoading(true)
    setError(null)
    setNotFound(false)
    try {
      const from = dateRangeValid ? dateInputToStartIso(checkIn) : undefined
      const to = dateRangeValid ? dateInputToEndIso(checkOut) : undefined
      const data = await getApartment(id, { from, to })
      setApartment(data)
    } catch (err) {
      setApartment(null)
      if (err instanceof ApiError && err.status === 404) {
        setNotFound(true)
      } else if (err instanceof ApiError) {
        setError(err.message)
      } else {
        setError('Failed to load apartment.')
      }
    } finally {
      setIsLoading(false)
    }
  }, [id, checkIn, checkOut, dateRangeValid])

  useEffect(() => {
    void load()
  }, [load])

  const canReserve =
    isClient &&
    dateRangeValid &&
    apartment &&
    (apartment.isAvailable === undefined ||
      apartment.isAvailable === null ||
      apartment.isAvailable)

  const handleReserve = (event: FormEvent) => {
    event.preventDefault()
    if (!token || !apartment || !dateRangeValid) return
    setShowReserve(false)
    navigate('/bookings/pay', {
      state: {
        apartmentId: apartment.id,
        apartmentVersion: apartment.version,
        apartmentName: apartment.name,
        city: apartment.city,
        thumbnailUrl: apartment.thumbnailUrl,
        pricePerNight: apartment.pricePerNight,
        guestCount: apartment.guestCount,
        checkIn,
        checkOut,
        start: dateInputToStartIso(checkIn),
        end: dateInputToEndIso(checkOut),
      },
    })
  }

  if (!id) {
    return (
      <section className="panel">
        <p className="status-message">Invalid apartment link.</p>
        <Link to="/apartments">Back to catalog</Link>
      </section>
    )
  }

  if (notFound) {
    return (
      <section className="panel">
        <div className="panel-header">
          <div>
            <p className="eyebrow">Listing</p>
            <h1>Apartment not found</h1>
            <p className="panel-lead">This listing may have been removed or the link is incorrect.</p>
          </div>
        </div>
        <Link to="/apartments" className="btn btn-primary">
          Browse apartments
        </Link>
      </section>
    )
  }

  const subtitle = apartment ? listingSubtitle(apartment) : null

  return (
    <section className="panel apartment-detail">
      <p className="eyebrow">
        <Link to="/apartments">← Stays</Link>
      </p>

      {isLoading && !apartment && <p className="status-message">Loading apartment…</p>}

      {error && (
        <div className="alert alert-error" role="alert">
          {error}
        </div>
      )}

      {apartment && (
        <>
          <ApartmentGallery
            apartmentId={apartment.id}
            imageUrls={apartment.imageUrls}
            thumbnailUrl={apartment.thumbnailUrl}
          />

          <div className="apartment-detail-layout">
            <div className="apartment-detail-main">
              <header className="apartment-detail-header">
                <h1>{apartment.name}</h1>
                {subtitle && <p className="apartment-detail-subtitle">{subtitle}</p>}
                <p className="apartment-detail-rating">New · No reviews yet</p>
                <div className="apartment-detail-badges">
                  {apartment.isAvailable !== undefined && apartment.isAvailable !== null && (
                    <span
                      className={`badge apartment-detail-availability ${
                        apartment.isAvailable ? 'badge-success' : 'badge-muted'
                      }`}
                    >
                      {apartment.isAvailable ? 'Available' : 'Booked for selected dates'}
                    </span>
                  )}
                </div>
                <p className="apartment-detail-capacity">{capacitySummary(apartment)}</p>
              </header>

              {apartment.highlights && apartment.highlights.length > 0 && (
                <section className="apartment-detail-section">
                  <h2>Highlights</h2>
                  <ul className="apartment-detail-highlights">
                    {apartment.highlights.map((highlight) => (
                      <li key={highlight}>{highlight}</li>
                    ))}
                  </ul>
                </section>
              )}

              <section className="apartment-detail-section">
                <h2>About this place</h2>
                <p className="apartment-detail-desc">{apartment.description || '—'}</p>
              </section>

              <section className="apartment-detail-section">
                <h2>What this place offers</h2>
                <AmenityTags amenities={apartment.amenities} />
              </section>

              <section className="apartment-detail-section">
                <h2>Where you&apos;ll be</h2>
                <p className="apartment-detail-location">{apartment.city}</p>
                <div className="apartment-detail-map-placeholder" aria-hidden="true">
                  <span>Map preview (MVP)</span>
                </div>
              </section>

              <section className="apartment-detail-section">
                <h2>Things to know</h2>
                <ul className="apartment-detail-things">
                  <li>Check-in and check-out times are flexible for this demo.</li>
                  <li>Self check-in may be available when listed in highlights.</li>
                  <li>Cancellation policies are not configured in this MVP.</li>
                </ul>
              </section>
            </div>

            <ApartmentBookingCard
              pricePerNight={apartment.pricePerNight}
              checkIn={checkIn}
              checkOut={checkOut}
              onCheckInChange={setCheckIn}
              onCheckOutChange={setCheckOut}
              minCheckOut={checkIn ? addDaysToDateInput(checkIn, 1) : undefined}
              dateRangeValid={dateRangeValid}
              nights={nights}
              total={total}
              isClient={isClient}
              canReserve={Boolean(canReserve)}
              isLoading={isLoading}
              onReserveClick={() => {
                setShowReserve(true)
                setReserveError(null)
              }}
            />
          </div>
        </>
      )}

      {showReserve && apartment && isClient && (
        <div
          className="modal-backdrop"
          role="presentation"
          onClick={() => setShowReserve(false)}
        >
          <div
            className="modal"
            role="dialog"
            aria-labelledby="reserve-title"
            onClick={(e) => e.stopPropagation()}
          >
            <h2 id="reserve-title">Reserve {apartment.name}</h2>
            <p className="panel-lead">
              {nights} {nights === 1 ? 'night' : 'nights'} · {formatCurrency(total)} total
            </p>
            <form className="auth-form" onSubmit={(e) => void handleReserve(e)}>
              {reserveError && (
                <div className="alert alert-error" role="alert">
                  {reserveError}
                </div>
              )}
              <label className="field">
                <span>Check-in</span>
                <input
                  type="date"
                  value={checkIn}
                  onChange={(e) => setCheckIn(e.target.value)}
                  required
                />
              </label>
              <label className="field">
                <span>Check-out</span>
                <input
                  type="date"
                  value={checkOut}
                  min={checkIn ? addDaysToDateInput(checkIn, 1) : undefined}
                  onChange={(e) => setCheckOut(e.target.value)}
                  required
                />
              </label>
              <div className="modal-actions">
                <button type="button" className="btn btn-secondary" onClick={() => setShowReserve(false)}>
                  Cancel
                </button>
                <button type="submit" className="btn btn-primary" disabled={!dateRangeValid}>
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
