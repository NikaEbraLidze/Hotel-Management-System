using hms.Application.Contracts.Repository;
using hms.Application.Models.DTO;
using hms.Application.Models.Exceptions;
using hms.Application.Services;
using hms.Application.Tests.Common;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace hms.Application.Tests.Services;

public class GuestsServiceTests
{
    private readonly Mock<IGuestsRepository> _guestsRepository = new();
    private readonly IMapper _mapper = MapsterTestMapperFactory.Create();

    [Fact]
    public async Task GetGuestsAsync_ReturnsMappedGuests()
    {
        var guests = new List<hms.Domain.Identity.ApplicationUser>
        {
            TestDataFactory.CreateUser(id: Guid.NewGuid(), email: "guest1@example.com", firstName: "Guest1"),
            TestDataFactory.CreateUser(id: Guid.NewGuid(), email: "guest2@example.com", firstName: "Guest2")
        };

        _guestsRepository.Setup(repository => repository.GetAllAsync())
            .ReturnsAsync(guests);

        var service = CreateService();
        var result = await service.GetGuestsAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("guest1@example.com", result[0].Email);
        Assert.Equal("Guest2", result[1].FirstName);
    }

    [Fact]
    public async Task UpdateGuestAsync_ThrowsConflict_WhenEmailAlreadyExists()
    {
        var guest = TestDataFactory.CreateUser(id: Guid.NewGuid(), email: "old@example.com");
        var request = new UpdateGuestRequestDTO
        {
            Email = "new@example.com"
        };

        _guestsRepository.Setup(repository => repository.GetByIdAsync(guest.Id))
            .ReturnsAsync(guest);
        _guestsRepository.Setup(repository => repository.UserExistsByEmailAsync("new@example.com"))
            .ReturnsAsync(true);

        var service = CreateService();
        var exception = await Assert.ThrowsAsync<ConflictException>(() => service.UpdateGuestAsync(guest.Id, request));

        Assert.Equal("User with email new@example.com already exists.", exception.Message);
    }

    [Fact]
    public async Task UpdateGuestAsync_ThrowsIdentityOperationException_WhenRepositoryUpdateFails()
    {
        var guest = TestDataFactory.CreateUser(id: Guid.NewGuid(), email: "guest@example.com");
        var request = new UpdateGuestRequestDTO
        {
            FirstName = " Updated "
        };

        _guestsRepository.Setup(repository => repository.GetByIdAsync(guest.Id))
            .ReturnsAsync(guest);
        _guestsRepository.Setup(repository => repository.UpdateAsync(guest))
            .ReturnsAsync(IdentityResultFactory.Failed("Update failed."));

        var service = CreateService();
        var exception = await Assert.ThrowsAsync<IdentityOperationException>(() => service.UpdateGuestAsync(guest.Id, request));

        Assert.Contains("Guest update failed: Update failed.", exception.Errors);
    }

    [Fact]
    public async Task DeleteGuestAsync_ThrowsConflict_WhenGuestHasReservations()
    {
        var guest = TestDataFactory.CreateUser(id: Guid.NewGuid());

        _guestsRepository.Setup(repository => repository.GetByIdAsync(guest.Id))
            .ReturnsAsync(guest);
        _guestsRepository.Setup(repository => repository.HasReservationsAsync(guest.Id))
            .ReturnsAsync(true);

        var service = CreateService();
        var exception = await Assert.ThrowsAsync<ConflictException>(() => service.DeleteGuestAsync(guest.Id));

        Assert.Equal($"Guest with ID {guest.Id} cannot be deleted because it has reservations.", exception.Message);
    }

    private GuestsService CreateService() => new(_guestsRepository.Object, _mapper);
}
