using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SocialConnect.Domain.Interfaces;
using SocialConnect.WebUI.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using SocialConnect.Domain.Entities;
using SocialConnect.Domain.Entities.Constants;
using SocialConnect.Infrastructure.Interfaces;
using SocialConnect.Shared.Models;

namespace SocialConnect.WebUI.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepository _accountRepository;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IMailKitEmailService _emailService;
        private readonly IMapper _mapper;

        public AccountController(IUserRepository accountRepository,
                                 UserManager<User> userManager,
                                 SignInManager<User> signInManager,
                                 IMailKitEmailService emailService,
                                 IMapper mapper)
        {
            this._accountRepository = accountRepository;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            this._mapper = mapper;
        }

        #region Login

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVm)
        {
            if(!ModelState.IsValid)
            {
                return View(loginVm);
            }

            User? user = await _userManager.FindByNameAsync(loginVm.Username);
            if (user == null)
            {
                ModelState.AddModelError("", "Incorrect login/password.");
                return View(loginVm);
            }
            Microsoft.AspNetCore.Identity.SignInResult signInResult = await _signInManager.PasswordSignInAsync(user, loginVm.Password, true, true);

            if(!signInResult.Succeeded)
            {
                ModelState.AddModelError("", "Incorrect login/password.");
                return View(loginVm);
            }

            return RedirectToAction(nameof(Profile), new { loginVm.Username });
        }

        #endregion

        #region Register

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVm)
        {
            if(!ModelState.IsValid)
            {
                return View(registerVm);
            }
            User user = _mapper.Map<User>(registerVm);
            IdentityResult registerResult = await _userManager.CreateAsync(user, registerVm.Password);

            if (!registerResult.Succeeded)
            {
                foreach (IdentityError error in registerResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(registerVm);
            }
            string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            string url = Url.ActionLink(nameof(Confirmation), "Account", new { userId = user.Id, token }) ?? "";
            
            EmailMessage emailMessage = new EmailMessage()
            {
                Subject = "Confirm email",
                Content = $"<h1>Confirm email</h1> <a href='{url}'>Click here to confirm email</a>",
                Reciever = user.Email
            };

            _emailService.SendAsync(emailMessage);
            return RedirectToAction(nameof(Login));
        }


        #endregion

        #region Profile

        [HttpGet("{action}/{username?}")]
        public async Task<IActionResult> Profile(string? username)
        {
            if(username == null)
            {
                username = User.Identity?.Name ?? "";
            }
            User? user = await _accountRepository.FindByUsernameAsync(username);

            if(user == null)
            {
                return View();
            }

            return View(user);
        }

        #endregion

        #region Logout

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region Confirmation

        [HttpGet]
        public async Task<IActionResult> Confirmation(string userId, string token)
        {
            token = token.Replace(' ', '+');
            
            User? user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return BadRequest();
            }

            IdentityResult confirmResult = await _userManager.ConfirmEmailAsync(user, token);

            if (!confirmResult.Succeeded)
            {
                return BadRequest();
            }
            
            await _userManager.AddToRoleAsync(user, UserConstant.USER);

            return RedirectToAction(nameof(Login));
        }

        #endregion
    }
}
