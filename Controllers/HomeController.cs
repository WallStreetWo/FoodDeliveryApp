using FoodDeliveryApp.Data;
using FoodDeliveryApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDeliveryApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (!(User?.Identity?.IsAuthenticated ?? false))
            {
                return Redirect("/Identity/Account/Login");
            }

            var categories = await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();

            var featuredRestaurants = await _context.Restaurants
                .AsNoTracking()
                .OrderBy(r => r.Name)
                .Take(8)
                .ToListAsync();

            var topRatedRestaurants = await _context.Restaurants
                .AsNoTracking()
                .OrderByDescending(r => r.RestaurantId)
                .Take(8)
                .ToListAsync();

            var viewModel = new HomeViewModel
            {
                Categories = categories,
                FeaturedRestaurants = featuredRestaurants,
                TopRatedRestaurants = topRatedRestaurants
            };

            return View(viewModel);
        }
    }
}