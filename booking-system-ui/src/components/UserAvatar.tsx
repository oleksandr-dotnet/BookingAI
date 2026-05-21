interface UserAvatarProps {
  displayName?: string
  initials: string
  profileImageUrl?: string | null
  size?: 'sm' | 'md' | 'lg'
}

const sizeClass: Record<NonNullable<UserAvatarProps['size']>, string> = {
  sm: 'user-avatar-sm',
  md: 'user-avatar-md',
  lg: 'user-avatar-lg',
}

export function UserAvatar({
  displayName,
  initials,
  profileImageUrl,
  size = 'md',
}: UserAvatarProps) {
  const label = displayName ?? initials
  const className = `user-avatar ${sizeClass[size]}`

  if (profileImageUrl?.trim()) {
    return (
      <img
        src={profileImageUrl}
        alt=""
        className={className}
        title={label}
      />
    )
  }

  return (
    <span className={className} title={label} aria-hidden={!displayName}>
      {initials}
    </span>
  )
}
