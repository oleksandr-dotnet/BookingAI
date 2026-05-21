import { useCallback, useEffect, useState, type FormEvent } from 'react'
import {
  createHostApartment,
  getHostImageUploadConfig,
  listHostApartments,
  updateHostApartment,
} from '../api/hostApartments'
import { AmenityCheckboxes } from '../components/AmenityCheckboxes'
import { ApartmentCard } from '../components/ApartmentCard'
import { ImageUploadField } from '../components/ImageUploadField'
import { ListingPresentationFieldsForm } from '../components/ListingPresentationFieldsForm'
import { useAuth } from '../context/AuthContext'
import { ApiError, type ApartmentListItem, type ApartmentResponse } from '../types/api'
import { cloudinaryConfigFromEnv, type CloudinaryConfig } from '../utils/cloudinary'
import {
  buildPresentationMetadata,
  parsePresentationFromMetadata,
} from '../utils/listingPresentation'

function toListItem(a: ApartmentResponse): ApartmentListItem {
  const presentation = parsePresentationFromMetadata(a.metadata ?? null)
  return {
    id: a.id,
    name: a.name,
    city: a.city,
    imageUrls: a.imageUrls,
    thumbnailUrl: a.thumbnailUrl,
    description: a.description,
    pricePerNight: a.pricePerNight,
    guestCount: a.guestCount,
    amenities: a.amenities,
    version: a.version,
    ...presentation,
  }
}

function readOptionalCount(value: string): number | undefined {
  const trimmed = value.trim()
  if (!trimmed) return undefined
  const parsed = Number(trimmed)
  return Number.isInteger(parsed) ? parsed : undefined
}

const defaultAmenities = ['Shower']

