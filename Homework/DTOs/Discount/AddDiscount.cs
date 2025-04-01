using System.ComponentModel.DataAnnotations;

namespace Homework.DTOs.Discount
{
    public class AddDiscount
    {
        [Required]
        public required int productID { get; set; }
        [Required]
        [StringLength(1)]
        public required string productName { get; set; }
        public required bool available { get; set; }
        public decimal? discountPercentile { get; set; }
        public decimal? discountFlat { get; set; }
    }
}
