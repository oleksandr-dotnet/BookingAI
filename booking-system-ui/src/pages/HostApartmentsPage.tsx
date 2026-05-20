import { useCallback, useEffect, useState, type FormEvent } from 'react'
import { createHostApartment, listHostApartments } from '../api/hostApartments'
import { ApartmentCard } from '../components/ApartmentCard'
import { useAuth } from '../context/AuthContext'
import { ApiError, type ApartmentListItem, type ApartmentResponse } from '../types/api'

function toListItem(a: ApartmentResponse): ApartmentListItem {
  return { id: a.id, name: a.name, description: a.description }
}

export function HostApartmentsPage() {
  const { token } = useAuth()
  const [apartments, setApartments] = useState<ApartmentResponse[]>([])
  const [name, setName] = useState('')
  const [description, setDescription] = useState('')
  const [isLoading, setIsLoading] = useState(true)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [fieldErrors, setFieldErrors] = useState<Record<string, string[]> | undefined>()
  const [success, setSuccess] = useState<string | null>(null)

  const load = useCallback(async () => {
    if (!token) return
    setIsLoading(true)
    setError(null)
    try {
      setApartments(await listHostApartments(token))
    } catch (err) {
      setApartments([])
      if (err instanceof ApiError) setError(err.message)
      else setError('Failed to load your apartments.')
    } finally {
      setIsLoading(false)
    }
  }, [token])

  useEffect(() => {
    void load()
  }, [load])

  const handleCreate = async (event: FormEvent) => {
    event.preventDefault()
    if (!token) return
    setIsSubmitting(true)
    setError(null)
    setFieldErrors(undefined)
    setSuccess(null)
    try {
      await createHostApartment({ name, description }, token)
      setName('')
      setDescription('')
      setSuccess('Apartment created.')
      await load()
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.message)
        setFieldErrors(err.validationErrors)
      } else {
        setError('Failed to create apartment.')
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <section className="panel">
      <div className="panel-header">
        <div>
          <p className="eyebrow">Host</p>
          <h1>My apartments</h1>
          <p className="panel-lead">Create and manage apartments you offer for booking.</p>
        </div>
        <button type="button" className="btn btn-secondary" onClick={() => void load()} disabled={isLoading}>
          Refresh
        </button>
      </div>

      <div className="host-layout">
        <form className="auth-card host-form" onSubmit={(e) => void handleCreate(e)}>
          <h2>Add apartment</h2>
          {success && (
            <div className="alert alert-success" role="status">
              {success}
            </div>
          )}
          {error && (
            <div className="alert alert-error" role="alert">
              {error}
            </div>
          )}
          <label className="field">
            <span>Name</span>
            <input value={name} onChange={(e) => setName(e.target.value)} required maxLength={200} />
          </label>
          <label className="field">
            <span>Description</span>
            <textarea
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              rows={3}
              maxLength={2000}
            />
          </label>
          <button type="submit" className="btn btn-primary btn-block" disabled={isSubmitting}>
            {isSubmitting ? 'Creating…' : 'Create apartment'}
          </button>
          {fieldErrors && (
            <ul className="field-error-list">
              {Object.entries(fieldErrors).flatMap(([field, messages]) =>
                messages.map((message) => (
                  <li key={`${field}-${message}`}>
                    <strong>{field}</strong>: {message}
                  </li>
                )),
              )}
            </ul>
          )}
        </form>

        <div className="host-list">
          <h2>Your listings</h2>
          {isLoading && <p className="status-message">Loading…</p>}
          {!isLoading && apartments.length === 0 && (
            <p className="status-message">No apartments yet. Create one using the form.</p>
          )}
          <div className="apartment-grid">
            {apartments.map((apt) => (
              <ApartmentCard key={apt.id} apartment={toListItem(apt)} />
            ))}
          </div>
        </div>
      </div>
    </section>
  )
}
