using FoodDeliveryApp.Data;
using FoodDeliveryApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace FoodDeliveryApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ShoppingCartService _cartService;
        private readonly ApplicationDbContext _context;

        public CartController(ShoppingCartService cartService, ApplicationDbContext context)
        {
            _cartService = cartService;
            _context = context;
        }

        [HttpPost("AddToCart/{id:int}")]
        public IActionResult AddToCart(int id)
        {
            var menuItem = _context.MenuItems.FirstOrDefault(m => m.MenuItemId == id);

            if (menuItem == null)
            {
                return NotFound(new { message = "Menu item not found." });
            }

            _cartService.AddToCart(menuItem);
            return Ok(BuildCartResponse("Item added to cart."));
        }

        [HttpPost("RemoveFromCart/{id:int}")]
        public IActionResult RemoveFromCart(int id)
        {
            _cartService.RemoveFromCart(id);
            return Ok(BuildCartResponse("Item quantity reduced."));
        }

        [HttpPost("RemoveItemCompletely/{id:int}")]
        public IActionResult RemoveItemCompletely(int id)
        {
            _cartService.RemoveItemCompletely(id);
            return Ok(BuildCartResponse("Item removed from cart."));
        }

        [HttpPost("ClearCart")]
        public IActionResult ClearCart()
        {
            _cartService.ClearCart();
            return Ok(new
            {
                message = "Cart cleared.",
                count = 0,
                total = 0
            });
        }

        [HttpGet("GetCartSummary")]
        public IActionResult GetCartSummary()
        {
            return Ok(new
            {
                count = _cartService.GetCartCount(),
                total = _cartService.GetCartTotal()
            });
        }

        private object BuildCartResponse(string message)
        {
            return new
            {
                message,
                count = _cartService.GetCartCount(),
                total = _cartService.GetCartTotal()
            };
        }
    }
}