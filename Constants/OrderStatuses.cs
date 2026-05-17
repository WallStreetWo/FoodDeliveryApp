namespace FoodDeliveryApp.Constants
{
    public static class OrderStatuses
    {
        public const string OrderPlaced = "Order placed";
        public const string RestaurantAccepted = "Restaurant accepted";
        public const string PreparingFood = "Preparing food";
        public const string DriverAssigned = "Driver assigned";
        public const string PickedUp = "Picked up";
        public const string OnTheWay = "On the way";
        public const string Delivered = "Delivered";
        public const string Cancelled = "Cancelled";

        public static readonly string[] TrackingFlow =
        {
            OrderPlaced,
            RestaurantAccepted,
            PreparingFood,
            DriverAssigned,
            PickedUp,
            OnTheWay,
            Delivered
        };

        public static readonly string[] AllowedStatuses =
        {
            OrderPlaced,
            RestaurantAccepted,
            PreparingFood,
            DriverAssigned,
            PickedUp,
            OnTheWay,
            Delivered,
            Cancelled
        };

        public static bool IsAllowed(string? status)
        {
            return !string.IsNullOrWhiteSpace(status)
                && AllowedStatuses.Contains(status);
        }
    }
}