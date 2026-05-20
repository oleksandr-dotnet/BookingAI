const ROLE_CLAIM =
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'

export function getRolesFromToken(token: string): string[] {
  try {
    const segment = token.split('.')[1]
    if (!segment) return []
    const json = atob(segment.replace(/-/g, '+').replace(/_/g, '/'))
    const payload = JSON.parse(json) as Record<string, unknown>
    const role = payload.role ?? payload[ROLE_CLAIM]
    if (Array.isArray(role)) return role.filter((r): r is string => typeof r === 'string')
    if (typeof role === 'string') return [role]
    return []
  } catch {
    return []
  }
}

export function getDefaultPathForRoles(roles: string[]): string {
  if (roles.includes('Admin')) return '/admin'
  if (roles.includes('Host')) return '/host'
  if (roles.includes('Client')) return '/apartments'
  return '/'
}
