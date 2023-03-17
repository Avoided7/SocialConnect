using Microsoft.AspNetCore.Identity;
using SocialConnect.Entity.Models;
using SocialConnect.Domain.Extenstions;
using SocialConnect.Domain.Services.Interfaces;
using SocialConnect.Entity.Dtos;

namespace SocialConnect.Domain.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailService _emailService;

        public AccountService(UserManager<User> userManager,
                              SignInManager<User> signInManager,
                              IEmailService emailService)
        { 
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._emailService = emailService;
        }

        public async Task<ResponseDto<UserDto>> FindByIdAsync(string id)
        {
            User? user = await _userManager.FindByIdAsync(id);

            ResponseDto<UserDto> response = new ResponseDto<UserDto>();

            if (user == null)
            {
                response.Errors = new List<string> { "User not found." };
            }
            else
            {
                response.Content = user.ConvertToDto();
            }

            return response;
        }

        public async Task<ResponseDto<UserDto>> FindByUsernameAsync(string username)
        {
            User? user = await _userManager.FindByNameAsync(username);

            ResponseDto<UserDto> response = new ResponseDto<UserDto>();

            if(user == null)
            {
                response.Errors = new List<string> { "User not found." };
            }
            else
            {
                response.Content = user.ConvertToDto();
            }

            return response;
        }
        
        public async Task<ResponseDto<UserDto>> LoginAsync(LoginUserDto loginDto)
        {
            SignInResult signResult = await _signInManager.PasswordSignInAsync(loginDto.Username, loginDto.Password, true, true);

            ResponseDto<UserDto> response = new ResponseDto<UserDto>(); 
            
            if(!signResult.Succeeded)
            {
                response.Errors = new List<string>() { "Incorrect username/password." };
            }
            else
            {
                User user = await _userManager.FindByNameAsync(loginDto.Username);
                response.Content = user.ConvertToDto();
            }

            return response;
        }

        public async Task<ResponseDto<UserDto>> RegisterAsync(RegisterUserDto registerDto)
        {
            User user = new User();

            user.Email = registerDto.Email;
            user.UserName = registerDto.Username;
            user.Firstname = registerDto.Firstname;
            user.Lastname = registerDto.Lastname;
            user.DateOfBirth = registerDto.DateOfBirth;
            user.Gender = registerDto.Gender;

            IdentityResult createdUser = await _userManager.CreateAsync(user, registerDto.Password);

            ResponseDto<UserDto> response = new ResponseDto<UserDto>();
            if(!createdUser.Succeeded)
            {
                response.Errors = new List<string>(createdUser.Errors.Select(error => error.Description));
            }
            else
            {
                response.Content = user.ConvertToDto();
                string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                EmailDto emailDto = new EmailDto()
                {
                    Subject = "Confirm email",
                    Content = $"<h1>Confirm email</h1> <a href=https://localhost:7035/Account/Confirmation/?userid={user.Id}&token={token}>Click here to confirm email</a>",
                    Reciever = registerDto.Email
                };
                await _emailService.SendAsync(emailDto);
            }

            return response;
        }

        public async Task<ResponseDto<DateTime>> DeleteAsync(string id)
        {
            User user = await _userManager.FindByIdAsync(id);
            await _userManager.DeleteAsync(user);
            ResponseDto<DateTime> response = new ResponseDto<DateTime> { Content = DateTime.UtcNow };
            return response;
        }

        public async Task<ResponseDto<UserDto>> ConfirmAsync(string userId, string token)
        {
            User? user = await _userManager.FindByIdAsync(userId);
            ResponseDto<UserDto> response = new ResponseDto<UserDto>();
            if (user == null)
            {
                response.Errors = new List<string> { "User not found." };
                return response;
            }

            IdentityResult confirmResult = await _userManager.ConfirmEmailAsync(user, token);

            if(!confirmResult.Succeeded)
            {
                response.Errors = new List<string>
                {
                    "Incorrect token."
                };
            }
            else
            {
                response.Content = user.ConvertToDto();
            }

            return response;

        }

        public async Task<ResponseDto<DateTime>> LogoutAsync()
        {
            await _signInManager.SignOutAsync();
            ResponseDto<DateTime> response = new ResponseDto<DateTime>() { Content = DateTime.UtcNow };
            return response;
        }
    }
}
