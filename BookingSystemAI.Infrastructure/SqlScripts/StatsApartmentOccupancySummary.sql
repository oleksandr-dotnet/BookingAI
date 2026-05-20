SELECT
    b."ApartmentId",
    COUNT(*)::int AS "BookingCount",
    COALESCE(AVG(GREATEST(EXTRACT(EPOCH FROM (b."End" - b."Start")) / 86400.0, 0)), 0) AS "AverageNightsBooked"
FROM "Bookings" b
GROUP BY b."ApartmentId"
HAVING COALESCE(AVG(GREATEST(EXTRACT(EPOCH FROM (b."End" - b."Start")) / 86400.0, 0)), 0) >= @MinAvgNights
ORDER BY "AverageNightsBooked" DESC;
