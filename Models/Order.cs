namespace FoodDeliveryApp.Models
{
    public class Order
    {
        public int Id { get; set;}
        
        public string UserId { get; set;}

        public DateTime OrderDate { get; set;}

        public string Status { get; set;}

    }
}