using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContactsManager.UI.Areas.Admin.Controllers
{
    [Area("Admin" )]
    [Authorize(Roles = "ADMIN")] // only admin can access this controller
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
