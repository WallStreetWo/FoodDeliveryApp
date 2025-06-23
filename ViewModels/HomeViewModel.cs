using FoodDeliveryApp.Models;
using System.Collections.Generic;

namespace FoodDeliveryApp.ViewModels
{
    public class HomeViewModel
    {
        public List<Category> Categories { get; set; }
        public List<Restaurant> FeaturedRestaurants { get; set; }
        public List<Restaurant> TopRatedRestaurants { get; set; }
        // We can add more lists here later, like 'Promotions' or 'Newest'
    }
}