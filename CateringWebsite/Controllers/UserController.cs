using CateringWebsite.Data;
using CateringWebsite.Models;
using CateringWebsite.Models.ViewModels;
using CateringWebsite.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CateringWebsite.Controllers;

[Authorize(Roles = AppRoles.User)]
public class UserController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IGoogleDistanceService _distanceService;
    private readonly GoogleMapsOptions _googleMapsOptions;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserController(
        ApplicationDbContext dbContext,
        IGoogleDistanceService distanceService,
        IOptions<GoogleMapsOptions> googleMapsOptions,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _distanceService = distanceService;
        _googleMapsOptions = googleMapsOptions.Value;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        return RedirectToAction(nameof(Home));
    }

    public async Task<IActionResult> Home(CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        var viewModel = new NearbyMenusViewModel
        {
            UserLocationAddress = user.LocationAddress,
            NearbyDistanceKm = _googleMapsOptions.NearbyDistanceKm,
            GoogleMapsConfigured = !string.IsNullOrWhiteSpace(_googleMapsOptions.ApiKey)
        };

        if (string.IsNullOrWhiteSpace(user.LocationAddress))
        {
            viewModel.StatusMessage = "Set your delivery location before browsing nearby menus.";
            return View(viewModel);
        }

        if (!viewModel.GoogleMapsConfigured)
        {
            viewModel.StatusMessage = "Google Maps API key is not configured, so nearby menus cannot be calculated.";
            return View(viewModel);
        }

        var menuItems = await _dbContext.MenuItems
            .Include(item => item.Caretaker)
            .Include(item => item.CustomizationOptions)
            .Where(item => item.Caretaker != null && item.Caretaker.LocationAddress != null)
            .OrderByDescending(item => item.CreatedAt)
            .ToListAsync(cancellationToken);

        var distanceByCaretaker = new Dictionary<string, DistanceMatrixResult>();
        var nearbyMenus = new List<NearbyMenuItemViewModel>();

        foreach (var menuItem in menuItems)
        {
            if (menuItem.Caretaker is null || string.IsNullOrWhiteSpace(menuItem.Caretaker.LocationAddress))
            {
                continue;
            }

            if (!distanceByCaretaker.TryGetValue(menuItem.CaretakerId, out var distance))
            {
                distance = await _distanceService.GetDrivingDistanceAsync(
                    user.LocationAddress,
                    menuItem.Caretaker.LocationAddress,
                    cancellationToken);
                distanceByCaretaker[menuItem.CaretakerId] = distance;
            }

            if (distance.IsSuccess &&
                distance.DistanceMeters <= _googleMapsOptions.NearbyDistanceKm * 1000)
            {
                nearbyMenus.Add(new NearbyMenuItemViewModel
                {
                    MenuItem = menuItem,
                    DistanceText = distance.DistanceText
                });
            }
        }

        viewModel.Menus = nearbyMenus;
        return View(viewModel);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        var menuItem = await _dbContext.MenuItems
            .Include(item => item.Caretaker)
            .Include(item => item.CustomizationOptions)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (menuItem is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(user.LocationAddress) ||
            string.IsNullOrWhiteSpace(menuItem.Caretaker?.LocationAddress) ||
            string.IsNullOrWhiteSpace(_googleMapsOptions.ApiKey))
        {
            return Forbid();
        }

        var distance = await _distanceService.GetDrivingDistanceAsync(
            user.LocationAddress,
            menuItem.Caretaker.LocationAddress,
            cancellationToken);

        if (!distance.IsSuccess ||
            distance.DistanceMeters > _googleMapsOptions.NearbyDistanceKm * 1000)
        {
            return Forbid();
        }

        return View(new MenuDetailsViewModel
        {
            MenuItem = menuItem,
            DistanceText = distance.DistanceText
        });
    }
}
