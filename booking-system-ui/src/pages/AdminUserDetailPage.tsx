import { useCallback, useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import {
  getAdminUserById,
  listAdminUserBookings,
  lockAdminUser,
  setAdminUserRoles,
  unlockAdminUser,
} from '../api/adminUsers'
import { UserAvatar } from '../components/UserAvatar'
import { useAuth } from '../context/AuthContext'
import { ApiError, type AdminUserDetail, type BookingResponse } from '../types/api'
type DetailTab = 'profile' | 'bookings'

const ALL_ROLES = ['Admin', 'Host', 'Client'] as const

export function AdminUserDetailPage() {
  const { userId } = useParams<{ userId: string }>()
  const { token } = useAuth()
  const [tab, setTab] = useState<DetailTab>('profile')
  const [user, setUser] = useState<AdminUserDetail | null>(null)
  const [bookings, setBookings] = useState<BookingResponse[]>([])
  const [roleDraft, setRoleDraft] = useState<string[]>([])
  const [notFound, setNotFound] = useState(false)
  const [isLoading, setIsLoading] = useState(true)
  const [bookingsLoading, setBookingsLoading] = useState(false)
  const [actionLoading, setActionLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [actionMessage, setActionMessage] = useState<string | null>(null)

  const loadProfile = useCallback(async () => {
    if (!token || !userId) return
    setIsLoading(true)
    setError(null)
    setNotFound(false)
    try {
      const data = await getAdminUserById(token, userId)
      setUser(data)
      setRoleDraft([...data.roles])
    } catch (err) {
      setUser(null)
      if (err instanceof ApiError && err.status === 404) setNotFound(true)
      else if (err instanceof ApiError) setError(err.message)
      else setError('Failed to load user profile.')
    } finally {
      setIsLoading(false)
    }
  }, [token, userId])

  const loadBookings = useCallback(async () => {
    if (!token || !userId || notFound) return
    setBookingsLoading(true)
    try {
      const data = await listAdminUserBookings(token, userId)
      setBookings(data)
    } catch (err) {
      if (err instanceof ApiError) setError(err.message)
      else setError('Failed to load bookings.')
    } finally {
      setBookingsLoading(false)
    }
  }, [token, userId, notFound])

  useEffect(() => {
    void loadProfile()
  }, [loadProfile])

  useEffect(() => {
    if (tab === 'bookings') void loadBookings()
  }, [tab, loadBookings])

  const isLocked = Boolean(user?.lockoutEnabled && user.lockoutEnd)

  async function handleLock() {
    if (!token || !userId || !window.confirm('Lock this account? The user will not be able to sign in.')) return
    setActionLoading(true)
    setActionMessage(null)
    setError(null)
    try {
      await lockAdminUser(token, userId)
      setActionMessage('Account locked.')
      await loadProfile()
    } catch (err) {
      if (err instanceof ApiError) setError(err.message)
      else setError('Failed to lock account.')
    } finally {
      setActionLoading(false)
    }
  }

  async function handleUnlock() {
    if (!token || !userId) return
    setActionLoading(true)
    setActionMessage(null)
    setError(null)
    try {
      await unlockAdminUser(token, userId)
      setActionMessage('Account unlocked.')
      await loadProfile()
    } catch (err) {
      if (err instanceof ApiError) setError(err.message)
      else setError('Failed to unlock account.')
    } finally {
      setActionLoading(false)
    }
  }

  async function handleSaveRoles() {
    if (!token || !userId || roleDraft.length === 0) return
    setActionLoading(true)
    setActionMessage(null)
    setError(null)
    try {
      const updated = await setAdminUserRoles(token, userId, { roles: roleDraft })
      setUser(updated)
      setRoleDraft([...updated.roles])
      setActionMessage('Roles updated.')
    } catch (err) {
      if (err instanceof ApiError) setError(err.message)
      else setError('Failed to update roles.')
    } finally {
      setActionLoading(false)
    }
  }

  function toggleRole(role: string) {
    setRoleDraft((prev) =>
      prev.includes(role) ? prev.filter((r) => r !== role) : [...prev, role]
    )
  }

  return (
    <>
      <p className="admin-back-link">
        <Link to="/admin/users">← Back to users</Link>
      </p>

      <div className="panel-header">
        <div>
          <p className="eyebrow">Admin · User</p>
          <h1>{user?.displayName ?? user?.email ?? 'User profile'}</h1>
        </div>
        {user && (
          <div className="admin-action-bar">
            {isLocked ? (
              <button
                type="button"
                className="btn btn-secondary"
                disabled={actionLoading}
                onClick={() => void handleUnlock()}
              >
                Unlock
              </button>
            ) : (
              <button
                type="button"
                className="btn btn-secondary"
                disabled={actionLoading}
                onClick={() => void handleLock()}
              >
                Lock account
              </button>
            )}
          </div>
        )}
      </div>

      <nav className="admin-subtabs" aria-label="User detail sections">
        <button
          type="button"
          className={tab === 'profile' ? 'admin-tab admin-tab-active' : 'admin-tab'}
          onClick={() => setTab('profile')}
        >
          Profile
        </button>
        <button
          type="button"
          className={tab === 'bookings' ? 'admin-tab admin-tab-active' : 'admin-tab'}
          onClick={() => setTab('bookings')}
        >
          Bookings
        </button>
      </nav>

      {actionMessage && (
        <div className="alert alert-success" role="status">
          {actionMessage}
        </div>
      )}

      {error && (
        <div className="alert alert-error" role="alert">
          {error}
        </div>
      )}

      {isLoading && <p className="status-message">Loading profile…</p>}

      {notFound && !isLoading && (
        <div className="alert alert-error" role="alert">
          User not found.
        </div>
      )}

      {!isLoading && !notFound && user && tab === 'profile' && (
        <>
          <div className="admin-profile-hero">
            <UserAvatar
              displayName={user.displayName}
              initials={user.displayName.slice(0, 2).toUpperCase()}
              profileImageUrl={user.profileImageUrl}
              size="lg"
            />
            <div>
              <p className="panel-lead">{user.email}</p>
              <div className="role-badges">
                {user.roles.map((role) => (
                  <span key={role} className={`role-badge role-badge-${role.toLowerCase()}`}>
                    {role}
                  </span>
                ))}
              </div>
            </div>
          </div>
          <dl className="admin-profile-grid">
            <div className="admin-profile-row">
              <dt>User ID</dt>
              <dd>
                <code>{user.userId}</code>
              </dd>
            </div>
            <div className="admin-profile-row">
              <dt>Email</dt>
              <dd>{user.email}</dd>
            </div>
            <div className="admin-profile-row">
              <dt>First name</dt>
              <dd>{user.firstName ?? '—'}</dd>
            </div>
            <div className="admin-profile-row">
              <dt>Last name</dt>
              <dd>{user.lastName ?? '—'}</dd>
            </div>
            <div className="admin-profile-row">
              <dt>Date of birth</dt>
              <dd>{user.dateOfBirth ? new Date(user.dateOfBirth).toLocaleDateString() : '—'}</dd>
            </div>
            <div className="admin-profile-row">
              <dt>Username</dt>
              <dd>{user.userName ?? '—'}</dd>
            </div>
            <div className="admin-profile-row">
              <dt>Email confirmed</dt>
              <dd>{user.emailConfirmed ? 'Yes' : 'No'}</dd>
            </div>
            <div className="admin-profile-row">
              <dt>Lockout</dt>
              <dd>
                {user.lockoutEnabled && user.lockoutEnd
                  ? `Locked until ${new Date(user.lockoutEnd).toLocaleString()}`
                  : user.lockoutEnabled
                    ? 'Lockout enabled'
                    : 'Not locked'}
              </dd>
            </div>
            {(user.sourceCompanyId || user.externalId) && (
              <>
                <div className="admin-profile-row">
                  <dt>Source company ID</dt>
                  <dd>
                    <code>{user.sourceCompanyId ?? '—'}</code>
                  </dd>
                </div>
                <div className="admin-profile-row">
                  <dt>External ID</dt>
                  <dd>
                    <code>{user.externalId ?? '—'}</code>
                  </dd>
                </div>
              </>
            )}
          </dl>

          <section className="admin-role-editor">
            <h2>Roles</h2>
            <div className="admin-role-checkboxes">
              {ALL_ROLES.map((role) => (
                <label key={role} className="checkbox-label">
                  <input
                    type="checkbox"
                    checked={roleDraft.includes(role)}
                    onChange={() => toggleRole(role)}
                    disabled={actionLoading}
                  />
                  <span className={`role-badge role-badge-${role.toLowerCase()}`}>{role}</span>
                </label>
              ))}
            </div>
            <button
              type="button"
              className="btn btn-primary"
              disabled={actionLoading || roleDraft.length === 0}
              onClick={() => void handleSaveRoles()}
            >
              Save roles
            </button>
            <p className="form-hint">Cannot remove the last Admin account.</p>
          </section>
        </>
      )}

      {!isLoading && !notFound && user && tab === 'bookings' && (
        <div className="data-table-panel">
          {bookingsLoading && <p className="status-message">Loading bookings…</p>}
          {!bookingsLoading && bookings.length === 0 && (
            <p className="status-message">This user has no bookings.</p>
          )}
          {!bookingsLoading && bookings.length > 0 && (
            <table className="data-table admin-users-table">
              <thead>
                <tr>
                  <th>Apartment</th>
                  <th>City</th>
                  <th>Dates</th>
                  <th>Price/night</th>
                </tr>
              </thead>
              <tbody>
                {bookings.map((b) => (
                  <tr key={b.id}>
                    <td>{b.apartmentName ?? b.apartmentId}</td>
                    <td>{b.city ?? '—'}</td>
                    <td>
                      {new Date(b.start).toLocaleDateString()} – {new Date(b.end).toLocaleDateString()}
                    </td>
                    <td>{b.pricePerNight}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      )}
    </>
  )
}
