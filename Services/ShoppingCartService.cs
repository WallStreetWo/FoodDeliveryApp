using FoodDeliveryApp.Models;
using FoodDeliveryApp.ViewModels;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace FoodDeliveryApp.Services
{
    public class ShoppingCartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ISession Session => _httpContextAccessor.HttpContext.Session;
        private const string CartSessionKey = "ShoppingCart";

        public ShoppingCartService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // Method to get the current list of items from the session
        public List<CartItemViewModel> GetCartItems()
        {
            var cartJson = Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(cartJson))
            {
                return new List<CartItemViewModel>();
            }
            return JsonSerializer.Deserialize<List<CartItemViewModel>>(cartJson);
        }

        // Method to save the list of items back to the session
        private void SaveCartItems(List<CartItemViewModel> cartItems)
        {
            var cartJson = JsonSerializer.Serialize(cartItems);
            Session.SetString(CartSessionKey, cartJson);
        }

        // Public method to add an item to the cart
        public void AddToCart(MenuItem item)
        {
            var cartItems = GetCartItems();

            // Check if the item is already in the cart
            var cartItem = cartItems.FirstOrDefault(ci => ci.MenuItemId == item.MenuItemId);

            if (cartItem == null)
            {
                // If not, add it as a new item
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
                // If it is, just increment the quantity
                cartItem.Quantity++;
            }

            SaveCartItems(cartItems);
        }

        // Removes one instance of an item. If quantity is 1, removes the item completely.
        public int RemoveFromCart(int menuItemId)
        {
            var cartItems = GetCartItems();
            var cartItem = cartItems.FirstOrDefault(ci => ci.MenuItemId == menuItemId);

            var remainingQuatity = 0;

            if (cartItem != null)
            {
                if (cartItem.Quantity > 1)
                {
                    cartItem.Quantity--;
                    remainingQuatity = cartItem.Quantity;
                }
                else
                {
                    cartItems.Remove(cartItem);
                }
                SaveCartItems(cartItems);
            }
            return remainingQuatity;
        }

        // Completely removes an item from the cart, regardless of quantity.
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

        // Empties the entire shopping cart
        public void ClearCart()
        {
            SaveCartItems(new List<CartItemViewModel>());
        }

        // Calculates the total number of items in the cart.
        public int GetCartCount()
        {
            return GetCartItems().Sum(item => item.Quantity);
        }

        // Calculates the total price of all items in the cart.
        public decimal GetCartTotal()
        {
            return GetCartItems().Sum(item => item.Total);
        }
    }
}