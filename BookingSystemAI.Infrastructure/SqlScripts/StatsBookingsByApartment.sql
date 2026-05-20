SELECT
    b."ApartmentId",
    COUNT(*)::int AS "BookingCount",
    COALESCE(SUM(
        b."PricePerNight" * GREATEST(EXTRACT(EPOCH FROM (b."End" - b."Start")) / 86400.0, 0)
    ), 0) AS "RevenueSum"
FROM "Bookings" b
GROUP BY b."ApartmentId"
ORDER BY "RevenueSum" DESC;
