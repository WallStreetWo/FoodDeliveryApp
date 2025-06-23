using FoodDeliveryApp.Data;
using FoodDeliveryApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace FoodDeliveryApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Constructor to get the database context via dependency injection
        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Create a new ViewModel to hold our data
            var viewModel = new HomeViewModel
            {
                // Get all categories from the database
                Categories = await _context.Categories.ToListAsync(),
                
                // Get some restaurants to feature (e.g., the first 8)
                FeaturedRestaurants = await _context.Restaurants.Take(8).ToListAsync(),
                
                // Get the top-rated restaurants (we'll simulate this for now)
                TopRatedRestaurants = await _context.Restaurants.OrderByDescending(r => r.RestaurantId).Take(8).ToListAsync()
            };

            // Pass the fully populated ViewModel to the View
            return View(viewModel);
        }
    }
}