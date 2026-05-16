using CateringWebsite.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CateringWebsite.Controllers;

[Authorize(Roles = AppRoles.User)]
public class UserController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public UserController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IActionResult Index()
    {
        return RedirectToAction(nameof(Home));
    }

    public async Task<IActionResult> Home()
    {
        var menuItems = await _dbContext.MenuItems
            .Include(item => item.Caretaker)
            .Include(item => item.CustomizationOptions)
            .OrderByDescending(item => item.CreatedAt)
            .ToListAsync();

        return View(menuItems);
    }

    public async Task<IActionResult> Details(int id)
    {
        var menuItem = await _dbContext.MenuItems
            .Include(item => item.Caretaker)
            .Include(item => item.CustomizationOptions)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (menuItem is null)
        {
            return NotFound();
        }

        return View(menuItem);
    }
}
