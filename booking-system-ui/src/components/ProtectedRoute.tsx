import { Navigate, useLocation } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

interface ProtectedRouteProps {
  children: React.ReactNode
  roles?: string[]
}

export function ProtectedRoute({ children, roles }: ProtectedRouteProps) {
  const { isAuthenticated, roles: userRoles } = useAuth()
  const location = useLocation()

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />
  }

  if (roles && !roles.some((role) => userRoles.includes(role))) {
    return (
      <section className="panel">
        <div className="alert alert-error" role="alert">
          You do not have permission to view this page.
        </div>
      </section>
    )
  }

  return children
}
