INSERT INTO "Apartments" (
    "Id",
    "HostId",
    "Name",
    "Description",
    "PricePerNight",
    "GuestCount",
    "Amenities",
    "MetadataJson",
    "SourceCompanyId",
    "ExternalId")
VALUES (
    @Id,
    @HostId,
    @Name,
    @Description,
    @PricePerNight,
    @GuestCount,
    @Amenities,
    @MetadataJson::jsonb,
    @SourceCompanyId,
    @ExternalId)
ON CONFLICT ("Id") DO UPDATE SET
    "Name" = EXCLUDED."Name",
    "Description" = EXCLUDED."Description",
    "PricePerNight" = EXCLUDED."PricePerNight",
    "GuestCount" = EXCLUDED."GuestCount",
    "Amenities" = EXCLUDED."Amenities",
    "MetadataJson" = CASE WHEN @UpdateMetadata THEN EXCLUDED."MetadataJson" ELSE "Apartments"."MetadataJson" END,
    "SourceCompanyId" = EXCLUDED."SourceCompanyId",
    "ExternalId" = EXCLUDED."ExternalId"
WHERE "Apartments"."HostId" = @HostId
RETURNING
    "Id",
    "Name",
    "Description",
    "PricePerNight",
    "GuestCount",
    "Amenities",
    "MetadataJson";
