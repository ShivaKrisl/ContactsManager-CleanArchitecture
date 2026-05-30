using Entities_DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Enums;
using Service_Contracts;

namespace ContactsManager.UI.Controllers
{
    [Route("[controller]")]
    
    public class AccountController : Controller
    {

        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        [AllowAnonymous]
        [Route("[action]")]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return View(registerDTO);
            }

            AuthResult result = await _authService.RegisterAsync(registerDTO);
            if (!result.Succeeded)
            {
                foreach (string error in result.Errors)
                {
                    ModelState.AddModelError("Register", error);
                }

                return View(registerDTO);
            }

            if (result.IsAdmin)
            {
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }

            return RedirectToAction("Index", "Persons");

        }

        [AllowAnonymous]
        [Route("[action]")]
        [HttpGet]
        public async Task<IActionResult> Login()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO loginDTO, string? ReturnUrl)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();

                return View(loginDTO);
            }

            AuthResult result = await _authService.LoginAsync(loginDTO, ReturnUrl);

            if (result.Succeeded)
            {
                if (result.IsAdmin)
                {
                    return RedirectToAction("Index", "Home", new { area = "Admin" });
                }

                if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                {
                    return LocalRedirect(ReturnUrl); // redirect to the page user was trying to access with in same application (security reason)
                }
                return RedirectToAction("Index", "Persons");
            }

            foreach (string error in result.Errors)
            {
                ModelState.AddModelError("Login", error);
            }

            return View(loginDTO);

        }

        [Authorize]
        [Route("[action]")]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync(); // remove cookie
            return RedirectToAction("Index", "Persons");
        }


        [AllowAnonymous]
        [Route("[action]")]
        public async Task<IActionResult> IsEmailAlreadyExists(string? email)
        {
            return Json(await _authService.IsEmailAlreadyExistsAsync(email));

        }

        // After login, if user tries to access a page which is not authorized for him, he will be redirected to this action
        [Authorize]
        [Route("[action]")]
        public async Task<IActionResult> AccessDenied(string? ReturnUrl)
        {
            return RedirectToAction("Index", "Persons");
        }
    }
}
