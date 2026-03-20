using Microsoft.AspNetCore.Identity;

namespace hms.Application.Tests.Common;

internal static class IdentityResultFactory
{
    public static IdentityResult Failed(params string[] descriptions)
    {
        var errors = descriptions
            .Select(description => new IdentityError { Description = description })
            .ToArray();

        return IdentityResult.Failed(errors);
    }
}
