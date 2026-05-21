import { useCallback, useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { listAdminUsers, type AdminUserRoleFilter } from '../api/adminUsers'
import { UserAvatar } from '../components/UserAvatar'
import { useAuth } from '../context/AuthContext'
import { ApiError } from '../types/api'
import type { AdminUserListItem } from '../types/api'
import { shortId } from '../utils/format'

export function AdminUsersPage() {
  const { token } = useAuth()
  const [roleFilter, setRoleFilter] = useState<AdminUserRoleFilter>('')
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)
  const [items, setItems] = useState<AdminUserListItem[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [pageSize, setPageSize] = useState(20)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    if (!token) return
    setIsLoading(true)
    setError(null)
    try {
      const data = await listAdminUsers(token, {
        role: roleFilter || undefined,
        search: search || undefined,
        page,
        pageSize,
        sort: 'email',
      })
      setItems(data.items)
      setTotalCount(data.totalCount)
      setPageSize(data.pageSize)
    } catch (err) {
      if (err instanceof ApiError) setError(err.message)
      else setError('Failed to load users.')
    } finally {
      setIsLoading(false)
    }
  }, [token, roleFilter, search, page, pageSize])

  useEffect(() => {
    void load()
  }, [load])

  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize))

  return (
    <>
      <div className="panel-header">
        <div>
          <p className="eyebrow">Admin</p>
          <h1>Users</h1>
          <p className="panel-lead">Manage accounts, roles, and lockout. Open a user for bookings and actions.</p>
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
        <label className="field">
          <span>Role</span>
          <select
            value={roleFilter}
            onChange={(e) => setRoleFilter(e.target.value as AdminUserRoleFilter)}
          >
            <option value="">All roles</option>
            <option value="Admin">Admin</option>
            <option value="Host">Host</option>
            <option value="Client">Client</option>
          </select>
        </label>
        <label className="field field-grow">
          <span>Search email</span>
          <input
            type="search"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Filter by email…"
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

      {isLoading && <p className="status-message">Loading users…</p>}

      {!isLoading && !error && (
        <div className="data-table-panel">
          {items.length === 0 ? (
            <p className="status-message">No users match the current filters.</p>
          ) : (
            <table className="data-table admin-users-table">
              <thead>
                <tr>
                  <th>User</th>
                  <th>Email</th>
                  <th>Username</th>
                  <th>Roles</th>
                  <th>Bookings</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {items.map((user) => (
                  <tr key={user.userId}>
                    <td>
                      <Link to={`/admin/users/${user.userId}`} className="admin-user-link admin-user-cell">
                        <UserAvatar
                          displayName={user.displayName}
                          initials={user.displayName.slice(0, 2).toUpperCase()}
                          profileImageUrl={user.profileImageUrl}
                          size="sm"
                        />
                        <span>
                          <strong>{user.displayName}</strong>
                          <br />
                          <code className="admin-user-id-sub">{shortId(user.userId)}</code>
                        </span>
                      </Link>
                    </td>
                    <td>
                      <Link to={`/admin/users/${user.userId}`} className="admin-user-link">
                        {user.email}
                      </Link>
                    </td>
                    <td>
                      <Link to={`/admin/users/${user.userId}`} className="admin-user-link">
                        {user.userName ?? '—'}
                      </Link>
                    </td>
                    <td>
                      <div className="role-badges">
                        {user.roles.map((role) => (
                          <span key={role} className={`role-badge role-badge-${role.toLowerCase()}`}>
                            {role}
                          </span>
                        ))}
                      </div>
                    </td>
                    <td>{user.bookingCount}</td>
                    <td className="admin-user-status">
                      {user.lockoutEnabled && user.lockoutEnd
                        ? 'Locked'
                        : user.emailConfirmed
                          ? 'Active'
                          : 'Unconfirmed'}
                    </td>
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
                Page {page} of {totalPages} ({totalCount} users)
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
