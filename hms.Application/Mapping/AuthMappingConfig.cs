using hms.Application.Models.DTO;
using hms.Domain.Identity;
using Mapster;

namespace hms.Application.Mapping
{
    public sealed class AuthMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<RegistrationRequestDTO, ApplicationUser>()
                .Map(dest => dest.FirstName, src => src.FirstName == null ? null : src.FirstName.Trim())
                .Map(dest => dest.LastName, src => src.LastName == null ? null : src.LastName.Trim())
                .Map(dest => dest.Email, src => src.Email == null ? null : src.Email.Trim())
                .Map(dest => dest.UserName, src => src.Email == null ? null : src.Email.Trim())
                .Map(dest => dest.PersonalNumber, src => src.PersonalNumber == null ? null : src.PersonalNumber.Trim())
                .Map(dest => dest.PhoneNumber, src => src.PhoneNumber == null ? null : src.PhoneNumber.Trim());
        }
    }
}
