using hms.Domain.Entities;
using hms.Domain.Identity;

namespace hms.Application.Tests.Common;

internal static class TestDataFactory
{
    public static ApplicationUser CreateUser(
        Guid? id = null,
        string email = "guest@example.com",
        string firstName = "Nika",
        string lastName = "Ebralidze",
        string personalNumber = "12345678901",
        string phoneNumber = "+995599123456")
    {
        return new ApplicationUser
        {
            Id = id ?? Guid.NewGuid(),
            Email = email,
            UserName = email,
            FirstName = firstName,
            LastName = lastName,
            PersonalNumber = personalNumber,
            PhoneNumber = phoneNumber
        };
    }

    public static Hotel CreateHotel(
        Guid? id = null,
        string name = "HMS Hotel",
        byte rating = 5,
        string address = "123 Rustaveli Ave",
        string city = "Tbilisi",
        string country = "Georgia",
        string imageUrl = null,
        string imagePublicId = null)
    {
        return new Hotel
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Rating = rating,
            Address = address,
            City = city,
            Country = country,
            ImageUrl = imageUrl,
            ImgPublicId = imagePublicId
        };
    }

    public static Room CreateRoom(
        Guid? id = null,
        Guid? hotelId = null,
        string name = "101",
        decimal price = 150m)
    {
        return new Room
        {
            Id = id ?? Guid.NewGuid(),
            HotelId = hotelId ?? Guid.NewGuid(),
            Name = name,
            Price = price
        };
    }

    public static HotelManager CreateHotelManager(
        Guid? hotelId = null,
        ApplicationUser manager = null)
    {
        manager ??= CreateUser(email: "manager@example.com", firstName: "Mariam");

        return new HotelManager
        {
            HotelId = hotelId ?? Guid.NewGuid(),
            ManagerUserId = manager.Id,
            ManagerUser = manager
        };
    }

    public static Reservation CreateReservation(
        Guid? id = null,
        Guid? guestId = null,
        DateTime? checkInDate = null,
        DateTime? checkOutDate = null,
        ApplicationUser guest = null,
        params Room[] rooms)
    {
        guest ??= CreateUser(id: guestId);

        var reservation = new Reservation
        {
            Id = id ?? Guid.NewGuid(),
            GuestId = guest.Id,
            Guest = guest,
            CheckInDate = checkInDate ?? DateTime.UtcNow.Date.AddDays(2),
            CheckOutDate = checkOutDate ?? DateTime.UtcNow.Date.AddDays(4)
        };

        var reservationRooms = rooms.Select(room => new ReservationRoom
        {
            ReservationId = reservation.Id,
            Reservation = reservation,
            RoomId = room.Id,
            Room = room
        }).ToList();

        reservation.ReservationRooms = reservationRooms;
        return reservation;
    }
}
