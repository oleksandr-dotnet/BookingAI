export const PROPERTY_TYPES = ['Apartment', 'House', 'Studio', 'Room'] as const

export type PropertyType = (typeof PROPERTY_TYPES)[number]

export interface ListingPresentationFields {
  propertyType?: string | null
  bedroomCount?: number | null
  bedCount?: number | null
  bathroomCount?: number | null
  highlights?: string[] | null
}

export function parsePresentationFromMetadata(
  metadata?: Record<string, unknown> | null,
): ListingPresentationFields {
  if (!metadata) return {}

  const propertyType =
    typeof metadata.propertyType === 'string' &&
    PROPERTY_TYPES.includes(metadata.propertyType as PropertyType)
      ? metadata.propertyType
      : undefined

  const bedroomCount = readCount(metadata.bedroomCount)
  const bedCount = readCount(metadata.bedCount)
  const bathroomCount = readCount(metadata.bathroomCount)

  const highlights = Array.isArray(metadata.highlights)
    ? metadata.highlights
        .filter((item): item is string => typeof item === 'string')
        .map((item) => item.trim())
        .filter((item) => item.length > 0)
        .slice(0, 5)
    : undefined

  return {
    propertyType,
    bedroomCount,
    bedCount,
    bathroomCount,
    highlights: highlights && highlights.length > 0 ? highlights : undefined,
  }
}

export function buildPresentationMetadata(
  fields: ListingPresentationFields,
  existingMetadata?: Record<string, unknown> | null,
): Record<string, unknown> | undefined {
  const base: Record<string, unknown> = { ...(existingMetadata ?? {}) }
  delete base.propertyType
  delete base.bedroomCount
  delete base.bedCount
  delete base.bathroomCount
  delete base.highlights

  if (fields.propertyType) base.propertyType = fields.propertyType
  if (fields.bedroomCount !== undefined && fields.bedroomCount !== null)
    base.bedroomCount = fields.bedroomCount
  if (fields.bedCount !== undefined && fields.bedCount !== null) base.bedCount = fields.bedCount
  if (fields.bathroomCount !== undefined && fields.bathroomCount !== null)
    base.bathroomCount = fields.bathroomCount
  if (fields.highlights && fields.highlights.length > 0) {
    base.highlights = fields.highlights.map((h) => h.trim()).filter(Boolean)
  }

  return Object.keys(base).length > 0 ? base : undefined
}

export function listingSubtitle(apartment: ListingPresentationFields & { city?: string }): string | null {
  if (!apartment.propertyType || !apartment.city) return apartment.city || null
  return `${apartment.propertyType} in ${apartment.city}`
}

export function capacitySummary(apartment: ListingPresentationFields & { guestCount: number }): string {
  const parts: string[] = []
  if (apartment.bedroomCount != null) {
    parts.push(`${apartment.bedroomCount} ${apartment.bedroomCount === 1 ? 'bedroom' : 'bedrooms'}`)
  }
  if (apartment.bedCount != null) {
    parts.push(`${apartment.bedCount} ${apartment.bedCount === 1 ? 'bed' : 'beds'}`)
  }
  if (apartment.bathroomCount != null) {
    parts.push(`${apartment.bathroomCount} ${apartment.bathroomCount === 1 ? 'bath' : 'baths'}`)
  }
  parts.push(`Up to ${apartment.guestCount} guests`)
  return parts.join(' · ')
}

function readCount(value: unknown): number | undefined {
  if (typeof value !== 'number' || !Number.isInteger(value) || value < 0 || value > 20) return undefined
  return value
}
