import type { FormEvent, ReactNode } from 'react'

interface AuthFormProps {
  title: string
  subtitle: string
  onSubmit: (event: FormEvent<HTMLFormElement>) => void
  isSubmitting: boolean
  error: string | null
  fieldErrors?: Record<string, string[]>
  children: ReactNode
  submitLabel: string
  footer?: ReactNode
}

export function AuthForm({
  title,
  subtitle,
  onSubmit,
  isSubmitting,
  error,
  fieldErrors,
  children,
  submitLabel,
  footer,
}: AuthFormProps) {
  return (
    <div className="auth-card">
      <div className="auth-card-header">
        <h1>{title}</h1>
        <p>{subtitle}</p>
      </div>
      <form className="auth-form" onSubmit={onSubmit} noValidate>
        {error && (
          <div className="alert alert-error" role="alert">
            {error}
          </div>
        )}
        {children}
        <button type="submit" className="btn btn-primary btn-block" disabled={isSubmitting}>
          {isSubmitting ? 'Please wait…' : submitLabel}
        </button>
      </form>
      {footer && <div className="auth-card-footer">{footer}</div>}
      {fieldErrors && Object.keys(fieldErrors).length > 0 && (
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
    </div>
  )
}
