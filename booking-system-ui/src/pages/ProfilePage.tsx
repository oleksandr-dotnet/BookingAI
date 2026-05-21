import { useEffect, useState, type FormEvent } from 'react'
import { updateMyProfile } from '../api/profile'
import { ProfilePhotoUpload } from '../components/ProfilePhotoUpload'
import { UserAvatar } from '../components/UserAvatar'
import { useAuth } from '../context/AuthContext'
import { useProfile } from '../context/ProfileContext'
import { ApiError } from '../types/api'
import { cloudinaryConfigFromEnv, type CloudinaryConfig } from '../utils/cloudinary'

export function ProfilePage() {
  const { token, isClient, isHost } = useAuth()
  const { profile, refreshProfile, setProfile } = useProfile()
  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [dateOfBirth, setDateOfBirth] = useState('')
  const [profileImageUrl, setProfileImageUrl] = useState<string | null>(null)
  const [cloudinary, setCloudinary] = useState<CloudinaryConfig | null>(cloudinaryConfigFromEnv())
  const [isSaving, setIsSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)

  const requiresBirthDate = isClient || isHost

  useEffect(() => {
    if (!profile) return
    setFirstName(profile.firstName ?? '')
    setLastName(profile.lastName ?? '')
    setDateOfBirth(profile.dateOfBirth ?? '')
    setProfileImageUrl(profile.profileImageUrl)
  }, [profile])

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()
    if (!token) return
    setIsSaving(true)
    setError(null)
    setSuccess(null)
    try {
      const updated = await updateMyProfile(
        {
          firstName: firstName.trim(),
          lastName: lastName.trim(),
          dateOfBirth: dateOfBirth || null,
          profileImageUrl,
        },
        token,
      )
      setProfile(updated)
      setSuccess('Profile saved.')
    } catch (err) {
      if (err instanceof ApiError) setError(err.message)
      else setError('Failed to save profile.')
    } finally {
      setIsSaving(false)
    }
  }

  if (!profile) {
    return (
      <section className="panel">
        <p className="status-message">Loading profile…</p>
      </section>
    )
  }

  return (
    <section className="panel profile-page">
      <div className="panel-header">
        <div>
          <p className="eyebrow">Account</p>
          <h1>My profile</h1>
          <p className="panel-lead">Manage how you appear across bookings, hosting, and admin tools.</p>
        </div>
        <UserAvatar
          displayName={profile.displayName}
          initials={profile.initials}
          profileImageUrl={profileImageUrl}
          size="lg"
        />
      </div>

      {error && (
        <div className="alert alert-error" role="alert">
          {error}
        </div>
      )}
      {success && (
        <div className="alert alert-success" role="status">
          {success}
        </div>
      )}

      <form className="auth-card profile-form" onSubmit={(e) => void handleSubmit(e)}>
        <label className="field">
          <span>Email</span>
          <input type="email" value={profile.email} readOnly disabled />
        </label>

        <label className="field">
          <span>First name</span>
          <input
            value={firstName}
            onChange={(e) => setFirstName(e.target.value)}
            required
            maxLength={100}
            autoComplete="given-name"
          />
        </label>

        <label className="field">
          <span>Last name</span>
          <input
            value={lastName}
            onChange={(e) => setLastName(e.target.value)}
            required
            maxLength={100}
            autoComplete="family-name"
          />
        </label>

        <label className="field">
          <span>
            Date of birth
            {!requiresBirthDate && ' (optional)'}
          </span>
          <input
            type="date"
            value={dateOfBirth}
            onChange={(e) => setDateOfBirth(e.target.value)}
            required={requiresBirthDate}
          />
        </label>

        <div className="field">
          <span>Profile photo</span>
          <ProfilePhotoUpload
            imageUrl={profileImageUrl}
            onChange={setProfileImageUrl}
            cloudinary={cloudinary}
            disabled={isSaving}
          />
        </div>

        <div className="form-actions">
          <button type="submit" className="btn btn-primary" disabled={isSaving}>
            {isSaving ? 'Saving…' : 'Save profile'}
          </button>
          <button type="button" className="btn btn-ghost" disabled={isSaving} onClick={() => void refreshProfile()}>
            Reset
          </button>
        </div>
      </form>
    </section>
  )
}
