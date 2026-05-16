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
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxImageBytes = 2 * 1024 * 1024;

    private readonly ApplicationDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;
    private readonly UserManager<ApplicationUser> _userManager;

    public CaretakerController(
        ApplicationDbContext dbContext,
        IWebHostEnvironment environment,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _environment = environment;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string search = "", int page = 1)
    {
        var caretakerId = _userManager.GetUserId(User);
        var caretaker = await _userManager.GetUserAsync(User);
        var query = _dbContext.MenuItems
            .Include(item => item.CustomizationOptions)
            .Where(item => item.CaretakerId == caretakerId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(item =>
                item.Name.Contains(search) ||
                item.Description.Contains(search));
        }

        var menuItems = await ToPagedListAsync(query.OrderByDescending(item => item.CreatedAt), search, page);
        var orderItems = _dbContext.OrderItems
            .Include(item => item.Order)
            .Where(item => item.CaretakerId == caretakerId);
        var caretakerReviews = _dbContext.OrderItemReviews
            .Where(review => review.CaretakerId == caretakerId);

        return View(new CaretakerDashboardViewModel
        {
            MenuItems = menuItems,
            MenuItemCount = await _dbContext.MenuItems.CountAsync(item => item.CaretakerId == caretakerId),
            ReceivedOrderCount = await orderItems.Select(item => item.OrderId).Distinct().CountAsync(),
            CompletedOrderCount = await orderItems
                .Where(item => item.Order != null && item.Order.Status == "Completed")
                .Select(item => item.OrderId)
                .Distinct()
                .CountAsync(),
            TotalRevenue = await orderItems.SumAsync(item => (decimal?)item.Subtotal) ?? 0m,
            AverageCaretakerRating = await caretakerReviews.AverageAsync(review => (double?)review.CaretakerRating),
            ReviewCount = await caretakerReviews.CountAsync(),
            LocationMissing = string.IsNullOrWhiteSpace(caretaker?.LocationAddress)
        });
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
        await ValidateImageAsync(model);

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
            ImagePath = await SaveImageAsync(model.ImageFile),
            CaretakerId = caretakerId
        };

        ApplyCustomizationOptions(menuItem, model);
        _dbContext.MenuItems.Add(menuItem);
        await _dbContext.SaveChangesAsync();

        TempData["StatusMessage"] = "Menu item created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var menuItem = await FindOwnedMenuItemAsync(id);
        if (menuItem is null)
        {
            return NotFound();
        }

        return View(ToFormModel(menuItem));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MenuItemFormViewModel model)
    {
        if (model.Id != id)
        {
            return BadRequest();
        }

        var menuItem = await FindOwnedMenuItemAsync(id);
        if (menuItem is null)
        {
            return NotFound();
        }

        model.ExistingImagePath = menuItem.ImagePath;
        await ValidateImageAsync(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        menuItem.Name = model.Name;
        menuItem.Price = model.Price;
        menuItem.Description = model.Description;

        var uploadedImagePath = await SaveImageAsync(model.ImageFile);
        if (uploadedImagePath is not null)
        {
            menuItem.ImagePath = uploadedImagePath;
        }

        menuItem.CustomizationOptions.Clear();
        ApplyCustomizationOptions(menuItem, model);

        await _dbContext.SaveChangesAsync();

        TempData["StatusMessage"] = "Menu item updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var menuItem = await FindOwnedMenuItemAsync(id);
        if (menuItem is null)
        {
            return NotFound();
        }

        _dbContext.MenuItems.Remove(menuItem);
        await _dbContext.SaveChangesAsync();

        TempData["StatusMessage"] = "Menu item deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<MenuItem?> FindOwnedMenuItemAsync(int id)
    {
        var caretakerId = _userManager.GetUserId(User);
        return await _dbContext.MenuItems
            .Include(item => item.CustomizationOptions)
            .FirstOrDefaultAsync(item => item.Id == id && item.CaretakerId == caretakerId);
    }

    private static MenuItemFormViewModel ToFormModel(MenuItem menuItem)
    {
        return new MenuItemFormViewModel
        {
            Id = menuItem.Id,
            Name = menuItem.Name,
            Price = menuItem.Price,
            Description = menuItem.Description,
            ExistingImagePath = menuItem.ImagePath,
            RemovableIngredientsText = ToOptionLines(menuItem, "Removable"),
            OptionalAdditionsText = ToOptionLines(menuItem, "Addition"),
            OptionGroupsText = ToOptionLines(menuItem, "Group")
        };
    }

    private static string? ToOptionLines(MenuItem menuItem, string optionType)
    {
        var lines = menuItem.CustomizationOptions
            .Where(option => option.OptionType == optionType)
            .OrderBy(option => option.GroupName)
            .ThenBy(option => option.Name)
            .Select(option => option.PriceChange == 0
                ? option.GroupName == option.Name ? option.Name : $"{option.GroupName}: {option.Name}"
                : $"{option.GroupName}: {option.Name} | {option.PriceChange}");

        var value = string.Join(Environment.NewLine, lines);
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static void ApplyCustomizationOptions(MenuItem menuItem, MenuItemFormViewModel model)
    {
        foreach (var option in ParseOptionLines(model.RemovableIngredientsText, "Removable ingredients", "Removable", allowPrices: false))
        {
            menuItem.CustomizationOptions.Add(option);
        }

        foreach (var option in ParseOptionLines(model.OptionalAdditionsText, "Additions", "Addition", allowPrices: true))
        {
            menuItem.CustomizationOptions.Add(option);
        }

        foreach (var option in ParseOptionLines(model.OptionGroupsText, "Options", "Group", allowPrices: true))
        {
            menuItem.CustomizationOptions.Add(option);
        }
    }

    private static IEnumerable<MenuCustomizationOption> ParseOptionLines(
        string? source,
        string defaultGroup,
        string optionType,
        bool allowPrices)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            yield break;
        }

        foreach (var rawLine in source.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var priceChange = 0m;
            var optionText = rawLine;

            var parts = rawLine.Split('|', 2, StringSplitOptions.TrimEntries);
            if (parts.Length == 2)
            {
                optionText = parts[0];
                if (allowPrices && decimal.TryParse(parts[1], out var parsedPrice))
                {
                    priceChange = parsedPrice;
                }
            }

            var groupName = defaultGroup;
            var optionName = optionText;
            var groupParts = optionText.Split(':', 2, StringSplitOptions.TrimEntries);
            if (groupParts.Length == 2)
            {
                groupName = groupParts[0];
                optionName = groupParts[1];
            }

            if (string.IsNullOrWhiteSpace(optionName))
            {
                continue;
            }

            yield return new MenuCustomizationOption
            {
                GroupName = groupName,
                Name = optionName,
                OptionType = optionType,
                PriceChange = priceChange
            };
        }
    }

    private async Task ValidateImageAsync(MenuItemFormViewModel model)
    {
        if (model.ImageFile is null)
        {
            return;
        }

        var extension = Path.GetExtension(model.ImageFile.FileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(extension))
        {
            ModelState.AddModelError(nameof(model.ImageFile), "Use a JPG, PNG, or WEBP image.");
        }

        if (model.ImageFile.Length > MaxImageBytes)
        {
            ModelState.AddModelError(nameof(model.ImageFile), "Image must be smaller than 2 MB.");
        }

        await Task.CompletedTask;
    }

    private static async Task<PagedListViewModel<MenuItem>> ToPagedListAsync(
        IQueryable<MenuItem> query,
        string search,
        int page,
        int pageSize = 10)
    {
        var pageNumber = Math.Max(1, page);
        var totalItems = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
        pageNumber = Math.Min(pageNumber, totalPages);

        return new PagedListViewModel<MenuItem>
        {
            Search = search,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            Items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync()
        };
    }

    private async Task<string?> SaveImageAsync(IFormFile? imageFile)
    {
        if (imageFile is null || imageFile.Length == 0)
        {
            return null;
        }

        var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var uploadDirectory = Path.Combine(_environment.WebRootPath, "uploads", "menu-items");
        Directory.CreateDirectory(uploadDirectory);

        var filePath = Path.Combine(uploadDirectory, fileName);
        await using var stream = System.IO.File.Create(filePath);
        await imageFile.CopyToAsync(stream);

        return $"/uploads/menu-items/{fileName}";
    }
}
