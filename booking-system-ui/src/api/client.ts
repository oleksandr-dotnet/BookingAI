import { ApiError, type ValidationProblemDetails } from '../types/api'

const baseUrl = import.meta.env.VITE_API_URL ?? ''

export function getApiBaseUrl(): string {
  return baseUrl.replace(/\/$/, '')
}

export async function apiFetch<T>(
  path: string,
  options: RequestInit = {},
  token?: string | null,
): Promise<T> {
  const headers = new Headers(options.headers)
  if (!headers.has('Content-Type') && options.body) {
    headers.set('Content-Type', 'application/json')
  }
  if (token) {
    headers.set('Authorization', `Bearer ${token}`)
  }

  const response = await fetch(`${getApiBaseUrl()}${path}`, {
    ...options,
    headers,
  })

  if (response.ok) {
    if (response.status === 204) {
      return undefined as T
    }
    const text = await response.text()
    if (!text) {
      return undefined as T
    }
    return JSON.parse(text) as T
  }

  const contentType = response.headers.get('content-type') ?? ''
  if (contentType.includes('application/json') || contentType.includes('application/problem+json')) {
    const problem = (await response.json()) as ValidationProblemDetails
    const errors = problem.errors
    const message =
      problem.title ??
      (errors ? Object.values(errors).flat().join(' ') : `Request failed (${response.status})`)
    throw new ApiError(message, response.status, errors)
  }

  throw new ApiError(`Request failed (${response.status})`, response.status)
}
