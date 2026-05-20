using BookingSystemAI.Application.Abstractions;
using BookingSystemAI.Application.DTOs;
using Dapper;

namespace BookingSystemAI.Infrastructure.Sql;

public sealed class DapperApartmentSqlGateway(INpgsqlConnectionFactory connectionFactory, ISqlScriptLoader scriptLoader)
    : IApartmentSqlGateway
{
    public async Task<ApartmentUpsertRow?> UpsertApartmentAsync(ApartmentUpsertCommand command,
        CancellationToken cancellationToken = default)
    {
        var sql = scriptLoader.Load("UpsertApartment.sql");
        await using var connection = connectionFactory.CreateConnection();
        var row = await connection.QuerySingleOrDefaultAsync<ApartmentUpsertRowDto>(
            new CommandDefinition(
                sql,
                new
                {
                    command.Id,
                    command.HostId,
                    command.Name,
                    command.Description,
                    command.PricePerNight,
                    command.GuestCount,
                    command.Amenities,
                    command.MetadataJson,
                    command.SourceCompanyId,
                    command.ExternalId,
                    command.UpdateMetadata
                },
                cancellationToken: cancellationToken));

        return row is null
            ? null
            : new ApartmentUpsertRow(
                row.Id,
                row.Name,
                row.Description,
                row.PricePerNight,
                row.GuestCount,
                row.Amenities,
                row.MetadataJson);
    }

    public async Task<BookingSummaryAnalyticsDto> GetBookingSummaryAsync(CancellationToken cancellationToken = default)
    {
        var sql = scriptLoader.Load("StatsBookingAggregates.sql");
        await using var connection = connectionFactory.CreateConnection();
        var row = await connection.QuerySingleAsync<BookingSummaryRowDto>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
        return new BookingSummaryAnalyticsDto(row.TotalBookings, row.TotalRevenue, row.AveragePricePerNight);
    }

    public async Task<IReadOnlyList<BookingsByApartmentAnalyticsDto>> GetBookingsByApartmentAsync(
        CancellationToken cancellationToken = default)
    {
        var sql = scriptLoader.Load("StatsBookingsByApartment.sql");
        await using var connection = connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<BookingsByApartmentRowDto>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows
            .Select(r => new BookingsByApartmentAnalyticsDto(r.ApartmentId, r.BookingCount, r.RevenueSum))
            .ToList();
    }

    public async Task<IReadOnlyList<ActiveHostAnalyticsDto>> GetActiveHostsAsync(int minBookings,
        CancellationToken cancellationToken = default)
    {
        var sql = scriptLoader.Load("StatsHostsWithMinBookings.sql");
        await using var connection = connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<ActiveHostRowDto>(
            new CommandDefinition(sql, new { MinBookings = minBookings }, cancellationToken: cancellationToken));
        return rows.Select(r => new ActiveHostAnalyticsDto(r.HostId, r.BookingCount)).ToList();
    }

    public async Task<PriceQuantilesAnalyticsDto> GetPriceQuantilesAsync(CancellationToken cancellationToken = default)
    {
        var sql = scriptLoader.Load("StatsPricePerNightQuantiles.sql");
        await using var connection = connectionFactory.CreateConnection();
        var row = await connection.QuerySingleAsync<PriceQuantilesRowDto>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
        return new PriceQuantilesAnalyticsDto(
            row.P25 is null ? null : (decimal)row.P25,
            row.P50 is null ? null : (decimal)row.P50,
            row.P75 is null ? null : (decimal)row.P75);
    }

    public async Task<IReadOnlyList<ApartmentOccupancyAnalyticsDto>> GetApartmentOccupancyAsync(decimal minAvgNights,
        CancellationToken cancellationToken = default)
    {
        var sql = scriptLoader.Load("StatsApartmentOccupancySummary.sql");
        await using var connection = connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<ApartmentOccupancyRowDto>(
            new CommandDefinition(sql, new { MinAvgNights = minAvgNights }, cancellationToken: cancellationToken));
        return rows
            .Select(r => new ApartmentOccupancyAnalyticsDto(
                r.ApartmentId,
                r.BookingCount,
                (decimal)r.AverageNightsBooked))
            .ToList();
    }
}
