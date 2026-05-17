using FoodDeliveryApp.Constants;
using FoodDeliveryApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodDeliveryApp.Controllers
{
    [Authorize(Roles = AppRoles.CustomerOrAdmin)]
    public class CheckoutController : Controller
    {
        private readonly ShoppingCartService _cartService;

        public CheckoutController(ShoppingCartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        [Route("Checkout")]
        [Route("Checkout/Index")]
        public IActionResult Index()
        {
            var cartItems = _cartService.GetCartItems();
            return View(cartItems);
        }
    }
}