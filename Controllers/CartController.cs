using FoodDeliveryApp.Data;
using FoodDeliveryApp.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

// This attribute marks the class as an API controller.
// It enables features that make returning data easier.
[ApiController]
[Route("api/[controller]")] // This sets the base URL for this controller to be /api/Cart
public class CartController : ControllerBase
{
    private readonly ShoppingCartService _cartService;
    private readonly ApplicationDbContext _context;

    // We inject both our cart service and the database context.
    public CartController(ShoppingCartService cartService, ApplicationDbContext context)
    {
        _cartService = cartService;
        _context = context;
    }

    // This action will be triggered by a POST request to /api/Cart/AddToCart/{id}
    [HttpPost("AddToCart/{id}")]
    public async Task<IActionResult> AddToCart(int id)
    {
        // Find the menu item in the database
        var menuItem = await _context.MenuItems.FindAsync(id);

        if (menuItem == null)
        {
            // If the item doesn't exist, return a "Not Found" error.
            return NotFound(new { message = "Menu item not found." });
        }

        // Use our service to add the item to the cart stored in the session.
        _cartService.AddToCart(menuItem);

        // Return a success response. We can include a simple message.
        // Later, we can return the updated cart count here.
        return Ok(new { message = $"{menuItem.Name} was added to your cart." });


    }
    [HttpPost("ClearCart")]
    public IActionResult ClearCart()
    {
        _cartService.ClearCart();
        return Ok(new { message = "Cart has been cleared." });
    }

    
}