## ADDED Requirements

### Requirement: Public listing presentation fields

The system SHALL project optional presentation fields on each apartment in `GET /apartments` and `GET /apartments/{id}` responses: `propertyType` (string), `bedroomCount`, `bedCount`, `bathroomCount` (integers), and `highlights` (string array). Values SHALL be read from apartment `metadata` using keys `propertyType`, `bedroomCount`, `bedCount`, `bathroomCount`, and `highlights`. The public responses MUST NOT include the raw `metadata` object.

#### Scenario: Presentation fields when metadata is set

- **WHEN** an apartment has metadata `{"propertyType":"Apartment","bedroomCount":2,"bedCount":3,"bathroomCount":1,"highlights":["Self check-in"]}`
- **THEN** public list and detail items include matching `propertyType`, counts, and `highlights`

#### Scenario: Omitted presentation when metadata empty

- **WHEN** apartment metadata is `{}` or lacks presentation keys
- **THEN** public responses omit presentation properties or return null/empty per DTO convention without error

### Requirement: Presentation metadata validation on host write

The system SHALL validate presentation keys when hosts create or update apartments with a `metadata` object. `propertyType` MUST be one of `Apartment`, `House`, `Studio`, `Room` when present. `bedroomCount`, `bedCount`, and `bathroomCount` MUST be integers from 0 through 20 when present. `highlights` MUST be an array of at most 5 strings, each at most 40 characters after trim.

#### Scenario: Invalid property type rejected

- **WHEN** host sets `metadata.propertyType` to `Castle`
- **THEN** the system returns 400 Bad Request

#### Scenario: Too many highlights rejected

- **WHEN** host sends more than five highlight strings
- **THEN** the system returns 400 Bad Request

#### Scenario: Valid presentation metadata accepted

- **WHEN** host sets valid presentation keys within limits
- **THEN** the system persists metadata and subsequent public reads project the values
