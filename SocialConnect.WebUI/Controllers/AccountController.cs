using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SocialConnect.Domain.Interfaces;
using SocialConnect.WebUI.ViewModels;
using AutoMapper;
using SocialConnect.Domain.Entities;

namespace SocialConnect.WebUI.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepository _accountRepository;
        private readonly IMapper _mapper;

        public AccountController(IUserRepository accountRepository,
                                 IMapper mapper)
        {
            this._accountRepository = accountRepository;
            this._mapper = mapper;
        }

        #region Login

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if(!ModelState.IsValid)
            {
                return View(loginVM);
            }
            bool isSignedIn = await _accountRepository.LoginAsync(loginVM.Username, loginVM.Password);

            if(!isSignedIn)
            {
                return View(loginVM);
            }

            return RedirectToAction(nameof(Profile), new { loginVM.Username });
        }

        #endregion

        #region Register

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if(!ModelState.IsValid)
            {
                return View(registerVM);
            }
            User user = _mapper.Map<User>(registerVM);
            bool isRegistered = await _accountRepository.RegisterAsync(user, registerVM.Password);

            if (!isRegistered)
            {
                return View(registerVM);
            }

            return RedirectToAction(nameof(Login));
        }


        #endregion

        #region Profile

        [HttpGet("{action}/{username}")]
        public async Task<IActionResult> Profile(string username)
        {
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
            bool isSignedOut = await _accountRepository.LogoutAsync();

            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region Confirmation

        [HttpGet]
        public async Task<IActionResult> Confirmation(string userid, string token)
        {
            token = token.Replace(' ', '+');
            bool isConfirmed = await _accountRepository.ConfirmAsync(userid, token);
            if (!isConfirmed)
            {
                return BadRequest();
            }
            return RedirectToAction(nameof(Login));
        }

        #endregion
    }
}
