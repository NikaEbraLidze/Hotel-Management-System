using hms.Application.Models.DTO;
using hms.Domain.Identity;
using Mapster;

namespace hms.Application.Mapping
{
    public sealed class GuestMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<UpdateGuestRequestDTO, ApplicationUser>()
                .IgnoreNullValues(true)
                .Map(dest => dest.FirstName, src => src.FirstName == null ? null : src.FirstName.Trim(), src => src.FirstName != null)
                .Map(dest => dest.LastName, src => src.LastName == null ? null : src.LastName.Trim(), src => src.LastName != null)
                .Map(dest => dest.Email, src => src.Email == null ? null : src.Email.Trim(), src => src.Email != null)
                .Map(dest => dest.UserName, src => src.Email == null ? null : src.Email.Trim(), src => src.Email != null)
                .Map(dest => dest.PersonalNumber, src => src.PersonalNumber == null ? null : src.PersonalNumber.Trim(), src => src.PersonalNumber != null)
                .Map(dest => dest.PhoneNumber, src => src.PhoneNumber == null ? null : src.PhoneNumber.Trim(), src => src.PhoneNumber != null);

            config.NewConfig<ApplicationUser, GetGuestByIdResponseDTO>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.FirstName, src => src.FirstName)
                .Map(dest => dest.LastName, src => src.LastName)
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.PersonalNumber, src => src.PersonalNumber)
                .Map(dest => dest.PhoneNumber, src => src.PhoneNumber);

            config.NewConfig<ApplicationUser, GetGuestsResponseDTO>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.FirstName, src => src.FirstName)
                .Map(dest => dest.LastName, src => src.LastName)
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.PersonalNumber, src => src.PersonalNumber)
                .Map(dest => dest.PhoneNumber, src => src.PhoneNumber);

            config.NewConfig<ApplicationUser, UpdateGuestResponseDTO>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.FirstName, src => src.FirstName)
                .Map(dest => dest.LastName, src => src.LastName)
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.PersonalNumber, src => src.PersonalNumber)
                .Map(dest => dest.PhoneNumber, src => src.PhoneNumber);
        }
    }
}
