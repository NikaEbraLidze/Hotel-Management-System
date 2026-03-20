using System.IdentityModel.Tokens.Jwt;
using System.Text;
using hms.Application.Services;
using hms.Application.Tests.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace hms.Application.Tests.Services;

public class JwtTokenGeneratorTests
{
    [Fact]
    public void GenerateToken_IncludesExpectedClaimsAndMetadata()
    {
        const string secret = "this-is-a-long-test-secret-key-for-jwt-signing";
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["JwtOptions:Secret"] = secret,
                ["JwtOptions:Issuer"] = "hms-api",
                ["JwtOptions:Audience"] = "hms-client"
            })
            .Build();
        var user = TestDataFactory.CreateUser(id: Guid.NewGuid(), email: "guest@example.com", firstName: "Nika");
        var generator = new JWTTokenGenerator(configuration);

        var token = generator.GenerateToken(user, new[] { "Guest", "Admin" });
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwt = tokenHandler.ReadJwtToken(token);

        Assert.Equal("hms-api", jwt.Issuer);
        Assert.Contains("hms-client", jwt.Audiences);
        Assert.Equal(user.Id.ToString(), jwt.Claims.Single(claim => claim.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal(user.Email, jwt.Claims.Single(claim => claim.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal(user.FirstName, jwt.Claims.Single(claim => claim.Type == JwtRegisteredClaimNames.Name).Value);

        var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret)),
            ValidateIssuer = true,
            ValidIssuer = "hms-api",
            ValidateAudience = true,
            ValidAudience = "hms-client",
            ValidateLifetime = false,
            ClockSkew = TimeSpan.Zero
        }, out _);

        Assert.True(principal.IsInRole("Guest"));
        Assert.True(principal.IsInRole("Admin"));
    }
}
