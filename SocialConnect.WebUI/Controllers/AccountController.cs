using Microsoft.AspNetCore.Mvc;
using SocialConnect.Entity.Dtos;
using SocialConnect.Domain.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SocialConnect.WebUI.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            this._accountService = accountService;
        }

        #region Login

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginUserDto loginDto)
        {
            if(!ModelState.IsValid)
            {
                return View(loginDto);
            }
            ResponseDto<UserDto> response = await _accountService.LoginAsync(loginDto);

            if(!response.IsSucceeded)
            {
                foreach(string error in response.Errors!)
                {
                    ModelState.AddModelError("", error);
                }
                return View(loginDto);
            }

            return RedirectToAction(nameof(Profile), new { loginDto.Username });
        }

        #endregion

        #region Register

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterUserDto registerDto)
        {
            if(!ModelState.IsValid)
            {
                return View(registerDto);
            }

            ResponseDto<UserDto> response = await _accountService.RegisterAsync(registerDto);

            if (!response.IsSucceeded)
            {
                foreach(string error in response.Errors!)
                {
                    ModelState.AddModelError("", error);
                }
                return View(registerDto);
            }

            return RedirectToAction(nameof(Profile), new { registerDto.Username });
        }


        #endregion

        #region Profile

        [HttpGet("{action}/{username}")]
        public async Task<IActionResult> Profile(string username)
        {
            ResponseDto<UserDto> response = await _accountService.FindByUsernameAsync(username);

            if(!response.IsSucceeded)
            {
                foreach(string error in response.Errors!)
                {
                    ModelState.AddModelError("", error);
                }
                return View();
            }

            return View(response.Content);
        }

        #endregion

        #region Logout

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            ResponseDto<DateTime> response = await _accountService.LogoutAsync();

            return RedirectToAction("Index", "Home");
        }

        #endregion

        public async Task<IActionResult> Delete()
        {
            ResponseDto<DateTime> response = await _accountService.DeleteAsync(User.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value);
            return RedirectToAction("Index", "Home");
        }
    }
}
