using hms.Domain.Identity;

namespace hms.Application.Contracts.Service
{
    public interface IJWTTokenGenerator
    {
        string GenerateToken(ApplicationUser user, IEnumerable<string> roles);
    }
}