using System.ComponentModel.DataAnnotations.Schema;
namespace FoodDeliveryApp.Models
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }

        public int MenuItemId { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        //Navigation Properties
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        [ForeignKey("MenuItemId")]
        public virtual MenuItem MenuItem { get; set; }
    }
}