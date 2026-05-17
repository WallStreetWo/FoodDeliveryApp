using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FoodDeliveryApp.Constants;

namespace FoodDeliveryApp.Models
{
    public class OrderStatusHistory
    {
        [Key]
        public int OrderStatusHistoryId { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = OrderStatuses.OrderPlaced;

        public string? ChangedByUserId { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string? Comment { get; set; }

        [ForeignKey(nameof(OrderId))]
        public virtual Order Order { get; set; } = null!;

        [ForeignKey(nameof(ChangedByUserId))]
        public virtual ApplicationUser? ChangedByUser { get; set; }
    }
}