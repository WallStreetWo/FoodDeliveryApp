using FoodDeliveryApp.Constants;
using FoodDeliveryApp.Data;
using FoodDeliveryApp.Models;
using FoodDeliveryApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDeliveryApp.Controllers
{
    [Authorize(Roles = AppRoles.Driver)]
    public class DriverOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;

        private static readonly string[] DriverAllowedStatuses =
        {
            OrderStatuses.PickedUp,
            OrderStatuses.OnTheWay,
            OrderStatuses.Delivered
        };

        public DriverOrdersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index()
        {
            var driverId = _userManager.GetUserId(User);

            var orders = await _context.Orders
                .AsNoTracking()
                .Include(o => o.User)
                .Include(o => o.Restaurant)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Where(o => o.DriverId == driverId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var driverId = _userManager.GetUserId(User);

            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.User)
                .Include(o => o.Restaurant)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.StatusHistory)
                    .ThenInclude(h => h.ChangedByUser)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.DriverId == driverId);

            if (order == null)
            {
                return NotFound();
            }

            order.StatusHistory = order.StatusHistory
                .OrderBy(h => h.ChangedAt)
                .ToList();

            ViewBag.DriverAllowedStatuses = DriverAllowedStatuses;

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            if (!DriverAllowedStatuses.Contains(status))
            {
                TempData["DriverOrderError"] = "Invalid driver status selected.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var driverId = _userManager.GetUserId(User);

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderId == id && o.DriverId == driverId);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status == OrderStatuses.Cancelled)
            {
                TempData["DriverOrderError"] = "This order has been cancelled and cannot be updated.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (order.Status == OrderStatuses.Delivered)
            {
                TempData["DriverOrderError"] = "This order has already been delivered.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (!IsForwardStatusChange(order.Status, status))
            {
                TempData["DriverOrderError"] = $"You cannot move this order from '{order.Status}' to '{status}'.";
                return RedirectToAction(nameof(Details), new { id });
            }

            order.Status = status;

            _context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = order.OrderId,
                Status = status,
                ChangedByUserId = driverId,
                ChangedAt = DateTime.UtcNow,
                Comment = $"Driver updated status to {status}."
            });

            await _context.SaveChangesAsync();

            await _notificationService.CreateAsync(
                order.UserId,
                $"Order #{order.OrderId} delivery update",
                $"Your order is now: {status}.",
                NotificationTypes.DriverDeliveryUpdated,
                order.OrderId);

            await _notificationService.CreateForAdminsAsync(
                $"Driver updated order #{order.OrderId}",
                $"The driver updated the delivery status to: {status}.",
                NotificationTypes.DriverDeliveryUpdated,
                order.OrderId);

            TempData["DriverOrderSuccess"] = $"Order #{order.OrderId} updated to {status}.";
            return RedirectToAction(nameof(Details), new { id });
        }

        private static bool IsForwardStatusChange(string currentStatus, string newStatus)
        {
            var currentIndex = Array.IndexOf(OrderStatuses.TrackingFlow, currentStatus);
            var newIndex = Array.IndexOf(OrderStatuses.TrackingFlow, newStatus);

            if (currentIndex == -1 || newIndex == -1)
            {
                return false;
            }

            return newIndex > currentIndex;
        }
    }
}