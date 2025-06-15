using System.ComponentModel.DataAnnotations;

namespace FoodDeliveryApp.Models
{
    public class Restaurant
    {
        public int RestaurantId { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        [Required]

        public string Address { get; set; }
        [Phone]

        public string PhoneNumber { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string LogoUrl { get; set; }
        public string Description { get; set; }

        //Navgation Properties - A restaurant can have many menu items
        public virtual ICollection <MenuItem> MenuItem { get; set; }


    }
}