DELETE FROM "Bookings" WHERE "ApartmentId" IN (
    SELECT "Id" FROM "Apartments" WHERE "SourceCompanyId" = '7c9e6679-7425-40de-944b-e07fc1f90ae7'
);
DELETE FROM "Apartments" WHERE "SourceCompanyId" = '7c9e6679-7425-40de-944b-e07fc1f90ae7';
DELETE FROM "AspNetUserRoles" WHERE "UserId" IN (
    SELECT "Id" FROM "AspNetUsers" WHERE "SourceCompanyId" = '7c9e6679-7425-40de-944b-e07fc1f90ae7'
);
DELETE FROM "AspNetUsers" WHERE "SourceCompanyId" = '7c9e6679-7425-40de-944b-e07fc1f90ae7';
