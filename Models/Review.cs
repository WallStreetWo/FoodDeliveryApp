using System.ComponentModel.DataAnnotations;

namespace FoodDeliveryApp.Models
{
    public class Review
    {
        public int ReviewId { get; set; }
        [Required]
        public int MenuItemId { get; set; }
        [Required]
        public string UserId { get; set; }
        [Range(1, 5)]
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

        public virtual ApplicationUser User { get; set; }
        
        // CORRECTED
        public virtual MenuItem MenuItem { get; set; }
    }
}