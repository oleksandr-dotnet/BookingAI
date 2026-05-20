import { Link, Outlet, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { getDefaultPathForRoles } from '../utils/jwt'

export function Layout() {
  const { isAuthenticated, logout, isHost, isClient, roles } = useAuth()
  const navigate = useNavigate()

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  return (
    <div className="app-shell">
      <header className="app-header">
        <Link to="/" className="brand">
          <span className="brand-mark" aria-hidden />
          Booking System
        </Link>
        <nav className="app-nav">
          <Link to="/apartments">Apartments</Link>
          {isAuthenticated && isClient && <Link to="/bookings">My bookings</Link>}
          {isAuthenticated && isHost && <Link to="/host">My apartments</Link>}
          {isAuthenticated ? (
            <>
              <Link to={getDefaultPathForRoles(roles)} className="nav-account">
                Account
              </Link>
              <button type="button" className="btn btn-ghost" onClick={handleLogout}>
                Sign out
              </button>
            </>
          ) : (
            <>
              <Link to="/login">Sign in</Link>
              <Link to="/register" className="btn btn-primary btn-sm">
                Register
              </Link>
            </>
          )}
        </nav>
      </header>
      <main className="app-main">
        <Outlet />
      </main>
    </div>
  )
}
