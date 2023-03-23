﻿using Microsoft.AspNetCore.Identity;
using SocialConnect.Domain.Entities;
using SocialConnect.Domain.Interfaces;
using SocialConnect.Shared.Models;
using SocialConnect.Infrastructure.Interfaces;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialConnect.Domain.Entities.Constants;
using Microsoft.Extensions.Logging;

namespace SocialConnect.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<UserRepository> _logger;
        private readonly IUrlHelper _urlHelper;

        public UserRepository(UserManager<User> userManager,
                              SignInManager<User> signInManager,
                              IEmailService emailService,
                              ILogger<UserRepository> logger,
                              IUrlHelper urlHelper)
        { 
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._emailService = emailService;
            this._logger = logger;
            this._urlHelper = urlHelper;
        }

        public async Task<User?> FindByIdAsync(string id)
        {
            User? user = await FirstOrDefaultAsync(user => user.Id == id);

            return user;
        }

        public async Task<User?> FindByUsernameAsync(string username)
        {
            User? user = await FirstOrDefaultAsync(user => user.UserName == username);

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
            string url = _urlHelper.Link("default", new { controller = "Account", action = "Confirmation", userid = user.Id, token = token });
            EmailMessage emailDto = new EmailMessage()
            {
                Subject = "Confirm email",
                Content = $"<h1>Confirm email</h1> <a href='{url}'>Click here to confirm email</a>",
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
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public async Task<IEnumerable<User>> GetAsync()
        {
            return await _userManager.Users.AsNoTracking()
                                           .Include(user => user.Friends)
                                           .Include(user => user.News)
                                                .ThenInclude(news => news.Likes)
                                           .Include(user => user.News)
                                                .ThenInclude(news => news.Comments)
                                                    .ThenInclude(comment => comment.Likes)
                                           .Include(user => user.Groups)
                                                .ThenInclude(groupUser => groupUser.Group)
                                           .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetAsync(Expression<Func<User, bool>> expression)
        {
            return await _userManager.Users.AsNoTracking()
                                           .Include(user => user.Friends)
                                           .Include(user => user.News)
                                                .ThenInclude(news => news.Likes)
                                           .Include(user => user.News)
                                                .ThenInclude(news => news.Comments)
                                                    .ThenInclude(comment => comment.Likes)
                                           .Include(user => user.Groups)
                                                .ThenInclude(groupUser => groupUser.Group)
                                           .Where(expression)
                                           .ToListAsync();
        }

        public async Task<User?> FirstOrDefaultAsync(Expression<Func<User, bool>> expression)
        {
            return await _userManager.Users.AsNoTracking()
                                           .Include(user => user.Friends)
                                           .Include(user => user.News)
                                                .ThenInclude(news => news.Likes)
                                           .Include(user => user.News)
                                                .ThenInclude(news => news.Comments)
                                                    .ThenInclude(comment => comment.Likes)
                                           .Include(user => user.Groups)
                                                .ThenInclude(groupUser => groupUser.Group)
                                           .FirstOrDefaultAsync(expression);
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
