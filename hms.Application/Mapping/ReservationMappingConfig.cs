using hms.Application.Models.DTO;
using hms.Domain.Entities;
using hms.Domain.Identity;
using Mapster;

namespace hms.Application.Mapping
{
    public sealed class ReservationMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateReservationRequestDTO, Reservation>()
                .Map(dest => dest.CheckInDate, src => src.CheckInDate.Date)
                .Map(dest => dest.CheckOutDate, src => src.CheckOutDate.Date)
                .Ignore(dest => dest.GuestId)
                .Ignore(dest => dest.Guest)
                .Ignore(dest => dest.ReservationRooms);

            config.NewConfig<UpdateReservationRequestDTO, Reservation>()
                .Map(dest => dest.CheckInDate, src => src.CheckInDate.Date)
                .Map(dest => dest.CheckOutDate, src => src.CheckOutDate.Date)
                .Ignore(dest => dest.GuestId)
                .Ignore(dest => dest.Guest)
                .Ignore(dest => dest.ReservationRooms);

            config.NewConfig<ApplicationUser, ReservationGuestResponseDTO>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.FirstName, src => src.FirstName)
                .Map(dest => dest.LastName, src => src.LastName)
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.PhoneNumber, src => src.PhoneNumber);

            config.NewConfig<Room, ReservationRoomResponseDTO>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Name, src => src.Name)
                .Map(dest => dest.Price, src => src.Price);

            config.NewConfig<Reservation, GetReservationResponseDTO>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.HotelId, src => src.ReservationRooms.Select(rr => rr.Room.HotelId).FirstOrDefault())
                .Map(dest => dest.CheckInDate, src => src.CheckInDate)
                .Map(dest => dest.CheckOutDate, src => src.CheckOutDate)
                .Map(dest => dest.Guest, src => src.Guest)
                .Map(dest => dest.Rooms, src => src.ReservationRooms.OrderBy(rr => rr.Room.Name).Select(rr => rr.Room));
        }
    }
}
