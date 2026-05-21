## ADDED Requirements

### Requirement: Airbnb-style catalog browse

The catalog page SHALL present a compact search row with location (city text filter client-side), check-in, check-out, and a search action that applies date filters. Listing cards SHALL show a subtitle `{propertyType} in {city}` when `propertyType` is present, a capacity summary when counts exist, and up to two highlight chips when `highlights` are present.

#### Scenario: Search row applies date filters

- **WHEN** user sets check-in and check-out and clicks Search
- **THEN** the page reloads apartments using `from` and `to` query parameters as today

#### Scenario: City filter narrows visible cards

- **WHEN** user types a city substring in the location field
- **THEN** the grid shows only apartments whose `city` contains the substring (case-insensitive) without an extra API call

### Requirement: Airbnb-style apartment detail layout

The apartment detail page SHALL use a two-column layout on wide viewports: main content (gallery, title, about, amenities, location placeholder, static things-to-know) and a sticky booking card with dates, nightly price, total, and Reserve. The page SHALL show static copy “New · No reviews yet” without calling a reviews API.

#### Scenario: Sticky booking card on desktop

- **WHEN** viewport width is at least 900px
- **THEN** the booking card remains visible while scrolling the main column

#### Scenario: Booking flow unchanged

- **WHEN** authenticated Client reserves from the sticky card
- **THEN** the app navigates to payment with the same state payload as before this change

### Requirement: Host listing presentation form

The host apartments create and edit forms SHALL include optional inputs for `propertyType`, bedroom/bed/bathroom counts, and up to five highlight strings, persisted via `metadata` on save.

#### Scenario: Host saves presentation fields

- **WHEN** host fills presentation fields and saves
- **THEN** a subsequent public detail view shows the projected fields
