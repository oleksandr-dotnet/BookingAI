## Context

Identity users currently store email, username, and migration metadata only. Apartment photos already use client-side Cloudinary upload (`ImageUploadField`, `uploadImageToCloudinary`). Admin user APIs return email-centric list/detail without personal fields.

## Goals / Non-Goals

**Goals:**

- Persist profile fields on `ApplicationUser` with EF migration.
- Role-aware validation in Application `UserProfileService`.
- Thin API endpoints; reuse Cloudinary config endpoint for upload preset.
- Consistent `UserAvatar` in layout, admin users UI, and anywhere a user id is shown.
- `displayName` = trimmed `"${firstName} ${lastName}"` or email local-part fallback.

**Non-Goals:**

- Backend multipart upload.
- Email/password change on profile page.
- Cross-user profile editing by admins.

## Decisions

1. **Cloudinary URL storage** — Client uploads image; API stores `ProfileImageUrl` string only. Matches apartment photo flow; no new storage dependency.
2. **Birth date as `DateOnly?`** — Stored in UTC date; Client/Host must set on save; Admin may leave null.
3. **`GET /profile/me` vs JWT claims** — Profile data not embedded in JWT (keeps tokens small); UI loads profile after login and caches in context optional refresh on save.
4. **`GET /users/{userId}/display`** — Separate read model for referencing other users; returns 404 for unknown id; no PII beyond display card.
5. **Completeness** — `profileComplete` on own profile: Client/Host need first, last, birth date; Admin needs first and last only. UI shows banner when incomplete after login.
6. **Validation** — Max lengths: names 100 chars; image URL 2048; birth date not in future; minimum age 13 for Client/Host birth date.

## Risks / Trade-offs

- **[Stale avatar in JWT]** → Mitigation: never put image URL in token; always fetch display endpoint or profile/me.
- **[Large Cloudinary URLs]** → Mitigation: validate HTTPS and max length.
- **[Existing users empty profile]** → Mitigation: allow login; show completion banner; do not hard-block MVP routes except optional nudge on `/profile`.

## Migration Plan

1. Add EF migration for new columns (nullable).
2. Deploy API; existing users have null profile fields until they edit profile.
3. Deploy UI with profile page.

## Open Questions

None — defaults above are sufficient for MVP.
