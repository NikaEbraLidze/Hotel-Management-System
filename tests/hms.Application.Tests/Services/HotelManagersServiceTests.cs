using System.Linq.Expressions;
using hms.Application.Contracts.Repository;
using hms.Application.Models.DTO;
using hms.Application.Models.Exceptions;
using hms.Application.Services;
using hms.Application.Tests.Common;
using hms.Domain.Entities;
using hms.Domain.Identity;
using MapsterMapper;
using Moq;

namespace hms.Application.Tests.Services;

public class HotelManagersServiceTests
{
    private readonly Mock<IHotelRepository> _hotelRepository = new();
    private readonly Mock<IManagersRepository> _managersRepository = new();
    private readonly Mock<IHotelManagersRepository> _hotelManagersRepository = new();
    private readonly IMapper _mapper = MapsterTestMapperFactory.Create();

    [Fact]
    public async Task AddHotelManagerAsync_AssignsManagerToHotelAndSaves()
    {
        var hotelId = Guid.NewGuid();
        var manager = TestDataFactory.CreateUser(id: Guid.NewGuid(), email: "manager@example.com", firstName: "Mariam");
        var request = new AddHotelManagerRequestDTO
        {
            ManagerId = manager.Id
        };
        HotelManager capturedRelation = null;

        SetupHotelExists(true);
        _managersRepository.Setup(repository => repository.GetByIdAsync(manager.Id))
            .ReturnsAsync(manager);
        _hotelManagersRepository.Setup(repository => repository.ExistsAsync(hotelId, manager.Id))
            .ReturnsAsync(false);
        _hotelManagersRepository.Setup(repository => repository.AddAsync(It.IsAny<HotelManager>()))
            .Callback<HotelManager>(relation => capturedRelation = relation)
            .Returns(Task.CompletedTask);
        _hotelManagersRepository.Setup(repository => repository.SaveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = CreateService();
        var result = await service.AddHotelManagerAsync(hotelId, request);

        Assert.NotNull(capturedRelation);
        Assert.Equal(hotelId, capturedRelation.HotelId);
        Assert.Equal(manager.Id, capturedRelation.ManagerUserId);
        Assert.Equal(manager.Id, result.Id);
        Assert.Equal("Mariam", result.FirstName);
    }

    [Fact]
    public async Task UpdateHotelManagerAsync_ThrowsConflict_WhenEmailAlreadyExists()
    {
        var hotelId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var relation = TestDataFactory.CreateHotelManager(
            hotelId: hotelId,
            manager: TestDataFactory.CreateUser(id: managerId, email: "old@example.com"));
        var request = new UpdateHotelManagerRequestDTO
        {
            Email = "new@example.com"
        };

        SetupHotelExists(true);
        _hotelManagersRepository.Setup(repository => repository.GetByHotelAndManagerIdAsync(hotelId, managerId, true))
            .ReturnsAsync(relation);
        _managersRepository.Setup(repository => repository.UserExistsByEmailAsync("new@example.com"))
            .ReturnsAsync(true);

        var service = CreateService();
        var exception = await Assert.ThrowsAsync<ConflictException>(() => service.UpdateHotelManagerAsync(hotelId, managerId, request));

        Assert.Equal("User with email new@example.com already exists.", exception.Message);
    }

    [Fact]
    public async Task UpdateHotelManagerAsync_ThrowsIdentityOperationException_WhenRepositoryUpdateFails()
    {
        var hotelId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var relation = TestDataFactory.CreateHotelManager(
            hotelId: hotelId,
            manager: TestDataFactory.CreateUser(id: managerId, email: "manager@example.com"));

        SetupHotelExists(true);
        _hotelManagersRepository.Setup(repository => repository.GetByHotelAndManagerIdAsync(hotelId, managerId, true))
            .ReturnsAsync(relation);
        _managersRepository.Setup(repository => repository.UpdateAsync(relation.ManagerUser))
            .ReturnsAsync(IdentityResultFactory.Failed("Update failed."));

        var service = CreateService();
        var exception = await Assert.ThrowsAsync<IdentityOperationException>(() => service.UpdateHotelManagerAsync(hotelId, managerId, new UpdateHotelManagerRequestDTO
        {
            FirstName = "Updated"
        }));

        Assert.Contains("Manager update failed: Update failed.", exception.Errors);
    }

    [Fact]
    public async Task DeleteHotelManagerAsync_ThrowsConflict_WhenHotelWouldHaveNoManagersLeft()
    {
        var hotelId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var relation = TestDataFactory.CreateHotelManager(
            hotelId: hotelId,
            manager: TestDataFactory.CreateUser(id: managerId));

        SetupHotelExists(true);
        _hotelManagersRepository.Setup(repository => repository.GetByHotelAndManagerIdAsync(hotelId, managerId, true))
            .ReturnsAsync(relation);
        _hotelManagersRepository.Setup(repository => repository.CountByHotelIdAsync(hotelId))
            .ReturnsAsync(1);

        var service = CreateService();
        var exception = await Assert.ThrowsAsync<ConflictException>(() => service.DeleteHotelManagerAsync(hotelId, managerId));

        Assert.Equal($"Manager with ID {managerId} cannot be deleted because hotel {hotelId} must have at least one manager.", exception.Message);
    }

    private HotelManagersService CreateService()
        => new(_hotelRepository.Object, _managersRepository.Object, _hotelManagersRepository.Object, _mapper);

    private void SetupHotelExists(bool exists)
    {
        _hotelRepository.Setup(repository => repository.ExistsAsync(It.IsAny<Expression<Func<Hotel, bool>>>()))
            .ReturnsAsync(exists);
    }
}
