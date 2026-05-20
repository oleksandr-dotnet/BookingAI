using BookingSystemAI.Application.DTOs;
using BookingSystemAI.Application.Models;

namespace BookingSystemAI.Application.Services;

public interface IUserService
{
    Task<RegisterResult> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);
    Task<LoginResult> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
}
