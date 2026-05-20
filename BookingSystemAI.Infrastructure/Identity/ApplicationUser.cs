using Microsoft.AspNetCore.Identity;

namespace BookingSystemAI.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public Guid? SourceCompanyId { get; set; }
    public string? ExternalId { get; set; }
}
