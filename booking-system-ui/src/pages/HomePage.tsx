import { Link } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { getDefaultPathForRoles } from '../utils/jwt'

export function HomePage() {
  const { isAuthenticated, roles, isHost, isClient } = useAuth()

  return (
    <section className="hero">
      <p className="eyebrow">Booking System</p>
      <h1>Book apartments or host your own listings</h1>
      <p className="hero-lead">
        Hosts publish apartments. Clients browse availability, filter by dates, and reserve
        open time slots. Each account has one role: Host or Client.
      </p>
      <div className="hero-actions">
        <Link to="/apartments" className="btn btn-primary">
          Browse apartments
        </Link>
        {isAuthenticated ? (
          <Link to={getDefaultPathForRoles(roles)} className="btn btn-secondary">
            {isHost ? 'My apartments' : isClient ? 'My bookings' : 'My account'}
          </Link>
        ) : (
          <>
            <Link to="/register" className="btn btn-secondary">
              Register
            </Link>
            <Link to="/login" className="btn btn-ghost">
              Sign in
            </Link>
          </>
        )}
      </div>
      <div className="hero-cards">
        <article className="info-card">
          <h3>Host</h3>
          <p>Create apartments and see only your listings.</p>
        </article>
        <article className="info-card">
          <h3>Client</h3>
          <p>Search the catalog, check availability, and manage your bookings.</p>
        </article>
      </div>
    </section>
  )
}
