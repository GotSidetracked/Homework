using Homework.DBEntities;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Homework.Models
{
    public class Invoice : BaseEntity
    {
        public int ProductId { get; set; }
        public int? OrderID { get; set; }
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
        public int? DiscountID { get; set; }
        public decimal? DiscountedPrice { get; set; }
    }
}
