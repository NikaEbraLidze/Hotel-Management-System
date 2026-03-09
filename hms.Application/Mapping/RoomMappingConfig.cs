using hms.Application.Models.DTO;
using hms.Domain.Entities;
using Mapster;

namespace hms.Application.Mapping
{
    public sealed class RoomMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateRoomRequestDTO, Room>()
                .Map(dest => dest.Name, src => src.Name == null ? null : src.Name.Trim())
                .Map(dest => dest.Price, src => src.Price);

            config.NewConfig<UpdateRoomRequestDTO, Room>()
                .IgnoreNullValues(true)
                .Map(dest => dest.Name, src => src.Name == null ? null : src.Name.Trim(), src => src.Name != null)
                .Map(dest => dest.Price, src => src.Price.Value, src => src.Price.HasValue);

            config.NewConfig<Room, GetRoomsResponseDTO>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Name, src => src.Name)
                .Map(dest => dest.Price, src => src.Price);
        }
    }
}
