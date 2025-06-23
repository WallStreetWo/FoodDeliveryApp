using FoodDeliveryApp.Data;
using FoodDeliveryApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        // GET: /Restaurant
        // This will be a simple page listing all your restaurants
        public IActionResult Index()
        {
            var restaurants = _context.Restaurants.ToList();
            return View(restaurants);
        }

        // GET: /Restaurant/Create
        // This shows the form to create a new restaurant
        public IActionResult Create()
        {
            // We need to pass the list of categories to the view for a dropdown menu
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");
            return View();
        }

        // POST: /Restaurant/Create
        // This handles the form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Restaurant restaurant)
        {
            if (ModelState.IsValid)
            {
                _context.Add(restaurant);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); // Go back to the list after success
            }
            
            // If the model is not valid, show the form again with the entered data
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name", restaurant.CategoryId);
            return View(restaurant);
        }
    }
}