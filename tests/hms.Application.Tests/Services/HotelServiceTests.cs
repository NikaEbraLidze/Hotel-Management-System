using System.Linq.Expressions;
using hms.Application.Contracts.Repository;
using hms.Application.Contracts.Service;
using hms.Application.Models.DTO;
using hms.Application.Models.Exceptions;
using hms.Application.Services;
using hms.Application.Tests.Common;
using hms.Domain.Entities;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Moq;

namespace hms.Application.Tests.Services;

public class HotelServiceTests
{
    private readonly Mock<IHotelRepository> _hotelRepository = new();
    private readonly Mock<ICloudinaryService> _cloudinaryService = new();
    private readonly IMapper _mapper = MapsterTestMapperFactory.Create();

    [Fact]
    public async Task RegisterHotelAsync_SavesHotelAndSkipsCloudinary_WhenImageIsNull()
    {
        var createdHotel = TestDataFactory.CreateHotel(imageUrl: null, imagePublicId: null);
        var request = new RegisterHotelRequestDTO
        {
            Name = " HMS Hotel ",
            Rating = 5,
            Address = " 123 Rustaveli Ave ",
            City = " Tbilisi ",
            Country = " Georgia "
        };

        Hotel capturedHotel = null;
        _hotelRepository.Setup(repository => repository.AddAsync(It.IsAny<Hotel>()))
            .Callback<Hotel>(hotel => capturedHotel = hotel)
            .Returns(Task.CompletedTask);
        _hotelRepository.Setup(repository => repository.SaveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _hotelRepository.Setup(repository => repository.GetAsync(
                It.IsAny<Expression<Func<Hotel, bool>>>(),
                It.IsAny<Func<IQueryable<Hotel>, IQueryable<Hotel>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(createdHotel);

        var service = CreateService();
        var result = await service.RegisterHotelAsync(request, image: null);

        Assert.NotNull(capturedHotel);
        Assert.Equal("HMS Hotel", capturedHotel.Name);
        Assert.Equal("123 Rustaveli Ave", capturedHotel.Address);
        Assert.Equal(createdHotel.Id, result.Id);
        _cloudinaryService.Verify(service => service.UploadImageAsync(It.IsAny<IFormFile>()), Times.Never);
        _cloudinaryService.Verify(service => service.UpdateImageAsync(It.IsAny<string>(), It.IsAny<IFormFile>()), Times.Never);
    }

    [Fact]
    public async Task UpdateHotelAsync_UsesUpdateImage_WhenHotelAlreadyHasPublicId()
    {
        var hotelId = Guid.NewGuid();
        var existingHotel = TestDataFactory.CreateHotel(id: hotelId, imagePublicId: "old-public-id", imageUrl: "https://old.example/image.jpg");
        var image = FormFileFactory.Create();
        var request = new UpdateHotelRequestDTO
        {
            Name = " Updated Hotel ",
            Rating = 4,
            Address = "New Address",
            City = "Batumi",
            Country = "Georgia"
        };

        _hotelRepository.Setup(repository => repository.GetAsync(
                It.IsAny<Expression<Func<Hotel, bool>>>(),
                It.IsAny<Func<IQueryable<Hotel>, IQueryable<Hotel>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(existingHotel);
        _hotelRepository.Setup(repository => repository.SaveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _cloudinaryService.Setup(service => service.UpdateImageAsync(existingHotel.ImgPublicId, image))
            .ReturnsAsync(new CloudinaryImageResponseDTO
            {
                PublicId = "new-public-id",
                Url = "https://cdn.example/new-image.jpg"
            });

        var service = CreateService();
        var result = await service.UpdateHotelAsync(hotelId, request, image);

        Assert.Equal(" Updated Hotel ", existingHotel.Name);
        Assert.Equal("new-public-id", existingHotel.ImgPublicId);
        Assert.Equal("https://cdn.example/new-image.jpg", result.ImageUrl);
        _hotelRepository.Verify(repository => repository.UpdateAsync(existingHotel), Times.Once);
    }

    [Fact]
    public async Task GetHotelByIdAsync_ThrowsNotFound_WhenHotelDoesNotExist()
    {
        var hotelId = Guid.NewGuid();
        _hotelRepository.Setup(repository => repository.GetAsync(
                It.IsAny<Expression<Func<Hotel, bool>>>(),
                It.IsAny<Func<IQueryable<Hotel>, IQueryable<Hotel>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync((Hotel)null);

        var service = CreateService();
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => service.GetHotelByIdAsync(hotelId));

        Assert.Equal($"Hotel with ID {hotelId} not found.", exception.Message);
    }

    [Fact]
    public async Task DeleteHotelAsync_RemovesHotelAndSaves()
    {
        var hotel = TestDataFactory.CreateHotel();
        _hotelRepository.Setup(repository => repository.GetAsync(
                It.IsAny<Expression<Func<Hotel, bool>>>(),
                It.IsAny<Func<IQueryable<Hotel>, IQueryable<Hotel>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(hotel);
        _hotelRepository.Setup(repository => repository.SaveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = CreateService();
        await service.DeleteHotelAsync(hotel.Id);

        _hotelRepository.Verify(repository => repository.DeleteAsync(hotel), Times.Once);
        _hotelRepository.Verify(repository => repository.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private HotelService CreateService() => new(_hotelRepository.Object, _mapper, _cloudinaryService.Object);
}

