using BookingSystemAI.Application.DTOs;

namespace BookingSystemAI.Application.Models;

public enum CreateBookingFailureReason
{
    None,
    Validation,
    NotFound,
    Conflict
}

public sealed class CreateBookingResult
{
    public bool Succeeded { get; init; }
    public CreateBookingFailureReason FailureReason { get; init; }
    public BookingResponseDto? Response { get; init; }
    public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; init; }

    public static CreateBookingResult Success(BookingResponseDto response) =>
        new() { Succeeded = true, Response = response };

    public static CreateBookingResult ValidationFailure(IReadOnlyDictionary<string, string[]> errors) =>
        new() { Succeeded = false, FailureReason = CreateBookingFailureReason.Validation, ValidationErrors = errors };

    public static CreateBookingResult NotFound() =>
        new() { Succeeded = false, FailureReason = CreateBookingFailureReason.NotFound };

    public static CreateBookingResult Conflict() =>
        new() { Succeeded = false, FailureReason = CreateBookingFailureReason.Conflict };
}
