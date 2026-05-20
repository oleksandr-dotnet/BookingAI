SELECT
    a."HostId",
    COUNT(b."Id")::int AS "BookingCount"
FROM "Bookings" b
INNER JOIN "Apartments" a ON a."Id" = b."ApartmentId"
GROUP BY a."HostId"
HAVING COUNT(b."Id") >= @MinBookings
ORDER BY "BookingCount" DESC;
