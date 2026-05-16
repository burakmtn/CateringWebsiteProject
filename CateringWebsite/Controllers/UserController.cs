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
            .OrderByDescending(item => item.CreatedAt)
            .ToListAsync();

        return View(menuItems);
    }
}
