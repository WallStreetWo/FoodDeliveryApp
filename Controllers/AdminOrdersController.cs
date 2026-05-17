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
    [Authorize(Roles = AppRoles.Admin)]
    public class AdminOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;

        private static readonly string[] AllowedStatuses = OrderStatuses.AllowedStatuses;

        public AdminOrdersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index(string status = "", string search = "")
        {
            var query = _context.Orders
                .AsNoTracking()
                .Include(o => o.User)
                .Include(o => o.Driver)
                .Include(o => o.Restaurant)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(o => o.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var trimmed = search.Trim().ToLower();

                query = query.Where(o =>
                    o.OrderId.ToString().Contains(trimmed) ||
                    (o.User.FullName ?? "").ToLower().Contains(trimmed) ||
                    (o.User.Email ?? "").ToLower().Contains(trimmed) ||
                    (o.Driver != null && (
                        (o.Driver.FullName ?? "").ToLower().Contains(trimmed) ||
                        (o.Driver.Email ?? "").ToLower().Contains(trimmed)
                    )) ||
                    (o.Restaurant.Name ?? "").ToLower().Contains(trimmed));
            }

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            ViewBag.AllowedStatuses = AllowedStatuses;
            ViewBag.CurrentStatus = status;
            ViewBag.Search = search;
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.PendingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatuses.OrderPlaced);
            ViewBag.PreparingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatuses.PreparingFood);
            ViewBag.OutForDeliveryOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatuses.OnTheWay);

            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.User)
                .Include(o => o.Driver)
                .Include(o => o.Restaurant)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.StatusHistory)
                    .ThenInclude(h => h.ChangedByUser)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            order.StatusHistory = order.StatusHistory
                .OrderBy(h => h.ChangedAt)
                .ToList();

            var drivers = await _userManager.GetUsersInRoleAsync(AppRoles.Driver);

            ViewBag.AllowedStatuses = AllowedStatuses;
            ViewBag.AvailableDrivers = drivers
                .OrderBy(d => d.FullName)
                .ThenBy(d => d.Email)
                .ToList();

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(
            int id,
            string status,
            string returnStatus = "",
            string search = "",
            bool returnToDetails = false)
        {
            if (!OrderStatuses.IsAllowed(status))
            {
                TempData["AdminOrderError"] = "Invalid status selected.";

                if (returnToDetails)
                {
                    return RedirectToAction(nameof(Details), new { id });
                }

                return RedirectToAction(nameof(Index), new { status = returnStatus, search });
            }

            var order = await _context.Orders
                .Include(o => o.Driver)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                TempData["AdminOrderError"] = "Order not found.";
                return RedirectToAction(nameof(Index), new { status = returnStatus, search });
            }

            if (order.Status == status)
            {
                TempData["AdminOrderSuccess"] = $"Order #{order.OrderId} is already marked as {status}.";

                if (returnToDetails)
                {
                    return RedirectToAction(nameof(Details), new { id });
                }

                return RedirectToAction(nameof(Index), new { status = returnStatus, search });
            }

            var adminUserId = _userManager.GetUserId(User);

            order.Status = status;

            _context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = order.OrderId,
                Status = status,
                ChangedByUserId = adminUserId,
                ChangedAt = DateTime.UtcNow,
                Comment = $"Status updated to {status} by admin."
            });

            await _context.SaveChangesAsync();

            await _notificationService.CreateAsync(
                order.UserId,
                $"Order #{order.OrderId} status updated",
                $"Your order status is now: {status}.",
                NotificationTypes.OrderStatusUpdated,
                order.OrderId);

            if (!string.IsNullOrWhiteSpace(order.DriverId))
            {
                await _notificationService.CreateAsync(
                    order.DriverId,
                    $"Order #{order.OrderId} status updated",
                    $"The order assigned to you is now: {status}.",
                    NotificationTypes.OrderStatusUpdated,
                    order.OrderId);
            }

            TempData["AdminOrderSuccess"] = $"Order #{order.OrderId} updated to {status}.";

            if (returnToDetails)
            {
                return RedirectToAction(nameof(Details), new { id });
            }

            return RedirectToAction(nameof(Index), new { status = returnStatus, search });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDriver(int id, string driverId)
        {
            if (string.IsNullOrWhiteSpace(driverId))
            {
                TempData["AdminOrderError"] = "Please select a driver.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var order = await _context.Orders
                .Include(o => o.Driver)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status == OrderStatuses.Cancelled)
            {
                TempData["AdminOrderError"] = "You cannot assign a driver to a cancelled order.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (order.Status == OrderStatuses.Delivered)
            {
                TempData["AdminOrderError"] = "You cannot assign a driver to an already delivered order.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var driver = await _userManager.FindByIdAsync(driverId);

            if (driver == null)
            {
                TempData["AdminOrderError"] = "Selected driver could not be found.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var isDriver = await _userManager.IsInRoleAsync(driver, AppRoles.Driver);

            if (!isDriver)
            {
                TempData["AdminOrderError"] = "Selected user is not a driver.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (order.DriverId == driverId)
            {
                TempData["AdminOrderSuccess"] = $"Order #{order.OrderId} is already assigned to this driver.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var adminUserId = _userManager.GetUserId(User);
            var now = DateTime.UtcNow;

            order.DriverId = driverId;

            if (ShouldMoveToDriverAssigned(order.Status))
            {
                order.Status = OrderStatuses.DriverAssigned;

                _context.OrderStatusHistories.Add(new OrderStatusHistory
                {
                    OrderId = order.OrderId,
                    Status = OrderStatuses.DriverAssigned,
                    ChangedByUserId = adminUserId,
                    ChangedAt = now,
                    Comment = $"Driver assigned: {driver.FullName ?? driver.Email}."
                });
            }
            else
            {
                _context.OrderStatusHistories.Add(new OrderStatusHistory
                {
                    OrderId = order.OrderId,
                    Status = order.Status,
                    ChangedByUserId = adminUserId,
                    ChangedAt = now,
                    Comment = $"Driver changed/assigned: {driver.FullName ?? driver.Email}."
                });
            }

            await _context.SaveChangesAsync();

            await _notificationService.CreateAsync(
                order.UserId,
                $"Driver assigned for order #{order.OrderId}",
                $"{driver.FullName ?? driver.Email} has been assigned to your delivery.",
                NotificationTypes.DriverAssigned,
                order.OrderId);

            await _notificationService.CreateAsync(
                driver.Id,
                $"New delivery assigned: Order #{order.OrderId}",
                "You have been assigned a new SnackDash delivery.",
                NotificationTypes.DriverAssigned,
                order.OrderId);

            TempData["AdminOrderSuccess"] = $"Driver assigned to order #{order.OrderId}.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnassignDriver(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Driver)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(order.DriverId))
            {
                TempData["AdminOrderSuccess"] = $"Order #{order.OrderId} has no driver assigned.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (order.Status == OrderStatuses.PickedUp ||
                order.Status == OrderStatuses.OnTheWay ||
                order.Status == OrderStatuses.Delivered)
            {
                TempData["AdminOrderError"] = "You cannot unassign a driver after the order has been picked up or delivered.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var previousDriverId = order.DriverId;
            var previousDriverName = order.Driver?.FullName ?? order.Driver?.Email ?? "Previous driver";
            var adminUserId = _userManager.GetUserId(User);

            order.DriverId = null;

            _context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = order.OrderId,
                Status = order.Status,
                ChangedByUserId = adminUserId,
                ChangedAt = DateTime.UtcNow,
                Comment = $"Driver unassigned: {previousDriverName}."
            });

            await _context.SaveChangesAsync();

            await _notificationService.CreateAsync(
                order.UserId,
                $"Driver update for order #{order.OrderId}",
                "The driver assigned to your order has been updated. A new driver may be assigned shortly.",
                NotificationTypes.DriverUnassigned,
                order.OrderId);

            if (!string.IsNullOrWhiteSpace(previousDriverId))
            {
                await _notificationService.CreateAsync(
                    previousDriverId,
                    $"Delivery unassigned: Order #{order.OrderId}",
                    "This delivery has been unassigned from you.",
                    NotificationTypes.DriverUnassigned,
                    order.OrderId);
            }

            TempData["AdminOrderSuccess"] = $"Driver unassigned from order #{order.OrderId}.";
            return RedirectToAction(nameof(Details), new { id });
        }

        private static bool ShouldMoveToDriverAssigned(string currentStatus)
        {
            if (currentStatus == OrderStatuses.Cancelled ||
                currentStatus == OrderStatuses.Delivered)
            {
                return false;
            }

            var currentIndex = Array.IndexOf(OrderStatuses.TrackingFlow, currentStatus);
            var driverAssignedIndex = Array.IndexOf(OrderStatuses.TrackingFlow, OrderStatuses.DriverAssigned);

            if (currentIndex == -1)
            {
                return true;
            }

            return currentIndex < driverAssignedIndex;
        }
    }
}