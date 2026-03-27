using hms.Application.Configuration;
using hms.Application.Contracts.Service;
using hms.Application.Models.DTO;
using hms.Application.Models.Exceptions;
using hms.Application.Services;
using hms.Application.Tests.Common;
using hms.Domain.Identity;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;

namespace hms.Application.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IJWTTokenGenerator> _jwt = new();
    private readonly Mock<IEmailService> _email = new();
    private readonly IMapper _mapper = MapsterTestMapperFactory.Create();
    private readonly IOptions<AppUrlConfiguration> _appUrlConfiguration = Options.Create(new AppUrlConfiguration
    {
        ApiBaseUrl = "https://localhost:7061"
    });

    [Fact]
    public async Task LoginAsync_ReturnsToken_WhenCredentialsAreValid()
    {
        var user = TestDataFactory.CreateUser(email: "guest@example.com");
        user.EmailConfirmed = true;
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
        user.EmailConfirmed = true;

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
    public async Task LoginAsync_ThrowsUnauthorized_WhenEmailIsNotConfirmed()
    {
        var user = TestDataFactory.CreateUser(email: "guest@example.com");

        _users.Setup(repository => repository.GetUserByEmailAsync(user.Email))
            .ReturnsAsync(user);
        _users.Setup(repository => repository.CheckPasswordAsync(user, "Password123"))
            .ReturnsAsync(true);

        var service = CreateService();
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.LoginAsync(new LoginRequestDTO
        {
            Email = user.Email,
            Password = "Password123"
        }));

        Assert.Equal("Email address is not confirmed.", exception.Message);
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
        _users.Setup(repository => repository.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync("token-value");
        _email.Setup(service => service.SendEmailAsync(
                "guest@example.com",
                "Confirm your email",
                It.Is<string>(body => body.Contains("confirm-email") && body.Contains("token-value")),
                true))
            .Returns(Task.CompletedTask);

        var service = CreateService();
        var result = await service.RegisterGuestAsync(request);

        Assert.Equal(createdUserId.ToString(), result.UserId);
        Assert.NotNull(capturedUser);
        Assert.Equal("guest@example.com", capturedUser.Email);
        Assert.Equal("guest@example.com", capturedUser.UserName);
        _email.VerifyAll();
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

    [Fact]
    public async Task ConfirmEmailAsync_ConfirmsUser_WhenTokenIsValid()
    {
        var userId = Guid.NewGuid();
        var user = TestDataFactory.CreateUser(email: "guest@example.com");
        user.Id = userId;

        _users.Setup(repository => repository.GetUserByIdAsync(userId))
            .ReturnsAsync(user);
        _users.Setup(repository => repository.ConfirmEmailAsync(user, "token-value"))
            .ReturnsAsync(IdentityResult.Success);

        var service = CreateService();
        await service.ConfirmEmailAsync(userId.ToString(), "token-value");
    }

    private AuthService CreateService() => new(_users.Object, _jwt.Object, _mapper, _email.Object, _appUrlConfiguration);
}
