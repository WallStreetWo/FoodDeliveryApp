using FoodDeliveryApp.Constants;
using FoodDeliveryApp.Data;
using FoodDeliveryApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDeliveryApp.Controllers
{
    [Authorize]
    public class OrderTrackingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderTrackingController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Track(int id)
        {
            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole(AppRoles.Admin);
            var isDriver = User.IsInRole(AppRoles.Driver);

            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Restaurant)
                .Include(o => o.Driver)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.StatusHistory)
                    .ThenInclude(h => h.ChangedByUser)
                .AsQueryable();

            if (isAdmin)
            {
                // Admin can track any order.
            }
            else if (isDriver)
            {
                query = query.Where(o => o.DriverId == currentUserId);
            }
            else
            {
                query = query.Where(o => o.UserId == currentUserId);
            }

            var order = await query.FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            order.StatusHistory = order.StatusHistory
                .OrderBy(h => h.ChangedAt)
                .ToList();

            return View(order);
        }
    }
}