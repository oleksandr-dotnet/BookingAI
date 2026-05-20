using BookingSystemAI.Application.DTOs;
using BookingSystemAI.Domain.Entities;

namespace BookingSystemAI.Application.Abstractions;

public interface IJwtTokenService
{
    AuthResponseDto CreateToken(User user);
}
