import type { ApartmentListItem } from '../types/api'

interface ApartmentCardProps {
  apartment: ApartmentListItem
  children?: React.ReactNode
}

export function ApartmentCard({ apartment, children }: ApartmentCardProps) {
  return (
    <article className="apartment-card">
      <div className="apartment-card-header">
        <h3>{apartment.name}</h3>
        {apartment.isAvailable !== undefined && apartment.isAvailable !== null && (
          <span
            className={`badge ${apartment.isAvailable ? 'badge-success' : 'badge-muted'}`}
          >
            {apartment.isAvailable ? 'Available' : 'Booked'}
          </span>
        )}
      </div>
      <p className="apartment-card-desc">{apartment.description || '—'}</p>
      <div className="apartment-card-meta">
        <span>${apartment.pricePerNight}/night</span>
        <span>·</span>
        <span>Up to {apartment.guestCount} guests</span>
      </div>
      {apartment.amenities.length > 0 && (
        <ul className="amenity-tags">
          {apartment.amenities.map((a) => (
            <li key={a}>{a}</li>
          ))}
        </ul>
      )}
      {children}
    </article>
  )
}
