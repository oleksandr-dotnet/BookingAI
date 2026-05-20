using BookingSystemAI.Application.DTOs;

namespace BookingSystemAI.Application.Models;

public enum UpdateApartmentFailureReason
{
    None,
    Validation,
    NotFound,
    Conflict
}

public sealed class UpdateApartmentResult
{
    public bool Succeeded { get; init; }
    public UpdateApartmentFailureReason FailureReason { get; init; }
    public ApartmentResponseDto? Response { get; init; }
    public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; init; }

    public static UpdateApartmentResult Success(ApartmentResponseDto response) =>
        new() { Succeeded = true, Response = response };

    public static UpdateApartmentResult ValidationFailure(IReadOnlyDictionary<string, string[]> errors) =>
        new() { Succeeded = false, FailureReason = UpdateApartmentFailureReason.Validation, ValidationErrors = errors };

    public static UpdateApartmentResult NotFound() =>
        new() { Succeeded = false, FailureReason = UpdateApartmentFailureReason.NotFound };

    public static UpdateApartmentResult Conflict() =>
        new() { Succeeded = false, FailureReason = UpdateApartmentFailureReason.Conflict };
}
