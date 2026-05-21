import { useState } from 'react'
import { Link } from 'react-router-dom'
import type { ApartmentListItem } from '../types/api'
import { defaultApartmentImageUrl } from '../utils/apartmentImages'
import { formatCurrency } from '../utils/format'
import { capacitySummary, listingSubtitle } from '../utils/listingPresentation'

interface ApartmentCardProps {
  apartment: ApartmentListItem
  children?: React.ReactNode
}

export function ApartmentCard({ apartment, children }: ApartmentCardProps) {
  const detailPath = `/apartments/${apartment.id}`
  const [thumbSrc, setThumbSrc] = useState(
    apartment.thumbnailUrl?.trim() || defaultApartmentImageUrl(apartment.id),
  )
  const subtitle = listingSubtitle(apartment)
  const capacity = capacitySummary(apartment)
  const topHighlights = apartment.highlights?.slice(0, 2) ?? []

  return (
    <article className="apartment-card">
      <Link to={detailPath} className="apartment-card-image-link">
        <img
          src={thumbSrc}
          alt=""
          className="apartment-card-image"
          loading="lazy"
          onError={() => setThumbSrc(defaultApartmentImageUrl(apartment.id))}
        />
      </Link>
      <div className="apartment-card-body">
        <div className="apartment-card-header">
          <h3>
            <Link to={detailPath} className="apartment-card-title">
              {apartment.name}
            </Link>
          </h3>
          {apartment.isAvailable !== undefined && apartment.isAvailable !== null && (
            <span
              className={`badge ${apartment.isAvailable ? 'badge-success' : 'badge-muted'}`}
            >
              {apartment.isAvailable ? 'Available' : 'Booked'}
            </span>
          )}
        </div>
        {subtitle && <p className="apartment-card-subtitle">{subtitle}</p>}
        <p className="apartment-card-meta">
          <span>{formatCurrency(apartment.pricePerNight)}</span>
          <span> night</span>
          <span className="apartment-card-meta-sep"> · </span>
          <span>{capacity}</span>
        </p>
        {topHighlights.length > 0 && (
          <ul className="apartment-card-highlights">
            {topHighlights.map((highlight) => (
              <li key={highlight}>{highlight}</li>
            ))}
          </ul>
        )}
        <p className="apartment-card-desc">{apartment.description || '—'}</p>
        <div className="apartment-card-actions">
          <Link to={detailPath} className="btn btn-secondary btn-sm">
            View
          </Link>
          {children}
        </div>
      </div>
    </article>
  )
}
