using Microsoft.AspNetCore.Identity;
using SocialConnect.Domain.Entities;
using SocialConnect.Domain.Interfaces;
using SocialConnect.Shared.Models;
using SocialConnect.Infrastructure.Interfaces;


namespace SocialConnect.Domain.Services
{
    public class AccountRepository : IAccountService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailService _emailService;

        public AccountRepository(UserManager<User> userManager,
                              SignInManager<User> signInManager,
                              IEmailService emailService)
        { 
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._emailService = emailService;
        }

        public async Task<User?> FindByIdAsync(string id)
        {
            User? user = await _userManager.FindByIdAsync(id);

            return user;
        }

        public async Task<User?> FindByUsernameAsync(string username)
        {
            User? user = await _userManager.FindByNameAsync(username);

            return user;
        }
        
        public async Task<bool> LoginAsync(string login, string password)
        {
            SignInResult signResult = await _signInManager.PasswordSignInAsync(login, password, true, true);

            return signResult.Succeeded;
        }

        public async Task<bool> RegisterAsync(User user, string password)
        {
            IdentityResult createdUser = await _userManager.CreateAsync(user, password);
            if(!createdUser.Succeeded)
            {
                return false;
            }
            string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            EmailMessage emailDto = new EmailMessage()
            {
                Subject = "Confirm email",
                Content = $"<h1>Confirm email</h1> <a href=https://localhost:7035/Account/Confirmation/?userid={user.Id}&token={token}>Click here to confirm email</a>",
                Reciever = user.Email
            };
            await _emailService.SendAsync(emailDto);

            return true;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            User user = await _userManager.FindByIdAsync(id);
            IdentityResult result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> ConfirmAsync(string userId, string token)
        {
            User? user = await _userManager.FindByIdAsync(userId);
            
            if (user == null) { return false; }

            IdentityResult confirmResult = await _userManager.ConfirmEmailAsync(user, token);

            return confirmResult.Succeeded;

        }

        public async Task<bool> LogoutAsync()
        {
            try
            {
                await _signInManager.SignOutAsync();
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }
    }
}
