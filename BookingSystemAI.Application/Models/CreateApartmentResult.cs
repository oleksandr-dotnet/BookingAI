using BookingSystemAI.Application.DTOs;

namespace BookingSystemAI.Application.Models;

public sealed class CreateApartmentResult
{
    public bool Succeeded { get; init; }
    public ApartmentResponseDto? Response { get; init; }
    public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; init; }

    public static CreateApartmentResult Success(ApartmentResponseDto response) =>
        new() { Succeeded = true, Response = response };

    public static CreateApartmentResult ValidationFailure(IReadOnlyDictionary<string, string[]> errors) =>
        new() { Succeeded = false, ValidationErrors = errors };
}
