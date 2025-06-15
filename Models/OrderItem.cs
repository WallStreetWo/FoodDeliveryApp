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
        public virtual Order Order { get; set; }
        public virtual MenuItem MenuItem { get; set; }

    }
}