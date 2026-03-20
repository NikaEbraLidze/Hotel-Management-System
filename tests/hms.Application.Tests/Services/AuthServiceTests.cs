using hms.Application.Contracts.Service;
using hms.Application.Models.DTO;
using hms.Application.Models.Exceptions;
using hms.Application.Services;
using hms.Application.Tests.Common;
using hms.Domain.Identity;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace hms.Application.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IJWTTokenGenerator> _jwt = new();
    private readonly IMapper _mapper = MapsterTestMapperFactory.Create();

    [Fact]
    public async Task LoginAsync_ReturnsToken_WhenCredentialsAreValid()
    {
        var user = TestDataFactory.CreateUser(email: "guest@example.com");
        var request = new LoginRequestDTO
        {
            Email = " guest@example.com ",
            Password = "Password123"
        };

        _users.Setup(repository => repository.GetUserByEmailAsync("guest@example.com"))
            .ReturnsAsync(user);
        _users.Setup(repository => repository.CheckPasswordAsync(user, request.Password))
            .ReturnsAsync(true);
        _users.Setup(repository => repository.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { AppRole.Guest.ToRoleName() });
        _jwt.Setup(generator => generator.GenerateToken(user, It.IsAny<IEnumerable<string>>()))
            .Returns("token-value");

        var service = CreateService();
        var result = await service.LoginAsync(request);

        Assert.Equal("token-value", result.Token);
    }

    [Fact]
    public async Task LoginAsync_ThrowsUnauthorized_WhenUserDoesNotExist()
    {
        _users.Setup(repository => repository.GetUserByEmailAsync("missing@example.com"))
            .ReturnsAsync((ApplicationUser)null);

        var service = CreateService();
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.LoginAsync(new LoginRequestDTO
        {
            Email = "missing@example.com",
            Password = "Password123"
        }));

        Assert.Equal("Invalid credentials.", exception.Message);
    }

    [Fact]
    public async Task LoginAsync_ThrowsUnauthorized_WhenPasswordIsInvalid()
    {
        var user = TestDataFactory.CreateUser(email: "guest@example.com");

        _users.Setup(repository => repository.GetUserByEmailAsync(user.Email))
            .ReturnsAsync(user);
        _users.Setup(repository => repository.CheckPasswordAsync(user, "wrong-password"))
            .ReturnsAsync(false);

        var service = CreateService();
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.LoginAsync(new LoginRequestDTO
        {
            Email = user.Email,
            Password = "wrong-password"
        }));

        Assert.Equal("Invalid credentials.", exception.Message);
    }

    [Fact]
    public async Task RegisterGuestAsync_ReturnsUserId_WhenIdentityOperationsSucceed()
    {
        var createdUserId = Guid.NewGuid();
        var request = new RegistrationRequestDTO
        {
            FirstName = " Nika ",
            LastName = " Ebralidze ",
            Email = " guest@example.com ",
            Password = "Password123",
            PersonalNumber = "12345678901",
            PhoneNumber = "+995599123456"
        };

        ApplicationUser capturedUser = null;
        _users.Setup(repository => repository.CreateUserAsync(It.IsAny<ApplicationUser>(), request.Password))
            .Callback<ApplicationUser, string>((user, _) =>
            {
                capturedUser = user;
                user.Id = createdUserId;
            })
            .ReturnsAsync(IdentityResult.Success);
        _users.Setup(repository => repository.AddToRoleAsync(It.IsAny<ApplicationUser>(), AppRole.Guest.ToRoleName()))
            .ReturnsAsync(IdentityResult.Success);

        var service = CreateService();
        var result = await service.RegisterGuestAsync(request);

        Assert.Equal(createdUserId.ToString(), result.UserId);
        Assert.NotNull(capturedUser);
        Assert.Equal("guest@example.com", capturedUser.Email);
        Assert.Equal("guest@example.com", capturedUser.UserName);
    }

    [Fact]
    public async Task RegisterGuestAsync_ThrowsIdentityOperationException_WhenCreateFails()
    {
        _users.Setup(repository => repository.CreateUserAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResultFactory.Failed("Duplicate email."));

        var service = CreateService();
        var exception = await Assert.ThrowsAsync<IdentityOperationException>(() => service.RegisterGuestAsync(new RegistrationRequestDTO
        {
            FirstName = "Nika",
            LastName = "Ebralidze",
            Email = "guest@example.com",
            Password = "Password123",
            PersonalNumber = "12345678901",
            PhoneNumber = "+995599123456"
        }));

        Assert.Contains("User registration failed: Duplicate email.", exception.Errors);
    }

    [Fact]
    public async Task RegisterGuestAsync_ThrowsIdentityOperationException_WhenRoleAssignmentFails()
    {
        _users.Setup(repository => repository.CreateUserAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _users.Setup(repository => repository.AddToRoleAsync(It.IsAny<ApplicationUser>(), AppRole.Guest.ToRoleName()))
            .ReturnsAsync(IdentityResultFactory.Failed("Role assignment failed."));

        var service = CreateService();
        var exception = await Assert.ThrowsAsync<IdentityOperationException>(() => service.RegisterGuestAsync(new RegistrationRequestDTO
        {
            FirstName = "Nika",
            LastName = "Ebralidze",
            Email = "guest@example.com",
            Password = "Password123",
            PersonalNumber = "12345678901",
            PhoneNumber = "+995599123456"
        }));

        Assert.Contains("Failed to assign Guest role: Role assignment failed.", exception.Errors);
    }

    private AuthService CreateService() => new(_users.Object, _jwt.Object, _mapper);
}

