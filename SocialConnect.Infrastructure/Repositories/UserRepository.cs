using Microsoft.AspNetCore.Identity;
using SocialConnect.Domain.Entities;
using SocialConnect.Domain.Interfaces;
using SocialConnect.Shared.Models;
using SocialConnect.Infrastructure.Interfaces;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SocialConnect.Domain.Entitities.Constants;

namespace SocialConnect.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailService _emailService;

        public UserRepository(UserManager<User> userManager,
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

            if (confirmResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, UserConstant.USER);
            }

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

        public async Task<IEnumerable<User>> GetAsync()
        {
            return await _userManager.Users.AsNoTracking()
                                           .Include(user => user.Friends)
                                           .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetAsync(Expression<Func<User, bool>> expression)
        {
            return await _userManager.Users.Where(expression).ToListAsync();
        }

        public async Task<User?> FirstOrDefaultAsync(Expression<Func<User, bool>> expression)
        {
            return await _userManager.Users.FirstOrDefaultAsync(expression);
        }

        public async Task<User?> CreateAsync(User entity)
        {
            IdentityResult createResult = await _userManager.CreateAsync(entity);

            if(!createResult.Succeeded)
            {
                return null;
            }

            return await _userManager.FindByEmailAsync(entity.Email);
        }

        public async Task<User?> UpdateAsync(string id, User entity)
        {
            User? user = await _userManager.FindByIdAsync(id);

            if(user == null)
            {
                return null;
            }

            user.Firstname = entity.Firstname;
            user.Lastname = entity.Lastname;
            user.UserName = entity.UserName;
            user.Email = entity.Email;
            user.DateOfBirth = entity.DateOfBirth;
            user.Gender = entity.Gender;

            IdentityResult updateResult = await _userManager.UpdateAsync(user);

            if(!updateResult.Succeeded)
            {
                return null;
            }

            return user;
        }

        public async Task<bool> DeleteAsync(User entity)
        {
            return await DeleteAsync(entity.Id);
        }
    }
}
