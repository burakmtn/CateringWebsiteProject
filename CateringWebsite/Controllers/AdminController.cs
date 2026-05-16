using CateringWebsite.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CateringWebsite.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class AdminController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
