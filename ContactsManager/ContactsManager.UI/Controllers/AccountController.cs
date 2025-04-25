using ContactManagement.Controllers;
using ContactsManager.Core.Domain.IdentityEntities;
using Entities_DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Enums;

namespace ContactsManager.UI.Controllers
{
    [Route("[controller]")]
    
    public class AccountController : Controller
    {

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly RoleManager<ApplicationRole> _roleManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [Authorize("NotAuthorized")]
        [Route("[action]")]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        [Authorize("NotAuthorized")]
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
                // Check the user role
                if (registerDTO.UserType == Enums.UserRoles.ADMIN)
                {
                    // create admin role if role does not exist
                    if (await _roleManager.FindByNameAsync(UserRoles.ADMIN.ToString()) == null)
                    {
                        ApplicationRole applicationRole = new ApplicationRole()
                        {
                            Name = UserRoles.ADMIN.ToString(),
                            NormalizedName = UserRoles.ADMIN.ToString().ToUpper()
                        };
                        IdentityResult identityResult = await _roleManager.CreateAsync(applicationRole);
                        if (!identityResult.Succeeded)
                        {
                            foreach (IdentityError error in identityResult.Errors)
                            {
                                ModelState.AddModelError("Register", error.Description);
                            }
                            return View(registerDTO);
                        }
                       
                    }
                    // add new user to admin role
                    IdentityResult roleres = await _userManager.AddToRoleAsync(user, UserRoles.ADMIN.ToString());
                    if (!roleres.Succeeded)
                    {
                        foreach (IdentityError error in roleres.Errors)
                        {
                            ModelState.AddModelError("Register", error.Description);
                        }
                        return View(registerDTO);
                    }
                }
                else
                {
                    // check if user role exists and create
                    if (await _roleManager.FindByNameAsync(UserRoles.USER.ToString()) == null)
                    {
                        ApplicationRole applicationRole = new ApplicationRole()
                        {
                            Name = UserRoles.USER.ToString(),
                            NormalizedName = UserRoles.USER.ToString().ToUpper()
                        };

                        IdentityResult identityResult = await _roleManager.CreateAsync(applicationRole);

                        if (!identityResult.Succeeded)
                        {
                            foreach (IdentityError error in identityResult.Errors)
                            {
                                ModelState.AddModelError("Register", error.Description);
                            }
                            return View(registerDTO);
                        }
                    }
                    // add new user to user role
                    IdentityResult roleres = await _userManager.AddToRoleAsync(user, UserRoles.USER.ToString());
                    if (!roleres.Succeeded)
                    {
                        foreach (IdentityError error in roleres.Errors)
                        {
                            ModelState.AddModelError("Register", error.Description);
                        }
                        return View(registerDTO);
                    }
                }

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

        [Authorize("NotAuthorized")]
        [Route("[action]")]
        [HttpGet]
        public async Task<IActionResult> Login()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        [Authorize("NotAuthorized")]
        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO loginDTO, string? ReturnUrl)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();

                return View(loginDTO);
            }

            // check if password is correct
            Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(loginDTO.Email, loginDTO.Password, isPersistent: false, lockoutOnFailure: false); // after 3 failed attempts, user will be locked out for 5 minutes

            if (result.Succeeded)
            {
                ApplicationUser? applicationUser = await _userManager.FindByEmailAsync(loginDTO.Email);
                if(applicationUser != null)
                {
                    // check user role
                    if(await _userManager.IsInRoleAsync(applicationUser, UserRoles.ADMIN.ToString()))
                    {
                        return RedirectToAction("Index", "Home", new
                        {
                            area = "Admin" // redirect to admin area
                        });
                    }
                }

                if (!string.IsNullOrEmpty(ReturnUrl) || Url.IsLocalUrl(ReturnUrl))
                {
                    return LocalRedirect(ReturnUrl); // redirect to the page user was trying to access with in same application (security reason)
                }
                return RedirectToAction(nameof(PersonsController.Index), "Persons");
            }

            ModelState.AddModelError("Login", "Invalid Email Id or Password attempt"); // added in Validation summary
            return View(loginDTO);

        }

        [Authorize]
        [Route("[action]")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync(); // remove cookie
            return RedirectToAction(nameof(PersonsController.Index), "Persons");
        }


        [AllowAnonymous]
        [Route("[action]")]
        public async Task<IActionResult> IsEmailAlreadyExists(string? email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Json(false); // json because in async req the data is communicated through JSON
            }
            ApplicationUser? user = await _userManager.FindByEmailAsync(email); // check if email already exists in db
            if (user != null)
            {
                return Json(false); // email already exists
            }
            return Json(true);

        }

        // After login, if user tries to access a page which is not authorized for him, he will be redirected to this action
        [Authorize]
        [Route("[action]")]
        public async Task<IActionResult> AccessDenied(string? ReturnUrl)
        {
            return RedirectToAction(nameof(PersonsController.Index), "Persons");
        }
    }
}
