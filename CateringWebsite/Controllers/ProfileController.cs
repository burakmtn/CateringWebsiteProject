using CateringWebsite.Data;
using CateringWebsite.Models;
using CateringWebsite.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CateringWebsite.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfileController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Location()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        return View(new LocationViewModel
        {
            LocationAddress = user.LocationAddress ?? string.Empty,
            RoleLabel = GetRoleLabel()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Location(LocationViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        model.RoleLabel = GetRoleLabel();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        user.LocationAddress = model.LocationAddress.Trim();
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        TempData["StatusMessage"] = "Location saved.";

        if (User.IsInRole(AppRoles.Caretaker))
        {
            return RedirectToAction(nameof(CaretakerController.Index), "Caretaker");
        }

        if (User.IsInRole(AppRoles.User))
        {
            return RedirectToAction(nameof(UserController.Home), "User");
        }

        return RedirectToAction(nameof(HomeController.Index), "Home");
    }

    private string GetRoleLabel()
    {
        if (User.IsInRole(AppRoles.Caretaker))
        {
            return "Service / restaurant address";
        }

        if (User.IsInRole(AppRoles.User))
        {
            return "Delivery address";
        }

        return "Location address";
    }
}
