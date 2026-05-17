using FoodDeliveryApp.Models;
using FoodDeliveryApp.ViewModels;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace FoodDeliveryApp.Services
{
    public class ShoppingCartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string CartSessionKey = "ShoppingCart";

        private ISession Session =>
            _httpContextAccessor.HttpContext?.Session
            ?? throw new InvalidOperationException("Session is not available.");

        public ShoppingCartService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // Get all cart items from session
        public List<CartItemViewModel> GetCartItems()
        {
            var cartJson = Session.GetString(CartSessionKey);

            if (string.IsNullOrWhiteSpace(cartJson))
            {
                return new List<CartItemViewModel>();
            }

            return JsonSerializer.Deserialize<List<CartItemViewModel>>(cartJson)
                   ?? new List<CartItemViewModel>();
        }

        // Save all cart items back into session
        private void SaveCartItems(List<CartItemViewModel> cartItems)
        {
            var cartJson = JsonSerializer.Serialize(cartItems);
            Session.SetString(CartSessionKey, cartJson);
        }

        // Add a menu item to the cart
        public void AddToCart(MenuItem item)
        {
            var cartItems = GetCartItems();

            var cartItem = cartItems.FirstOrDefault(ci => ci.MenuItemId == item.MenuItemId);

            if (cartItem == null)
            {
                cartItems.Add(new CartItemViewModel
                {
                    MenuItemId = item.MenuItemId,
                    Name = item.Name,
                    UnitPrice = item.Price,
                    Quantity = 1,
                    ImageUrl = item.ImageUrl
                });
            }
            else
            {
                cartItem.Quantity++;
            }

            SaveCartItems(cartItems);
        }

        // Remove one quantity of an item.
        // If quantity reaches zero, remove the item completely.
        public int RemoveFromCart(int menuItemId)
        {
            var cartItems = GetCartItems();
            var cartItem = cartItems.FirstOrDefault(ci => ci.MenuItemId == menuItemId);

            var remainingQuantity = 0;

            if (cartItem != null)
            {
                if (cartItem.Quantity > 1)
                {
                    cartItem.Quantity--;
                    remainingQuantity = cartItem.Quantity;
                }
                else
                {
                    cartItems.Remove(cartItem);
                }

                SaveCartItems(cartItems);
            }

            return remainingQuantity;
        }

        // Remove an item completely from the cart
        public void RemoveItemCompletely(int menuItemId)
        {
            var cartItems = GetCartItems();
            var cartItem = cartItems.FirstOrDefault(ci => ci.MenuItemId == menuItemId);

            if (cartItem != null)
            {
                cartItems.Remove(cartItem);
                SaveCartItems(cartItems);
            }
        }

        // Clear the whole cart
        public void ClearCart()
        {
            SaveCartItems(new List<CartItemViewModel>());
        }

        // Get total quantity of items in the cart
        public int GetCartCount()
        {
            return GetCartItems().Sum(item => item.Quantity);
        }

        // Get total cart value
        public decimal GetCartTotal()
        {
            return GetCartItems().Sum(item => item.Total);
        }
    }
}