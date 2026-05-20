import { AMENITY_OPTIONS } from '../types/api'

interface AmenityCheckboxesProps {
  selected: string[]
  onChange: (amenities: string[]) => void
  disabled?: boolean
}

export function AmenityCheckboxes({ selected, onChange, disabled }: AmenityCheckboxesProps) {
  const toggle = (value: string) => {
    if (selected.includes(value)) {
      onChange(selected.filter((a) => a !== value))
    } else {
      onChange([...selected, value])
    }
  }

  return (
    <fieldset className="amenity-fieldset" disabled={disabled}>
      <legend>Amenities</legend>
      <div className="amenity-grid">
        {AMENITY_OPTIONS.map((opt) => (
          <label key={opt.value} className="amenity-chip">
            <input
              type="checkbox"
              checked={selected.includes(opt.value)}
              onChange={() => toggle(opt.value)}
            />
            <span>{opt.label}</span>
          </label>
        ))}
      </div>
    </fieldset>
  )
}
