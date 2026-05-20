namespace BookingSystemAI.Application.DTOs;

public record RegisterRequestDto(string Email, string Password, string Role);

public record LoginRequestDto(string Email, string Password);

public record RegisterResponseDto(string UserId);

public record AuthResponseDto(string AccessToken, int ExpiresIn);
