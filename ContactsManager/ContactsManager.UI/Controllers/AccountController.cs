using ContactManagement.Controllers;
using ContactsManager.Core.Domain.IdentityEntities;
using Entities_DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace ContactsManager.UI.Controllers
{
    [Route("[controller]")]
    public class AccountController : Controller
    {

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [Route("[action]")]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {

            if (!ModelState.IsValid)
            {
                ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();

                return View(registerDTO);
            }

            ApplicationUser user = new ApplicationUser()
            {
                Email = registerDTO.Email,
                FirstName = registerDTO.FirstName,
                PhoneNumber = registerDTO.PhoneNumber,
                UserName = registerDTO.Email // using email as username for login
            };

            // result has status of user creation -- success or failure
            IdentityResult result = await _userManager.CreateAsync(user, registerDTO.Password); // create user with password (hashed here -- default)

            if (result.Succeeded)
            {
                // login him to generate cookie
                await _signInManager.SignInAsync(user, isPersistent: false); // isPersistent = false means cookie will expire when browser is closed
                return RedirectToAction(nameof(PersonsController.Index), "Persons");
            }

            // if user creation failed, add errors to model state
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError("Register", error.Description); // added in Validation summary
            }

            return View(registerDTO);
            
        }
    }
}
