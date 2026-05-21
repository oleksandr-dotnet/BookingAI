import { Link, Outlet, useNavigate } from 'react-router-dom'
import { ProfileCompletionBanner } from './ProfileCompletionBanner'
import { UserAvatar } from './UserAvatar'
import { useAuth } from '../context/AuthContext'
import { useProfile } from '../context/ProfileContext'

export function Layout() {
  const { isAuthenticated, logout, isHost, isClient, isAdmin } = useAuth()
  const { profile } = useProfile()
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
          {isAuthenticated && isAdmin && <Link to="/admin">Admin panel</Link>}
          {isAuthenticated ? (
            <>
              <Link to="/profile" className="nav-account nav-account-profile">
                {profile ? (
                  <>
                    <UserAvatar
                      displayName={profile.displayName}
                      initials={profile.initials}
                      profileImageUrl={profile.profileImageUrl}
                      size="sm"
                    />
                    <span>{profile.displayName}</span>
                  </>
                ) : (
                  'Profile'
                )}
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
        {isAuthenticated && <ProfileCompletionBanner />}
        <Outlet />
      </main>
    </div>
  )
}
