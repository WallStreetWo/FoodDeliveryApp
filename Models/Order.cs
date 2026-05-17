using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FoodDeliveryApp.Constants;

namespace FoodDeliveryApp.Models
{
    public class Order
    {
        public int OrderId { get; set; }

        public DateTime OrderDate { get; set; }

        public string UserId { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = OrderStatuses.OrderPlaced;

        public int RestaurantId { get; set; }

        public string? DriverId { get; set; }

        [StringLength(300)]
        public string? DeliveryAddress { get; set; }

        [StringLength(100)]
        public string? CustomerPhoneNumber { get; set; }

        [StringLength(500)]
        public string? DeliveryInstructions { get; set; }

        public decimal? DeliveryLatitude { get; set; }

        public decimal? DeliveryLongitude { get; set; }

        [ForeignKey(nameof(RestaurantId))]
        public virtual Restaurant Restaurant { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; } = null!;

        [ForeignKey(nameof(DriverId))]
        public virtual ApplicationUser? Driver { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public virtual ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
    }
}