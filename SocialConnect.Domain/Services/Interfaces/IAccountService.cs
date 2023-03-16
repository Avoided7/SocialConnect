using SocialConnect.Entity.Dtos;

namespace SocialConnect.Domain.Services.Interfaces
{
    public interface IAccountService
    {
        Task<ResponseDto<UserDto>> LoginAsync(LoginUserDto loginDto);
        Task<ResponseDto<UserDto>> RegisterAsync(RegisterUserDto registerDto);
        Task<ResponseDto<UserDto>> FindByIdAsync(string id);
        Task<ResponseDto<UserDto>> FindByUsernameAsync(string username);
        Task<ResponseDto<DateTime>> DeleteAsync(string id);
        Task<ResponseDto<DateTime>> LogoutAsync();
    }
}
