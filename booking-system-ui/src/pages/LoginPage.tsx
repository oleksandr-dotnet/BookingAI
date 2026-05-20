import { useState, type FormEvent } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { AuthForm } from '../components/AuthForm'
import { useAuth } from '../context/AuthContext'
import { ApiError } from '../types/api'
import { getDefaultPathForRoles } from '../utils/jwt'

export function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const from = (location.state as { from?: string } | null)?.from

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [fieldErrors, setFieldErrors] = useState<Record<string, string[]> | undefined>()

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setError(null)
    setFieldErrors(undefined)
    setIsSubmitting(true)

    try {
      const roles = await login({ email, password })
      const target = from && from !== '/login' && from !== '/register' ? from : getDefaultPathForRoles(roles)
      navigate(target, { replace: true })
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.message)
        setFieldErrors(err.validationErrors)
      } else {
        setError('Unable to sign in. Check that the API is running.')
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <AuthForm
      title="Welcome back"
      subtitle="Sign in with your email and password."
      onSubmit={handleSubmit}
      isSubmitting={isSubmitting}
      error={error}
      fieldErrors={fieldErrors}
      submitLabel="Sign in"
      footer={
        <p>
          No account? <Link to="/register">Register</Link>
        </p>
      }
    >
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
          autoComplete="current-password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
        />
      </label>
    </AuthForm>
  )
}
