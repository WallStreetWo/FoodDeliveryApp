namespace FoodDeliveryApp.Constants
{
    public static class AppRoles
    {
        public const string Admin = "Admin";
        public const string Customer = "Customer";
        public const string Driver = "Driver";

        public const string AdminPolicy = "AdminOnly";
        public const string CustomerPolicy = "CustomerOnly";
        public const string DriverPolicy = "DriverOnly";

        public const string CustomerOrAdmin = Customer + "," + Admin;
    }
}