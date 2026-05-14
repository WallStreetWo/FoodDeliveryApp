using FoodDeliveryApp.Data;
using FoodDeliveryApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace FoodDeliveryApp.Controllers
{
    public class RestaurantController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RestaurantController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Restaurant (No changes here)
        public IActionResult Index()
        {
            var restaurants = _context.Restaurants.ToList();
            return View(restaurants);
        }

        // GET: /Restaurant/Create (No changes here)
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");
            return View();
        }

        // POST: /Restaurant/Create (No changes here)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Restaurant restaurant)
        {
            if (ModelState.IsValid)
            {
                _context.Add(restaurant);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name", restaurant.CategoryId);
            return View(restaurant);
        }

        // GET: /Restaurant/Details/5
        // This action fetches a single restaurant and its complete menu.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // This is the key query. It finds the restaurant by its ID and
            // .Include(r => r.MenuCategories) -> Gets all menu categories for this restaurant.
            // .ThenInclude(mc => mc.MenuItems) -> For each menu category, get all its menu items.
            var restaurant = await _context.Restaurants
                .Include(r => r.MenuCategories)
                    .ThenInclude(mc => mc.MenuItems)
                .FirstOrDefaultAsync(m => m.RestaurantId == id);

            // If no restaurant with that ID exists, return a 404 Not Found error.
            if (restaurant == null)
            {
                return NotFound();
            }

            // Pass the complete restaurant object (with all its nested data) to the view.
            return View(restaurant);
        }
        // --- NEW METHOD ENDS HERE ---
    }
}