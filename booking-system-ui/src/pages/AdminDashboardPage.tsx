import { useCallback, useEffect, useMemo, useState } from 'react'
import {
  getActiveHosts,
  getApartmentOccupancy,
  getBookingsByApartment,
  getBookingSummary,
  getPriceQuantiles,
} from '../api/analytics'
import { BarChart } from '../components/BarChart'
import { StatCard } from '../components/StatCard'
import { useAuth } from '../context/AuthContext'
import { ApiError } from '../types/api'
import { formatCurrency, formatNumber, shortId } from '../utils/format'

export function AdminDashboardPage() {
  const { token } = useAuth()
  const [minBookings, setMinBookings] = useState(1)
  const [minAvgNights, setMinAvgNights] = useState(0)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [summary, setSummary] = useState<Awaited<ReturnType<typeof getBookingSummary>> | null>(null)
  const [byApartment, setByApartment] = useState<Awaited<ReturnType<typeof getBookingsByApartment>>>([])
  const [activeHosts, setActiveHosts] = useState<Awaited<ReturnType<typeof getActiveHosts>>>([])
  const [quantiles, setQuantiles] = useState<Awaited<ReturnType<typeof getPriceQuantiles>> | null>(null)
  const [occupancy, setOccupancy] = useState<Awaited<ReturnType<typeof getApartmentOccupancy>>>([])

  const load = useCallback(async () => {
    if (!token) return
    setIsLoading(true)
    setError(null)
    try {
      const [summaryData, apartmentData, hostsData, quantileData, occupancyData] = await Promise.all([
        getBookingSummary(token),
        getBookingsByApartment(token),
        getActiveHosts(token, minBookings),
        getPriceQuantiles(token),
        getApartmentOccupancy(token, minAvgNights),
      ])
      setSummary(summaryData)
      setByApartment(apartmentData)
      setActiveHosts(hostsData)
      setQuantiles(quantileData)
      setOccupancy(occupancyData)
    } catch (err) {
      if (err instanceof ApiError) setError(err.message)
      else setError('Failed to load analytics.')
    } finally {
      setIsLoading(false)
    }
  }, [token, minBookings, minAvgNights])

  useEffect(() => {
    void load()
  }, [load])

  const apartmentChartItems = useMemo(
    () =>
      byApartment.slice(0, 12).map((row) => ({
        label: shortId(row.apartmentId),
        value: row.revenueSum,
      })),
    [byApartment],
  )

  const bookingCountChartItems = useMemo(
    () =>
      byApartment.slice(0, 12).map((row) => ({
        label: shortId(row.apartmentId),
        value: row.bookingCount,
      })),
    [byApartment],
  )

  return (
    <section className="panel admin-panel">
      <div className="panel-header">
        <div>
          <p className="eyebrow">Admin</p>
          <h1>Analytics dashboard</h1>
          <p className="panel-lead">
            Platform-wide statistics from SQL aggregates, GROUP BY, HAVING, and quantiles.
          </p>
        </div>
        <button type="button" className="btn btn-secondary" onClick={() => void load()} disabled={isLoading}>
          Refresh
        </button>
      </div>

      <form
        className="filter-bar admin-filters"
        onSubmit={(e) => {
          e.preventDefault()
          void load()
        }}
      >
        <label className="field">
          <span>Min bookings (hosts)</span>
          <input
            type="number"
            min={1}
            value={minBookings}
            onChange={(e) => setMinBookings(Math.max(1, Number(e.target.value) || 1))}
          />
        </label>
        <label className="field">
          <span>Min avg nights (occupancy)</span>
          <input
            type="number"
            min={0}
            step={0.1}
            value={minAvgNights}
            onChange={(e) => setMinAvgNights(Math.max(0, Number(e.target.value) || 0))}
          />
        </label>
        <button type="submit" className="btn btn-primary" disabled={isLoading}>
          Apply filters
        </button>
      </form>

      {error && (
        <div className="alert alert-error" role="alert">
          {error}
        </div>
      )}

      {isLoading && <p className="status-message">Loading analytics…</p>}

      {!isLoading && !error && summary && (
        <>
          <div className="stat-grid">
            <StatCard label="Total bookings" value={String(summary.totalBookings)} />
            <StatCard label="Total revenue" value={formatCurrency(summary.totalRevenue)} />
            <StatCard
              label="Avg price / night"
              value={formatCurrency(summary.averagePricePerNight)}
              hint="Across all bookings"
            />
          </div>

          {quantiles && (
            <div className="stat-grid stat-grid-3">
              <StatCard
                label="Price p25"
                value={quantiles.p25 != null ? formatCurrency(quantiles.p25) : '—'}
                hint="Apartment catalog"
              />
              <StatCard
                label="Price p50"
                value={quantiles.p50 != null ? formatCurrency(quantiles.p50) : '—'}
              />
              <StatCard
                label="Price p75"
                value={quantiles.p75 != null ? formatCurrency(quantiles.p75) : '—'}
              />
            </div>
          )}

          <div className="admin-charts">
            <BarChart
              title="Revenue by apartment (GROUP BY)"
              items={apartmentChartItems}
              valueFormatter={(v) => formatCurrency(v)}
              emptyMessage="No bookings recorded yet."
            />
            <BarChart
              title="Bookings per apartment"
              items={bookingCountChartItems}
              emptyMessage="No bookings recorded yet."
            />
          </div>

          <div className="admin-tables">
            <div className="data-table-panel">
              <h3>Active hosts (HAVING)</h3>
              {activeHosts.length === 0 ? (
                <p className="status-message">No hosts match the filter.</p>
              ) : (
                <table className="data-table">
                  <thead>
                    <tr>
                      <th>Host ID</th>
                      <th>Bookings</th>
                    </tr>
                  </thead>
                  <tbody>
                    {activeHosts.map((h) => (
                      <tr key={h.hostId}>
                        <td>
                          <code>{h.hostId}</code>
                        </td>
                        <td>{h.bookingCount}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </div>

            <div className="data-table-panel">
              <h3>Occupancy by apartment</h3>
              {occupancy.length === 0 ? (
                <p className="status-message">No apartments match the occupancy filter.</p>
              ) : (
                <table className="data-table">
                  <thead>
                    <tr>
                      <th>Apartment</th>
                      <th>Bookings</th>
                      <th>Avg nights</th>
                    </tr>
                  </thead>
                  <tbody>
                    {occupancy.map((row) => (
                      <tr key={row.apartmentId}>
                        <td>
                          <code>{shortId(row.apartmentId)}</code>
                        </td>
                        <td>{row.bookingCount}</td>
                        <td>{formatNumber(row.averageNightsBooked, 2)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </div>
          </div>
        </>
      )}
    </section>
  )
}
