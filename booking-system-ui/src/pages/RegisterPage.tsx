import { useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { login, register } from '../api/auth'
import { AuthForm } from '../components/AuthForm'
import { useAuth } from '../context/AuthContext'
import { ApiError, type UserRole } from '../types/api'
import { getDefaultPathForRoles, getRolesFromToken } from '../utils/jwt'

export function RegisterPage() {
  const { setToken } = useAuth()
  const navigate = useNavigate()

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [role, setRole] = useState<UserRole>('Client')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [fieldErrors, setFieldErrors] = useState<Record<string, string[]> | undefined>()

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setError(null)
    setFieldErrors(undefined)
    setIsSubmitting(true)

    try {
      await register({ email, password, role })
      const auth = await login({ email, password })
      setToken(auth.accessToken)
      navigate(getDefaultPathForRoles(getRolesFromToken(auth.accessToken)), { replace: true })
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.message)
        setFieldErrors(err.validationErrors)
      } else {
        setError('Unable to register. Check that the API is running.')
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <AuthForm
      title="Create account"
      subtitle="Choose Host to list apartments, or Client to browse and book."
      onSubmit={handleSubmit}
      isSubmitting={isSubmitting}
      error={error}
      fieldErrors={fieldErrors}
      submitLabel="Register"
      footer={
        <p>
          Already have an account? <Link to="/login">Sign in</Link>
        </p>
      }
    >
      <fieldset className="role-picker">
        <legend>Account type</legend>
        <label className="role-option">
          <input
            type="radio"
            name="role"
            value="Client"
            checked={role === 'Client'}
            onChange={() => setRole('Client')}
          />
          <span>
            <strong>Client</strong>
            <small>Browse apartments and make bookings</small>
          </span>
        </label>
        <label className="role-option">
          <input
            type="radio"
            name="role"
            value="Host"
            checked={role === 'Host'}
            onChange={() => setRole('Host')}
          />
          <span>
            <strong>Host</strong>
            <small>Create and manage your apartments</small>
          </span>
        </label>
      </fieldset>
      <label className="field">
        <span>Email</span>
        <input
          type="email"
          name="email"
          autoComplete="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          required
        />
      </label>
      <label className="field">
        <span>Password</span>
        <input
          type="password"
          name="password"
          autoComplete="new-password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
          minLength={6}
        />
      </label>
    </AuthForm>
  )
}
