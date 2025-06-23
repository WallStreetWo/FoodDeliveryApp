using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FoodDeliveryApp.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        [Required]
        public string Name { get; set; }
        
        // This will store the path to an icon, like '/images/categories/pizza.png'
        public string ImageUrl { get; set; }

        // A category can have many restaurants
        public virtual ICollection<Restaurant> Restaurants { get; set; }
    }
}