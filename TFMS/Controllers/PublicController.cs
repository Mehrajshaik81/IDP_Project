using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TFMS.Controllers
{
    public class PublicController : Controller
    {
        // The [AllowAnonymous] attribute is explicitly added for clarity,
        // though it's the default if no [Authorize] is on the controller or action.
        // If your HomeController *had* [Authorize], this new controller needs to be explicitly anonymous.
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }
    }
}