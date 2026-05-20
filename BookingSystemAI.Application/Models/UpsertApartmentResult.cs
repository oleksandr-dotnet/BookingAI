using BookingSystemAI.Application.DTOs;

namespace BookingSystemAI.Application.Models;

public sealed class UpsertApartmentResult
{
    public bool Succeeded { get; init; }
    public UpsertApartmentFailureReason? FailureReason { get; init; }
    public ApartmentResponseDto? Response { get; init; }
    public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; init; }

    public static UpsertApartmentResult Success(ApartmentResponseDto response) =>
        new() { Succeeded = true, Response = response };

    public static UpsertApartmentResult NotFound() =>
        new() { Succeeded = false, FailureReason = UpsertApartmentFailureReason.NotFound };

    public static UpsertApartmentResult ValidationFailure(IReadOnlyDictionary<string, string[]> errors) =>
        new()
        {
            Succeeded = false,
            FailureReason = UpsertApartmentFailureReason.Validation,
            ValidationErrors = errors
        };
}

public enum UpsertApartmentFailureReason
{
    Validation,
    NotFound
}
