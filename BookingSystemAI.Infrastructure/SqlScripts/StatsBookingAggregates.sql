SELECT
    COUNT(*)::int AS "TotalBookings",
    COALESCE(SUM(
        b."PricePerNight" * GREATEST(EXTRACT(EPOCH FROM (b."End" - b."Start")) / 86400.0, 0)
    ), 0) AS "TotalRevenue",
    COALESCE(AVG(b."PricePerNight"), 0) AS "AveragePricePerNight"
FROM "Bookings" b;
