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
        public int Rtaing { get; set; }

        public string Comment { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        //Navigation Properties
        public virtual ApplicationUser User { get; set; }
        public virtual MenuItem MenuItem { get; set; }

    }
}
