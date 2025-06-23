using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodDeliveryApp.Models
{
    public class MenuItem
    {
        public int MenuItemId { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        [ForeignKey("RestaurantId")]
        public int RestaurantId { get; set; }
        
        // This property DEFINITELY exists.
        public virtual Restaurant Restaurant { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
    }
}