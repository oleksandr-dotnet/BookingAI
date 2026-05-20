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
      {children}
    </article>
  )
}
