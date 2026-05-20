namespace BookingSystemAI.Application.DTOs;

public record ApartmentListItemDto(Guid Id, string Name, string Description, bool? IsAvailable = null);

public record CreateApartmentRequestDto(string Name, string Description);

public record ApartmentResponseDto(Guid Id, string Name, string Description);

public record ListApartmentsQueryDto(DateTimeOffset? From, DateTimeOffset? To, bool AvailableOnly);
