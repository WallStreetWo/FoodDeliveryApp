using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace FoodDeliveryApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? Address { get; set; }

        // Navigation Properties
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
    }
}
