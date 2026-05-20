SELECT
    percentile_cont(0.25) WITHIN GROUP (ORDER BY a."PricePerNight") AS "P25",
    percentile_cont(0.50) WITHIN GROUP (ORDER BY a."PricePerNight") AS "P50",
    percentile_cont(0.75) WITHIN GROUP (ORDER BY a."PricePerNight") AS "P75"
FROM "Apartments" a;
