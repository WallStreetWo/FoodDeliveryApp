using System.Globalization;
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
    [Authorize(Roles = AppRoles.CustomerOrAdmin)]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ShoppingCartService _cartService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;

        public OrderController(
            ApplicationDbContext context,
            ShoppingCartService cartService,
            UserManager<ApplicationUser> userManager,
            INotificationService notificationService)
        {
            _context = context;
            _cartService = cartService;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Restaurant)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);

            var order = await _context.Orders
                .Include(o => o.Restaurant)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.StatusHistory)
                    .ThenInclude(h => h.ChangedByUser)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            order.StatusHistory = order.StatusHistory
                .OrderBy(h => h.ChangedAt)
                .ToList();

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(
            string deliveryAddress,
            string? customerPhoneNumber,
            string? deliveryInstructions,
            string? deliveryLatitude,
            string? deliveryLongitude)
        {
            var cartItems = _cartService.GetCartItems();

            if (cartItems == null || !cartItems.Any())
            {
                TempData["OrderError"] = "Your cart is empty. Add items before placing an order.";
                return RedirectToAction("Index", "Checkout");
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            if (string.IsNullOrWhiteSpace(deliveryAddress))
            {
                TempData["OrderError"] = "Please enter your delivery address before placing the order.";
                return RedirectToAction("Index", "Checkout");
            }

            decimal? parsedLatitude = null;
            decimal? parsedLongitude = null;

            if (!string.IsNullOrWhiteSpace(deliveryLatitude) &&
                decimal.TryParse(
                    deliveryLatitude,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var latitudeValue))
            {
                if (latitudeValue >= -90 && latitudeValue <= 90)
                {
                    parsedLatitude = latitudeValue;
                }
            }

            if (!string.IsNullOrWhiteSpace(deliveryLongitude) &&
                decimal.TryParse(
                    deliveryLongitude,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var longitudeValue))
            {
                if (longitudeValue >= -180 && longitudeValue <= 180)
                {
                    parsedLongitude = longitudeValue;
                }
            }

            var menuItemIds = cartItems.Select(x => x.MenuItemId).ToList();

            var menuItems = await _context.MenuItems
                .Where(m => menuItemIds.Contains(m.MenuItemId))
                .ToListAsync();

            if (menuItems.Count != menuItemIds.Count)
            {
                TempData["OrderError"] = "One or more cart items could not be found. Please refresh your cart and try again.";
                return RedirectToAction("Index", "Checkout");
            }

            var restaurantIds = menuItems
                .Select(m => m.RestaurantId)
                .Distinct()
                .ToList();

            if (restaurantIds.Count > 1)
            {
                TempData["OrderError"] = "Your cart contains items from multiple restaurants. Please keep one restaurant per order.";
                return RedirectToAction("Index", "Checkout");
            }

            var now = DateTime.UtcNow;

            var order = new Order
            {
                UserId = userId,
                RestaurantId = restaurantIds.First(),
                OrderDate = now,
                TotalAmount = cartItems.Sum(x => x.Total),
                Status = OrderStatuses.OrderPlaced,

                DeliveryAddress = deliveryAddress.Trim(),
                CustomerPhoneNumber = customerPhoneNumber?.Trim(),
                DeliveryInstructions = deliveryInstructions?.Trim(),
                DeliveryLatitude = parsedLatitude,
                DeliveryLongitude = parsedLongitude,

                OrderItems = new List<OrderItem>(),
                StatusHistory = new List<OrderStatusHistory>
                {
                    new OrderStatusHistory
                    {
                        Status = OrderStatuses.OrderPlaced,
                        ChangedByUserId = userId,
                        ChangedAt = now,
                        Comment = "Order was placed by the customer."
                    }
                }
            };

            foreach (var cartItem in cartItems)
            {
                var dbMenuItem = menuItems.First(m => m.MenuItemId == cartItem.MenuItemId);

                order.OrderItems.Add(new OrderItem
                {
                    MenuItemId = cartItem.MenuItemId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = dbMenuItem.Price
                });
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            await _notificationService.CreateAsync(
                userId,
                $"Order #{order.OrderId} placed",
                "Your order was placed successfully. We will notify you as it moves through the delivery process.",
                NotificationTypes.OrderPlaced,
                order.OrderId);

            await _notificationService.CreateForAdminsAsync(
                $"New order #{order.OrderId}",
                "A new order was placed and is waiting for processing.",
                NotificationTypes.OrderPlaced,
                order.OrderId);

            _cartService.ClearCart();

            TempData["OrderSuccess"] = $"Order #{order.OrderId} was placed successfully.";
            return RedirectToAction(nameof(Success), new { id = order.OrderId });
        }

        public async Task<IActionResult> Success(int id)
        {
            var userId = _userManager.GetUserId(User);

            var order = await _context.Orders
                .Include(o => o.Restaurant)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.StatusHistory)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}