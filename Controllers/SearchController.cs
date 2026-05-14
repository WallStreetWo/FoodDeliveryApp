using FoodDeliveryApp.Data;
using FoodDeliveryApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace FoodDeliveryApp.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Search
        // The searchString will come from the search bar form.
        public async Task<IActionResult> Index(string searchString)
        {
            // We'll pass the search string back to the view so we can display it.
            ViewData["CurrentFilter"] = searchString;

            // Start with a query for all restaurants.
            var restaurants = from r in _context.Restaurants
                              select r;

            // If a search string was provided, filter the results.
            if (!string.IsNullOrEmpty(searchString))
            {
                // This query will find restaurants where the search string appears in the
                // restaurant's Name, Description, OR in the name of one of its MenuItems.
                restaurants = restaurants.Where(r => 
                    r.Name.Contains(searchString) || 
                    r.Description.Contains(searchString) ||
                    r.MenuItems.Any(mi => mi.Name.Contains(searchString))
                );
            }

            // Execute the query and return the list of matching restaurants to the view.
            return View(await restaurants.ToListAsync());
        }
    }
}