using hms.Application.Models.DTO;
using hms.Domain.Entities;
using hms.Domain.Identity;
using Mapster;

namespace hms.Application.Mapping
{
    public sealed class HotelMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<RegisterHotelRequestDTO, Hotel>()
                .Map(dest => dest.Name, src => src.Name == null ? null : src.Name.Trim())
                .Map(dest => dest.Rating, src => src.Rating)
                .Map(dest => dest.Address, src => src.Address == null ? null : src.Address.Trim())
                .Map(dest => dest.City, src => src.City == null ? null : src.City.Trim())
                .Map(dest => dest.Country, src => src.Country == null ? null : src.Country.Trim());

            config.NewConfig<Hotel, RegisterHotelResponseDTO>()
                .Map(dest => dest.Id, src => src.Id);

            config.NewConfig<Hotel, GetHotelByIdResponseDTO>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Name, src => src.Name)
                .Map(dest => dest.Rating, src => src.Rating)
                .Map(dest => dest.Address, src => src.Address)
                .Map(dest => dest.City, src => src.City)
                .Map(dest => dest.Country, src => src.Country);

        }
    }
}
