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
using SocialConnect.WebUI.Extenstions;

namespace SocialConnect.WebUI.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IMailKitEmailService _emailService;
        private readonly IMapper _mapper;

        public AccountController(IUserRepository userRepository,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IMailKitEmailService emailService,
            IMapper mapper)
        {
            this._userRepository = userRepository;
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._emailService = emailService;
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
            if (!ModelState.IsValid)
            {
                return View(loginVm);
            }

            IdentityUser? user = await _userManager.FindByNameAsync(loginVm.Username);
            if (user == null)
            {
                ModelState.AddModelError("", "Incorrect login/password.");
                return View(loginVm);
            }

            Microsoft.AspNetCore.Identity.SignInResult signInResult =
                await _signInManager.PasswordSignInAsync(user, loginVm.Password, true, true);

            if (!signInResult.Succeeded)
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
            if (!ModelState.IsValid)
            {
                return View(registerVm);
            }

            IdentityUser user = _mapper.Map<IdentityUser>(registerVm);
            IdentityResult registerResult = await _userManager.CreateAsync(user, registerVm.Password);

            if (!registerResult.Succeeded)
            {
                foreach (IdentityError error in registerResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(registerVm);
            }

            User createdUser = _mapper.Map<User>(registerVm);
            createdUser.Id = user.Id;
            await _userRepository.CreateAsync(createdUser);

            AlertBoxVM alertBox = new()
            {
                IsSucceeded = true,
                Title = "Successfully registered!",
                Content =
                    "You're successfully registered. Now you can login. We sent letter to your email with confirmation."
            };
            HttpContext.Session.SetJson("confirmation", alertBox);
            await SendConfirmLetterAsync(user.Id);

            return RedirectToAction(nameof(Login));
        }


        #endregion

        #region Profile

        [HttpGet("{controller}/{action}/{username?}")]
        public async Task<IActionResult> Profile(string? username)
        {
            if (username == null)
            {
                username = User.Identity?.Name ?? "";
            }

            User? user = await _userRepository.FindByUsernameAsync(username);

            if (user == null)
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

            return RedirectToAction("Login", "Account");
        }

        #endregion

        #region Confirmation

        [HttpGet]
        public async Task<IActionResult> Confirmation(string userId, string token)
        {
            token = token.Replace(' ', '+');

            IdentityUser? user = await _userManager.FindByIdAsync(userId);

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

            AlertBoxVM alertBox = new()
            {
                IsSucceeded = true,
                Title = "Successfully confirmed!",
                Content = "Your account is successfully confirmed."
            };
            HttpContext.Session.SetJson("confirmation", alertBox);

            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction(nameof(Profile));
            }
            
            return RedirectToAction(nameof(Login));
        }

        #endregion

        #region Settings

        [Authorize]
        public async Task<IActionResult> Settings()
        {
            string? userId = User.GetUserId();

            if (userId == null)
            {
                ErrorVM error = new()
                {
                    Title = "User error?!",
                    Content = "Try re-login."
                };
                return View("Error", error);
            }

            IdentityUser? user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ErrorVM error = new()
                {
                    Title = "User error?!",
                    Content = "Try re-login."
                };
                return View("Error", error);
            }

            return View(user);
        }

        #endregion

        #region Resend Letter

        [Authorize]
        public async Task<IActionResult> ResendConfirmationLetter()
        {
            await SendConfirmLetterAsync();

            return Redirect(Request.Headers["Referer"]);
        }

        #endregion

        #region Private methods

        private async Task SendConfirmLetterAsync(string? userId = "")
        {
            if (string.IsNullOrEmpty(userId))
            {
                userId = User.GetUserId();
            }

            if (userId == null)
            {
                return;
            }
            
            IdentityUser user = await _userManager.FindByIdAsync(userId);

            if (user.EmailConfirmed)
            {
                return;
            }
            
            string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            string url = Url.ActionLink(nameof(Confirmation), "Account", new { userId = user.Id, token }) ?? "";

            EmailMessage emailMessage = new EmailMessage()
            {
                Subject = "Confirm email",
                Content = $"<h1>Confirm email</h1> <a href='{url}'>Click here to confirm email</a>",
                Reciever = user.Email
            };

            _ = _emailService.SendAsync(emailMessage);

        }

        #endregion
    }
}
