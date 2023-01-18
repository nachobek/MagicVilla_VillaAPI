using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;

namespace MagicVilla_VillaAPI.Repository.IRepository
{
    public interface IUserRepository
    {
        bool IsUniqueUser(string username);

        Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO);

        Task<LocalUser> Register(RegistrationRequestDTO registrationRequestDTO); // We could keep the return type as null, but we'll return the LocalUser since this is not the API endpoint, just the Repository that we are creating to register the user to our DB.
    }
}
