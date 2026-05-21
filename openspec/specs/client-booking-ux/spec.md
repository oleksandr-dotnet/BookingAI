# client-booking-ux Specification

## Purpose
TBD - created by archiving change apartment-photo-lightbox-all-photos. Update Purpose after archive.
## Requirements
### Requirement: Apartment detail image gallery navigation

The apartment detail page (`/apartments/:id`) SHALL render an `ApartmentGallery` (or equivalent) that displays photos from `imageUrls` with fallback to `thumbnailUrl` / placeholder when empty. When more than one photo is available, the gallery SHALL provide prev/next controls overlaid on the main image that, when activated, change the active photo and counter. Horizontal swipe (touch) and pointer drag on the main image area, thumbnail selection, keyboard Left/Right when the gallery is focused, and lightbox prev/next SHALL use the same `activeIndex`. The gallery SHALL show an image counter (e.g. `2 / 5`) when multiple photos exist. When only one photo is available, prev/next controls SHALL be hidden or disabled and swipe navigation SHALL have no effect. Activating **any** visible gallery photo (main hero or thumbnail in the strip) SHALL open a fullscreen lightbox at that photo's index, showing the image at the largest practical size within the viewport.

#### Scenario: Overlay arrow buttons change main photo

- **GIVEN** apartment detail has two or more gallery URLs
- **WHEN** user activates the prev or next overlay control on the main hero gallery (not only the thumbnail strip)
- **THEN** the main image updates to the previous or next photo and the counter reflects the new position

#### Scenario: Multiple photos with prev and next

- **GIVEN** apartment detail has two or more URLs in `imageUrls` (or combined with thumbnail per `galleryUrls` rules)
- **WHEN** user activates the next control on the main gallery
- **THEN** the main image shows the next photo and the counter updates

#### Scenario: Single photo hides navigation

- **GIVEN** apartment detail resolves to one gallery URL
- **WHEN** the detail page loads
- **THEN** prev/next controls are not shown or are disabled

#### Scenario: Swipe to change photo on touch

- **GIVEN** multiple gallery URLs
- **WHEN** user swipes horizontally on the main image region on a touch device
- **THEN** the gallery advances to the previous or next photo according to swipe direction

#### Scenario: Lightbox from main image

- **GIVEN** the detail gallery is visible
- **WHEN** user activates the main image (click or equivalent) without a qualifying drag/swipe gesture
- **THEN** a fullscreen overlay opens showing the current photo at larger size

#### Scenario: Lightbox from thumbnail

- **GIVEN** the detail gallery shows a thumbnail strip with multiple photos
- **WHEN** user activates a thumbnail for photo *n*
- **THEN** a fullscreen overlay opens showing photo *n* at larger size and the inline gallery active index is *n*

#### Scenario: Close lightbox

- **GIVEN** the lightbox is open
- **WHEN** user presses Escape or activates the backdrop
- **THEN** the overlay closes and the inline gallery remains on the same photo index

#### Scenario: Lightbox overlay arrows change photo

- **GIVEN** the lightbox is open with multiple photos
- **WHEN** user activates prev or next overlay controls inside the lightbox
- **THEN** the lightbox and inline gallery both show the same updated index

#### Scenario: Keyboard navigation when gallery focused

- **GIVEN** multiple photos and keyboard focus on the gallery region
- **WHEN** user presses ArrowLeft or ArrowRight
- **THEN** the active photo changes accordingly

#### Scenario: Host preview uses same gallery

- **GIVEN** a signed-in host opens `/apartments/:id` for their listing (e.g. via "View" on host apartments)
- **WHEN** the detail page loads
- **THEN** the same gallery and lightbox behavior applies as for a client on that route

### Requirement: Reserve flow routes through payment

On catalog and apartment detail, when a signed-in Client confirms valid check-in and check-out dates, the UI SHALL navigate to a payment page (`/bookings/pay` or equivalent) showing apartment summary, dates, night count, and total. The UI SHALL NOT call `POST /bookings` directly from reserve/book actions.

#### Scenario: Detail page continues to payment

- **GIVEN** Client is signed in on apartment detail with valid dates
- **WHEN** user confirms reserve
- **THEN** the app navigates to the payment page with summary state and does not show booking confirmation yet

#### Scenario: Catalog modal continues to payment

- **GIVEN** Client books from catalog modal with valid dates
- **WHEN** user confirms book
- **THEN** the app navigates to the payment page instead of creating a booking inline

### Requirement: Payment page starts Stripe Checkout

The payment page SHALL call `POST /bookings/checkout-session` with `apartmentId`, `apartmentVersion`, `start`, and `end`, then redirect the browser to the returned Stripe `url` when the user chooses to pay.

#### Scenario: Pay redirects to Stripe

- **WHEN** user activates pay on the payment page and the API returns 200 with `url`
- **THEN** the browser navigates to Stripe Checkout

#### Scenario: Validation error on payment page

- **WHEN** checkout-session returns 409 or 400
- **THEN** the payment page shows an error and does not redirect

### Requirement: Payment success shows booking

After Stripe success redirect, the UI SHALL load booking details via `GET /bookings/checkout-session/{sessionId}` (with polling if status is `open`) and then show the booking confirmation experience (same information as today's confirmation page).

#### Scenario: Success after webhook

- **GIVEN** user returns from Stripe to the success URL with `session_id`
- **WHEN** checkout session status becomes `complete` with a booking
- **THEN** the UI shows confirmation including booking id and dates

### Requirement: Payment failure or cancel shows no booking

When the user cancels Stripe Checkout or payment fails, the UI SHALL show that the booking was not created and offer **Try again** to return to the apartment (or catalog) to pick dates again.

#### Scenario: Cancelled checkout

- **WHEN** user lands on the cancel URL after abandoning Stripe
- **THEN** the UI states the booking was not created and shows Try again

#### Scenario: Failed or incomplete payment

- **WHEN** success page polling ends with session not complete or failed overlap at webhook
- **THEN** the UI states the booking was not created and shows Try again

### Requirement: Booking detail page

The Client UI SHALL provide route `/bookings/:id` protected for the Client role. The page SHALL display apartment image (first of `imageUrls` when present, else `imageUrl`), name, city, full booking id, stay dates, night count, total price, amenities, and a link back to My bookings (`/bookings`).

#### Scenario: Navigate from My bookings

- **WHEN** authenticated Client clicks a booking card on My bookings
- **THEN** the app navigates to `/bookings/{id}` and shows that booking's detail

#### Scenario: Booking not found in UI

- **WHEN** Client opens `/bookings/{id}` for an id they do not own or that does not exist
- **THEN** the page shows a not-found message and a link to My bookings

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

