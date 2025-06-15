using Microsoft.AspNetCore.Identity;
using FoodDeliveryApp.Models;
using System.Collections.Generic;

public class ApplicationUser : IdentityUser
{
    public string FullName{ get; set; }
    public string Address { get; set; }

    //Navigation properties
    public virtual ICollection<Order> Orders { get; set; }
    public virtual ICollection<Review> Reviews { get; set; }

}