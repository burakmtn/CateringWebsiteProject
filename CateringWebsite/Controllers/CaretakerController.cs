using CateringWebsite.Data;
using CateringWebsite.Models;
using CateringWebsite.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CateringWebsite.Controllers;

[Authorize(Roles = AppRoles.Caretaker)]
public class CaretakerController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public CaretakerController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var caretakerId = _userManager.GetUserId(User);
        var menuItems = await _dbContext.MenuItems
            .Where(item => item.CaretakerId == caretakerId)
            .OrderByDescending(item => item.CreatedAt)
            .ToListAsync();

        return View(menuItems);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new MenuItemFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MenuItemFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var caretakerId = _userManager.GetUserId(User);
        if (caretakerId is null)
        {
            return Challenge();
        }

        var menuItem = new MenuItem
        {
            Name = model.Name,
            Price = model.Price,
            Description = model.Description,
            CaretakerId = caretakerId
        };

        _dbContext.MenuItems.Add(menuItem);
        await _dbContext.SaveChangesAsync();

        TempData["StatusMessage"] = "Menu item created.";
        return RedirectToAction(nameof(Index));
    }
}
