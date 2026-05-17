using FoodDeliveryApp.Models;

namespace FoodDeliveryApp.ViewModels
{
    public class HomeViewModel
    {
        public List<Category> Categories { get; set; } = new();
        public List<Restaurant> FeaturedRestaurants { get; set; } = new();
        public List<Restaurant> TopRatedRestaurants { get; set; } = new();

        public int CategoryCount => Categories?.Count ?? 0;
        public int FeaturedRestaurantCount => FeaturedRestaurants?.Count ?? 0;
        public int TopRatedRestaurantCount => TopRatedRestaurants?.Count ?? 0;

        public bool HasCategories => CategoryCount > 0;
        public bool HasFeaturedRestaurants => FeaturedRestaurantCount > 0;
        public bool HasTopRatedRestaurants => TopRatedRestaurantCount > 0;
        public bool HasAnyRestaurants => HasFeaturedRestaurants || HasTopRatedRestaurants;
        public bool HasAnyHomeData => HasCategories || HasAnyRestaurants;
    }
}