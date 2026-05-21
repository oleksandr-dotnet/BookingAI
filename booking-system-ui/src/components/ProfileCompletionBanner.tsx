import { Link } from 'react-router-dom'
import { useProfile } from '../context/ProfileContext'

export function ProfileCompletionBanner() {
  const { profile, isLoading } = useProfile()

  if (isLoading || !profile || profile.profileComplete) {
    return null
  }

  return (
    <div className="alert alert-info profile-completion-banner" role="status">
      <span>Complete your profile so others can recognize you across the app.</span>
      <Link to="/profile" className="btn btn-primary btn-sm">
        Go to profile
      </Link>
    </div>
  )
}