export function HostApartmentsPage() {
  const { token } = useAuth()
  const [apartments, setApartments] = useState<ApartmentResponse[]>([])
  const [cloudinary, setCloudinary] = useState<CloudinaryConfig | null>(cloudinaryConfigFromEnv())
  const [name, setName] = useState('')
  const [city, setCity] = useState('')
  const [imageUrls, setImageUrls] = useState<string[]>([])
  const [description, setDescription] = useState('')
  const [pricePerNight, setPricePerNight] = useState('100')
  const [guestCount, setGuestCount] = useState('2')
  const [amenities, setAmenities] = useState<string[]>(defaultAmenities)
  const [propertyType, setPropertyType] = useState('')
  const [bedroomCount, setBedroomCount] = useState('')
  const [bedCount, setBedCount] = useState('')
  const [bathroomCount, setBathroomCount] = useState('')
  const [highlights, setHighlights] = useState<string[]>([])
  const [editing, setEditing] = useState<ApartmentResponse | null>(null)
  const [editName, setEditName] = useState('')
  const [editCity, setEditCity] = useState('')
  const [editImageUrls, setEditImageUrls] = useState<string[]>([])
  const [editDescription, setEditDescription] = useState('')
  const [editPricePerNight, setEditPricePerNight] = useState('')
  const [editGuestCount, setEditGuestCount] = useState('')
  const [editAmenities, setEditAmenities] = useState<string[]>([])
  const [editPropertyType, setEditPropertyType] = useState('')
  const [editBedroomCount, setEditBedroomCount] = useState('')
  const [editBedCount, setEditBedCount] = useState('')
  const [editBathroomCount, setEditBathroomCount] = useState('')
  const [editHighlights, setEditHighlights] = useState<string[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [isUpdating, setIsUpdating] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [editError, setEditError] = useState<string | null>(null)
  const [fieldErrors, setFieldErrors] = useState<Record<string, string[]> | undefined>()
  const [editFieldErrors, setEditFieldErrors] = useState<Record<string, string[]> | undefined>()
  const [success, setSuccess] = useState<string | null>(null)

  useEffect(() => {
    if (!token || cloudinary) return
    void getHostImageUploadConfig(token)
      .then((config) => setCloudinary({ cloudName: config.cloudName, uploadPreset: config.uploadPreset }))
      .catch(() => undefined)
  }, [token, cloudinary])

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

  const openEdit = (apartment: ApartmentResponse) => {
    setEditing(apartment)
    setEditName(apartment.name)
    setEditCity(apartment.city)
    setEditImageUrls([...apartment.imageUrls])
    setEditDescription(apartment.description)
    setEditPricePerNight(String(apartment.pricePerNight))
    setEditGuestCount(String(apartment.guestCount))
    setEditAmenities([...apartment.amenities])
    const presentation = parsePresentationFromMetadata(apartment.metadata ?? null)
    setEditPropertyType(presentation.propertyType ?? '')
    setEditBedroomCount(
      presentation.bedroomCount != null ? String(presentation.bedroomCount) : '',
    )
    setEditBedCount(presentation.bedCount != null ? String(presentation.bedCount) : '')
    setEditBathroomCount(
      presentation.bathroomCount != null ? String(presentation.bathroomCount) : '',
    )
    setEditHighlights(presentation.highlights ? [...presentation.highlights] : [])
    setEditError(null)
    setEditFieldErrors(undefined)
  }

  const closeEdit = () => {
    setEditing(null)
    setEditError(null)
    setEditFieldErrors(undefined)
  }

  const handleCreate = async (event: FormEvent) => {
    event.preventDefault()
    if (!token) return
    setIsSubmitting(true)
    setError(null)
    setFieldErrors(undefined)
    setSuccess(null)
    try {
      const metadata = buildPresentationMetadata({
        propertyType: propertyType || undefined,
        bedroomCount: readOptionalCount(bedroomCount),
        bedCount: readOptionalCount(bedCount),
        bathroomCount: readOptionalCount(bathroomCount),
        highlights: highlights.map((h) => h.trim()).filter(Boolean),
      })
      await createHostApartment(
        {
          name,
          city,
          description,
          pricePerNight: Number(pricePerNight),
          guestCount: Number(guestCount),
          amenities,
          ...(imageUrls.length > 0 ? { imageUrls } : {}),
          ...(metadata ? { metadata } : {}),
        },
        token,
      )
      setName('')
      setCity('')
      setImageUrls([])
      setDescription('')
      setPricePerNight('100')
      setGuestCount('2')
      setAmenities(defaultAmenities)
      setPropertyType('')
      setBedroomCount('')
      setBedCount('')
      setBathroomCount('')
      setHighlights([])
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

  const handleUpdate = async (event: FormEvent) => {
    event.preventDefault()
    if (!token || !editing) return
    setIsUpdating(true)
    setEditError(null)
    setEditFieldErrors(undefined)
    setSuccess(null)
    try {
      const metadata = buildPresentationMetadata(
        {
          propertyType: editPropertyType || undefined,
          bedroomCount: readOptionalCount(editBedroomCount),
          bedCount: readOptionalCount(editBedCount),
          bathroomCount: readOptionalCount(editBathroomCount),
          highlights: editHighlights.map((h) => h.trim()).filter(Boolean),
        },
        editing.metadata ?? null,
      )
      await updateHostApartment(
        editing.id,
        {
          name: editName,
          city: editCity,
          description: editDescription,
          pricePerNight: Number(editPricePerNight),
          guestCount: Number(editGuestCount),
          amenities: editAmenities,
          version: editing.version,
          imageUrls: editImageUrls,
          ...(metadata ? { metadata } : {}),
        },
        token,
      )
      setSuccess('Apartment updated.')
      closeEdit()
      await load()
    } catch (err) {
      if (err instanceof ApiError) {
        if (err.status === 409) {
          setEditError('This listing was changed elsewhere. Close, refresh, and try again.')
          await load()
        } else {
          setEditError(err.message)
          setEditFieldErrors(err.validationErrors)
        }
      } else {
        setEditError('Failed to update apartment.')
      }
    } finally {
      setIsUpdating(false)
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

      {success && (
        <div className="alert alert-success" role="status">
          {success}
        </div>
      )}

      <div className="host-layout">
        <form className="auth-card host-form" onSubmit={(e) => void handleCreate(e)}>
          <h2>Add apartment</h2>
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
            <span>City</span>
            <input value={city} onChange={(e) => setCity(e.target.value)} required maxLength={100} />
          </label>
          <ImageUploadField
            label="Photos (optional)"
            imageUrls={imageUrls}
            onChange={setImageUrls}
            cloudinary={cloudinary}
            disabled={isSubmitting}
          />
          <label className="field">
            <span>Description</span>
            <textarea
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              rows={3}
              maxLength={2000}
            />
          </label>
          <label className="field">
            <span>Price per night</span>
            <input
              type="number"
              min={0}
              step={0.01}
              value={pricePerNight}
              onChange={(e) => setPricePerNight(e.target.value)}
              required
            />
          </label>
          <label className="field">
            <span>Guest capacity</span>
            <input
              type="number"
              min={1}
              value={guestCount}
              onChange={(e) => setGuestCount(e.target.value)}
              required
            />
          </label>
          <AmenityCheckboxes selected={amenities} onChange={setAmenities} disabled={isSubmitting} />
          <ListingPresentationFieldsForm
            propertyType={propertyType}
            bedroomCount={bedroomCount}
            bedCount={bedCount}
            bathroomCount={bathroomCount}
            highlights={highlights}
            onPropertyTypeChange={setPropertyType}
            onBedroomCountChange={setBedroomCount}
            onBedCountChange={setBedCount}
            onBathroomCountChange={setBathroomCount}
            onHighlightsChange={setHighlights}
            disabled={isSubmitting}
          />
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
              <ApartmentCard key={apt.id} apartment={toListItem(apt)}>
                <div className="apartment-card-actions">
                  <span className="badge badge-muted">v{apt.version}</span>
                  <button type="button" className="btn btn-secondary btn-sm" onClick={() => openEdit(apt)}>
                    Edit
                  </button>
                </div>
              </ApartmentCard>
            ))}
          </div>
        </div>
      </div>

      {editing && (
        <div className="modal-backdrop" role="presentation" onClick={closeEdit}>
          <div
            className="modal modal-wide"
            role="dialog"
            aria-labelledby="edit-apartment-title"
            onClick={(e) => e.stopPropagation()}
          >
            <h2 id="edit-apartment-title">Edit apartment</h2>
            <p className="panel-lead">
              Saving uses listing version {editing.version}. Clients booking with an older version will be asked to
              refresh.
            </p>
            <form className="auth-form" onSubmit={(e) => void handleUpdate(e)}>
              {editError && (
                <div className="alert alert-error" role="alert">
                  {editError}
                </div>
              )}
              <label className="field">
                <span>Name</span>
                <input value={editName} onChange={(e) => setEditName(e.target.value)} required maxLength={200} />
              </label>
              <label className="field">
                <span>City</span>
                <input value={editCity} onChange={(e) => setEditCity(e.target.value)} required maxLength={100} />
              </label>
              <ImageUploadField
                label="Photos"
                imageUrls={editImageUrls}
                onChange={setEditImageUrls}
                cloudinary={cloudinary}
                disabled={isUpdating}
              />
              <label className="field">
                <span>Description</span>
                <textarea
                  value={editDescription}
                  onChange={(e) => setEditDescription(e.target.value)}
                  rows={3}
                  maxLength={2000}
                />
              </label>
              <label className="field">
                <span>Price per night</span>
                <input
                  type="number"
                  min={0}
                  step={0.01}
                  value={editPricePerNight}
                  onChange={(e) => setEditPricePerNight(e.target.value)}
                  required
                />
              </label>
              <label className="field">
                <span>Guest capacity</span>
                <input
                  type="number"
                  min={1}
                  value={editGuestCount}
                  onChange={(e) => setEditGuestCount(e.target.value)}
                  required
                />
              </label>
              <AmenityCheckboxes selected={editAmenities} onChange={setEditAmenities} disabled={isUpdating} />
              <ListingPresentationFieldsForm
                propertyType={editPropertyType}
                bedroomCount={editBedroomCount}
                bedCount={editBedCount}
                bathroomCount={editBathroomCount}
                highlights={editHighlights}
                onPropertyTypeChange={setEditPropertyType}
                onBedroomCountChange={setEditBedroomCount}
                onBedCountChange={setEditBedCount}
                onBathroomCountChange={setEditBathroomCount}
                onHighlightsChange={setEditHighlights}
                disabled={isUpdating}
              />
              {editFieldErrors && (
                <ul className="field-error-list">
                  {Object.entries(editFieldErrors).flatMap(([field, messages]) =>
                    messages.map((message) => (
                      <li key={`edit-${field}-${message}`}>
                        <strong>{field}</strong>: {message}
                      </li>
                    )),
                  )}
                </ul>
              )}
              <div className="modal-actions">
                <button type="button" className="btn btn-secondary" onClick={closeEdit}>
                  Cancel
                </button>
                <button type="submit" className="btn btn-primary" disabled={isUpdating}>
                  {isUpdating ? 'Saving…' : 'Save changes'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </section>
  )
}
