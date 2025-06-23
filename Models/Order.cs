using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodDeliveryApp.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";
        public int RestaurantId { get; set; }

        // CORRECTED
        [ForeignKey("RestaurantId")]
        public Restaurant Restaurant { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}