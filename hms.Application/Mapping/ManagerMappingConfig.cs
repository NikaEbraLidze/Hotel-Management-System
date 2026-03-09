using hms.Application.Models.DTO;
using hms.Domain.Entities;
using hms.Domain.Identity;
using Mapster;

namespace hms.Application.Mapping
{
    public sealed class ManagerMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<AddHotelManagerRequestDTO, HotelManager>()
                .Map(dest => dest.ManagerUserId, src => src.ManagerId);

            config.NewConfig<UpdateHotelManagerRequestDTO, ApplicationUser>()
                .IgnoreNullValues(true)
                .Map(dest => dest.FirstName, src => src.FirstName == null ? null : src.FirstName.Trim(), src => src.FirstName != null)
                .Map(dest => dest.LastName, src => src.LastName == null ? null : src.LastName.Trim(), src => src.LastName != null)
                .Map(dest => dest.Email, src => src.Email == null ? null : src.Email.Trim(), src => src.Email != null)
                .Map(dest => dest.UserName, src => src.Email == null ? null : src.Email.Trim(), src => src.Email != null)
                .Map(dest => dest.PersonalNumber, src => src.PersonalNumber == null ? null : src.PersonalNumber.Trim(), src => src.PersonalNumber != null)
                .Map(dest => dest.PhoneNumber, src => src.PhoneNumber == null ? null : src.PhoneNumber.Trim(), src => src.PhoneNumber != null);

            config.NewConfig<HotelManager, AddHotelManagerResponseDTO>()
                .Map(dest => dest.Id, src => src.ManagerUserId)
                .Map(dest => dest.FirstName, src => src.ManagerUser.FirstName)
                .Map(dest => dest.LastName, src => src.ManagerUser.LastName)
                .Map(dest => dest.Email, src => src.ManagerUser.Email)
                .Map(dest => dest.PersonalNumber, src => src.ManagerUser.PersonalNumber)
                .Map(dest => dest.PhoneNumber, src => src.ManagerUser.PhoneNumber);

            config.NewConfig<HotelManager, GetHotelManagersResponseDTO>()
                .Map(dest => dest.Id, src => src.ManagerUserId)
                .Map(dest => dest.FirstName, src => src.ManagerUser.FirstName)
                .Map(dest => dest.LastName, src => src.ManagerUser.LastName)
                .Map(dest => dest.Email, src => src.ManagerUser.Email)
                .Map(dest => dest.PersonalNumber, src => src.ManagerUser.PersonalNumber)
                .Map(dest => dest.PhoneNumber, src => src.ManagerUser.PhoneNumber);

            config.NewConfig<HotelManager, GetHotelManagerByIdResponseDTO>()
                .Map(dest => dest.Id, src => src.ManagerUserId)
                .Map(dest => dest.FirstName, src => src.ManagerUser.FirstName)
                .Map(dest => dest.LastName, src => src.ManagerUser.LastName)
                .Map(dest => dest.Email, src => src.ManagerUser.Email)
                .Map(dest => dest.PersonalNumber, src => src.ManagerUser.PersonalNumber)
                .Map(dest => dest.PhoneNumber, src => src.ManagerUser.PhoneNumber);

            config.NewConfig<HotelManager, UpdateHotelManagerResponseDTO>()
                .Map(dest => dest.Id, src => src.ManagerUserId)
                .Map(dest => dest.FirstName, src => src.ManagerUser.FirstName)
                .Map(dest => dest.LastName, src => src.ManagerUser.LastName)
                .Map(dest => dest.Email, src => src.ManagerUser.Email)
                .Map(dest => dest.PersonalNumber, src => src.ManagerUser.PersonalNumber)
                .Map(dest => dest.PhoneNumber, src => src.ManagerUser.PhoneNumber);
        }
    }
}
