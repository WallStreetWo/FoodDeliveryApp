using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        //Operating Hours
        public string OpeningHours { get; set; }
        public string ClosingHours { get; set; }
        public virtual ICollection<Order> Orders { get; set; }

        //Navgation Properties - A restaurant can have many menu items
        public virtual ICollection<MenuItem> MenuItems { get; set; }
        // ADD THIS NAVIGATION PROPERTY
        public virtual ICollection<MenuCategory> MenuCategories { get; set; }

        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }


    }
}