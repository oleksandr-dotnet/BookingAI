using Microsoft.AspNetCore.Identity;

namespace BookingSystemAI.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public Guid? SourceCompanyId { get; set; }
    public string? ExternalId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? ProfileImageUrl { get; set; }
}
