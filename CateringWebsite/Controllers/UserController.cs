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
            GoogleMapsConfigured = !string.IsNullOrWhiteSpace(_googleMapsOptions.ApiKey),
            Stats = await BuildDashboardStatsAsync(user.Id)
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
        var ratingByMenu = await BuildMenuRatingLookupAsync(menuItems.Select(item => item.Id));
        var ratingByCaretaker = await BuildCaretakerRatingLookupAsync(menuItems.Select(item => item.CaretakerId));

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
                    DistanceText = distance.DistanceText,
                    MenuRating = ratingByMenu.GetValueOrDefault(menuItem.Id) ?? new RatingSummary(),
                    CaretakerRating = ratingByCaretaker.GetValueOrDefault(menuItem.CaretakerId) ?? new RatingSummary()
                });
            }
        }

        viewModel.Menus = nearbyMenus;
        viewModel.Stats.NearbyMenuCount = nearbyMenus.Count;
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
            DistanceText = distance.DistanceText,
            MenuRating = await GetMenuRatingAsync(menuItem.Id),
            CaretakerRating = await GetCaretakerRatingAsync(menuItem.CaretakerId)
        });
    }

    private async Task<Dictionary<int, RatingSummary>> BuildMenuRatingLookupAsync(IEnumerable<int> menuItemIds)
    {
        var ids = menuItemIds.Distinct().ToList();
        return await _dbContext.OrderItemReviews
            .Where(review => ids.Contains(review.MenuItemId))
            .GroupBy(review => review.MenuItemId)
            .Select(group => new
            {
                MenuItemId = group.Key,
                Average = group.Average(review => review.MenuRating),
                Count = group.Count()
            })
            .ToDictionaryAsync(
                item => item.MenuItemId,
                item => new RatingSummary { Average = item.Average, Count = item.Count });
    }

    private async Task<Dictionary<string, RatingSummary>> BuildCaretakerRatingLookupAsync(IEnumerable<string> caretakerIds)
    {
        var ids = caretakerIds.Distinct().ToList();
        return await _dbContext.OrderItemReviews
            .Where(review => ids.Contains(review.CaretakerId))
            .GroupBy(review => review.CaretakerId)
            .Select(group => new
            {
                CaretakerId = group.Key,
                Average = group.Average(review => review.CaretakerRating),
                Count = group.Count()
            })
            .ToDictionaryAsync(
                item => item.CaretakerId,
                item => new RatingSummary { Average = item.Average, Count = item.Count });
    }

    private async Task<RatingSummary> GetMenuRatingAsync(int menuItemId)
    {
        var ratings = await _dbContext.OrderItemReviews
            .Where(review => review.MenuItemId == menuItemId)
            .Select(review => review.MenuRating)
            .ToListAsync();

        return ratings.Count == 0
            ? new RatingSummary()
            : new RatingSummary { Average = ratings.Average(), Count = ratings.Count };
    }

    private async Task<RatingSummary> GetCaretakerRatingAsync(string caretakerId)
    {
        var ratings = await _dbContext.OrderItemReviews
            .Where(review => review.CaretakerId == caretakerId)
            .Select(review => review.CaretakerRating)
            .ToListAsync();

        return ratings.Count == 0
            ? new RatingSummary()
            : new RatingSummary { Average = ratings.Average(), Count = ratings.Count };
    }

    private async Task<UserDashboardStatsViewModel> BuildDashboardStatsAsync(string userId)
    {
        return new UserDashboardStatsViewModel
        {
            OrderCount = await _dbContext.Orders.CountAsync(order => order.UserId == userId),
            ReviewCount = await _dbContext.OrderItemReviews.CountAsync(review => review.UserId == userId),
            TotalSpent = await _dbContext.Orders
                .Where(order => order.UserId == userId)
                .SumAsync(order => (decimal?)order.TotalAmount) ?? 0m
        };
    }
}
