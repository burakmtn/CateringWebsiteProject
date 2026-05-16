using CateringWebsite.Data;
using CateringWebsite.Models;
using CateringWebsite.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CateringWebsite.Controllers;

public class AccountController : Controller
{
    private static readonly string[] RegisterableRoles = [AppRoles.User, AppRoles.Caretaker];

    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        return View(new RegisterViewModel { Role = AppRoles.User });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!RegisterableRoles.Contains(model.Role))
        {
            ModelState.AddModelError(nameof(model.Role), "Please choose a valid role.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            DisplayName = model.DisplayName,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            AddIdentityErrors(createResult);
            return View(model);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, model.Role);
        if (!roleResult.Succeeded)
        {
            AddIdentityErrors(roleResult);
            return View(model);
        }

        await _signInManager.SignInAsync(user, isPersistent: false);
        return await RedirectToRoleHomeAsync(user);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        return await RedirectToRoleHomeAsync(user);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }

    private async Task<IActionResult> RedirectToRoleHomeAsync(ApplicationUser user)
    {
        if (await _userManager.IsInRoleAsync(user, AppRoles.Admin))
        {
            return RedirectToAction(nameof(AdminController.Index), "Admin");
        }

        if (await _userManager.IsInRoleAsync(user, AppRoles.Caretaker))
        {
            return RedirectToAction(nameof(CaretakerController.Index), "Caretaker");
        }

        return RedirectToAction(nameof(UserController.Home), "User");
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}
