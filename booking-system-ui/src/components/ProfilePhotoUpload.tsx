import { useRef, useState, type ChangeEvent } from 'react'
import type { CloudinaryConfig } from '../utils/cloudinary'
import { uploadImageToCloudinary } from '../utils/cloudinary'

interface ProfilePhotoUploadProps {
  imageUrl: string | null
  onChange: (url: string | null) => void
  cloudinary: CloudinaryConfig | null
  disabled?: boolean
}

export function ProfilePhotoUpload({
  imageUrl,
  onChange,
  cloudinary,
  disabled = false,
}: ProfilePhotoUploadProps) {
  const inputRef = useRef<HTMLInputElement>(null)
  const [manualUrl, setManualUrl] = useState('')
  const [uploadError, setUploadError] = useState<string | null>(null)
  const [isUploading, setIsUploading] = useState(false)

  const handleFile = async (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0]
    if (!file) return
    setUploadError(null)

    if (!cloudinary) {
      setUploadError('Cloudinary is not configured.')
      event.target.value = ''
      return
    }

    setIsUploading(true)
    try {
      const url = await uploadImageToCloudinary(file, cloudinary)
      onChange(url)
    } catch (err) {
      setUploadError(err instanceof Error ? err.message : 'Upload failed.')
    } finally {
      setIsUploading(false)
      event.target.value = ''
    }
  }

  return (
    <div className="profile-photo-upload">
      <div className="profile-photo-preview">
        {imageUrl ? (
          <img src={imageUrl} alt="Profile preview" />
        ) : (
          <span className="profile-photo-placeholder">No photo</span>
        )}
      </div>
      <div className="profile-photo-actions">
        {cloudinary && (
          <>
            <input
              ref={inputRef}
              type="file"
              accept="image/*"
              hidden
              disabled={disabled || isUploading}
              onChange={(e) => void handleFile(e)}
            />
            <button
              type="button"
              className="btn btn-secondary btn-sm"
              disabled={disabled || isUploading}
              onClick={() => inputRef.current?.click()}
            >
              {isUploading ? 'Uploading…' : 'Upload photo'}
            </button>
          </>
        )}
        <div className="image-upload-manual">
          <input
            type="url"
            value={manualUrl}
            onChange={(e) => setManualUrl(e.target.value)}
            placeholder="Or paste HTTPS image URL"
            disabled={disabled}
          />
          <button
            type="button"
            className="btn btn-secondary btn-sm"
            disabled={disabled || !manualUrl.trim()}
            onClick={() => {
              onChange(manualUrl.trim())
              setManualUrl('')
            }}
          >
            Use URL
          </button>
        </div>
        {imageUrl && (
          <button
            type="button"
            className="btn btn-ghost btn-sm"
            disabled={disabled}
            onClick={() => onChange(null)}
          >
            Remove photo
          </button>
        )}
      </div>
      {uploadError && <p className="field-hint field-hint-error">{uploadError}</p>}
    </div>
  )
}
