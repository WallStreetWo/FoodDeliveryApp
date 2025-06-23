using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodDeliveryApp.Models
{
    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; }
        [Required]
        public int OrderId { get; set; }
        [Required]
        public int MenuItemId { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal UnitPrice { get; set; }
        [NotMapped]
        public decimal Total => Quantity * UnitPrice;

        // CORRECTED
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }
        [ForeignKey("MenuItemId")]
        public virtual MenuItem MenuItem { get; set; }
    }
}