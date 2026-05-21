import { useCallback, useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { listAdminBookings } from '../api/adminBookings'
import { useAuth } from '../context/AuthContext'
import { ApiError } from '../types/api'
import type { AdminBookingListItem } from '../types/api'
import { shortId } from '../utils/format'

export function AdminBookingsPage() {
  const { token } = useAuth()
  const [userIdFilter, setUserIdFilter] = useState('')
  const [page, setPage] = useState(1)
  const [items, setItems] = useState<AdminBookingListItem[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [pageSize, setPageSize] = useState(20)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    if (!token) return
    setIsLoading(true)
    setError(null)
    try {
      const data = await listAdminBookings(token, {
        userId: userIdFilter || undefined,
        page,
        pageSize,
      })
      setItems(data.items)
      setTotalCount(data.totalCount)
      setPageSize(data.pageSize)
    } catch (err) {
      if (err instanceof ApiError) setError(err.message)
      else setError('Failed to load bookings.')
    } finally {
      setIsLoading(false)
    }
  }, [token, userIdFilter, page, pageSize])

  useEffect(() => {
    void load()
  }, [load])

  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize))

  return (
    <>
      <div className="panel-header">
        <div>
          <p className="eyebrow">Admin</p>
          <h1>Bookings</h1>
          <p className="panel-lead">All reservations across the platform.</p>
        </div>
        <button type="button" className="btn btn-secondary" onClick={() => void load()} disabled={isLoading}>
          Refresh
        </button>
      </div>

      <form
        className="filter-bar admin-filters"
        onSubmit={(e) => {
          e.preventDefault()
          if (page !== 1) setPage(1)
          else void load()
        }}
      >
        <label className="field field-grow">
          <span>Filter by user ID</span>
          <input
            type="search"
            value={userIdFilter}
            onChange={(e) => setUserIdFilter(e.target.value)}
            placeholder="User ID (optional)…"
          />
        </label>
        <button type="submit" className="btn btn-primary" disabled={isLoading}>
          Apply
        </button>
      </form>

      {error && (
        <div className="alert alert-error" role="alert">
          {error}
        </div>
      )}

      {isLoading && <p className="status-message">Loading bookings…</p>}

      {!isLoading && !error && (
        <div className="data-table-panel">
          {items.length === 0 ? (
            <p className="status-message">No bookings match the current filters.</p>
          ) : (
            <table className="data-table admin-users-table">
              <thead>
                <tr>
                  <th>Booking</th>
                  <th>Guest</th>
                  <th>Apartment</th>
                  <th>Dates</th>
                  <th>Price/night</th>
                </tr>
              </thead>
              <tbody>
                {items.map((b) => (
                  <tr key={b.bookingId}>
                    <td>
                      <code>{shortId(b.bookingId)}</code>
                    </td>
                    <td>
                      <Link to={`/admin/users/${b.userId}`} className="admin-user-link">
                        {b.userEmail}
                      </Link>
                      <div className="admin-meta-line">
                        <code>{shortId(b.userId)}</code>
                      </div>
                    </td>
                    <td>
                      {b.apartmentName}
                      <div className="admin-meta-line">{b.city}</div>
                    </td>
                    <td>
                      {new Date(b.start).toLocaleDateString()} – {new Date(b.end).toLocaleDateString()}
                    </td>
                    <td>{b.pricePerNight}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}

          {totalCount > pageSize && (
            <div className="pagination-bar">
              <button
                type="button"
                className="btn btn-ghost btn-sm"
                disabled={page <= 1 || isLoading}
                onClick={() => setPage((p) => Math.max(1, p - 1))}
              >
                Previous
              </button>
              <span className="pagination-meta">
                Page {page} of {totalPages} ({totalCount} bookings)
              </span>
              <button
                type="button"
                className="btn btn-ghost btn-sm"
                disabled={page >= totalPages || isLoading}
                onClick={() => setPage((p) => p + 1)}
              >
                Next
              </button>
            </div>
          )}
        </div>
      )}
    </>
  )
}
