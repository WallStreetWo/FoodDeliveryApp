namespace FoodDeliveryApp.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }

        public string UserId { get; set; }

        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "Pending"; //Default state for Order Status

        //Navigation Properties
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }

    }
}