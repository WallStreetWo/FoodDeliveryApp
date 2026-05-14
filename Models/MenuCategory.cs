using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FoodDeliveryApp.Data;
using FoodDeliveryApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;


namespace FoodDeliveryApp.Models
{
    public class MenuCategory
    {
        public int MenuCategoryId { get; set; }

        [Required]
        public string Name { get; set; }

        // Foreign key to the Restaurant this category belongs to
        public int RestaurantId { get; set; }
        [ForeignKey("RestaurantId")]
        public virtual Restaurant Restaurant { get; set; }

        // A menu category has many menu items
        public virtual ICollection<MenuItem> MenuItems { get; set; }
    }
}