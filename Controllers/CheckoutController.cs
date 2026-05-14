using FoodDeliveryApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace FoodDeliveryApp.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ShoppingCartService _cartService;

        public CheckoutController(ShoppingCartService cartService)
        {
            _cartService = cartService;
        }

        // This action will display the contents of the cart
        public IActionResult Index()
        {
            var cartItems = _cartService.GetCartItems();
            // We can pass the cart items directly to the view
            return View(cartItems);
        }

        // POST /api/Cart/RemoveFromCart/{id}
        [HttpPost("RemoveFromCart/{id}")]
        public IActionResult RemoveFromCart(int id)
        {
            _cartService.RemoveFromCart(id);
            return Ok(new { message = "Item quantity reduced." });
        }

        // POST /api/Cart/RemoveItemCompletely/{id}
        [HttpPost("RemoveItemCompletely/{id}")]
        public IActionResult RemoveItemCompletely(int id)
        {
            _cartService.RemoveItemCompletely(id);
            return Ok(new { message = "Item removed from cart." });
        }

        // GET /api/Cart/GetCartSummary
        // This will be useful for updating the UI
        [HttpGet("GetCartSumary")]
        public IActionResult GetCartSummary()
        {
            var itemCount = _cartService.GetCartCount();
            var cartTotal = _cartService.GetCartTotal();
            return Ok(new { count = itemCount, total = cartTotal });
        }

    }
}
