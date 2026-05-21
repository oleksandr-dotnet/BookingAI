import { NavLink, Outlet } from 'react-router-dom'

export function AdminLayout() {
  return (
    <section className="panel admin-panel">
      <nav className="admin-tabs" aria-label="Admin sections">
        <NavLink
          to="/admin/analytics"
          className={({ isActive }) => (isActive ? 'admin-tab admin-tab-active' : 'admin-tab')}
          end
        >
          Analytics
        </NavLink>
        <NavLink
          to="/admin/users"
          className={({ isActive }) => (isActive ? 'admin-tab admin-tab-active' : 'admin-tab')}
        >
          Users
        </NavLink>
        <NavLink
          to="/admin/bookings"
          className={({ isActive }) => (isActive ? 'admin-tab admin-tab-active' : 'admin-tab')}
        >
          Bookings
        </NavLink>
      </nav>
      <Outlet />
    </section>
  )
}
