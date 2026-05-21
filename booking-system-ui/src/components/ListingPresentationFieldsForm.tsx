import { PROPERTY_TYPES } from '../utils/listingPresentation'

interface ListingPresentationFieldsFormProps {
  propertyType: string
  bedroomCount: string
  bedCount: string
  bathroomCount: string
  highlights: string[]
  onPropertyTypeChange: (value: string) => void
  onBedroomCountChange: (value: string) => void
  onBedCountChange: (value: string) => void
  onBathroomCountChange: (value: string) => void
  onHighlightsChange: (value: string[]) => void
  disabled?: boolean
}

export function ListingPresentationFieldsForm({
  propertyType,
  bedroomCount,
  bedCount,
  bathroomCount,
  highlights,
  onPropertyTypeChange,
  onBedroomCountChange,
  onBedCountChange,
  onBathroomCountChange,
  onHighlightsChange,
  disabled,
}: ListingPresentationFieldsFormProps) {
  return (
    <fieldset className="listing-presentation-fields" disabled={disabled}>
      <legend>Listing details (optional)</legend>
      <label className="field">
        <span>Property type</span>
        <select value={propertyType} onChange={(e) => onPropertyTypeChange(e.target.value)}>
          <option value="">—</option>
          {PROPERTY_TYPES.map((type) => (
            <option key={type} value={type}>
              {type}
            </option>
          ))}
        </select>
      </label>
      <div className="listing-presentation-counts">
        <label className="field">
          <span>Bedrooms</span>
          <input
            type="number"
            min={0}
            max={20}
            value={bedroomCount}
            onChange={(e) => onBedroomCountChange(e.target.value)}
          />
        </label>
        <label className="field">
          <span>Beds</span>
          <input
            type="number"
            min={0}
            max={20}
            value={bedCount}
            onChange={(e) => onBedCountChange(e.target.value)}
          />
        </label>
        <label className="field">
          <span>Bathrooms</span>
          <input
            type="number"
            min={0}
            max={20}
            value={bathroomCount}
            onChange={(e) => onBathroomCountChange(e.target.value)}
          />
        </label>
      </div>
      {highlights.map((highlight, index) => (
        <label className="field" key={`highlight-${index}`}>
          <span>Highlight {index + 1}</span>
          <input
            type="text"
            maxLength={40}
            value={highlight}
            onChange={(e) => {
              const next = [...highlights]
              next[index] = e.target.value
              onHighlightsChange(next)
            }}
          />
        </label>
      ))}
      {highlights.length < 5 && (
        <button
          type="button"
          className="btn btn-secondary btn-sm"
          onClick={() => onHighlightsChange([...highlights, ''])}
        >
          Add highlight
        </button>
      )}
    </fieldset>
  )
}
