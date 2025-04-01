using Homework.DBEntities;
using Homework.DTOs.Discount;
using System.ComponentModel.DataAnnotations;

namespace Homework.Models
{
    public class Discount : BaseEntity
    {
        public required int ProductID { get; set; }
        public required string ProductName { get; set; }
        public bool Available { get; set; }
        [Range(0, 100)]
        public decimal? DiscountPercentile { get; set; }
        [Range(0, float.MaxValue)]
        public decimal? DiscountFlat { get; set; }
        public int UsedAmountOfTimes { get; set; }
        public bool IsFlat { get; set; }
    }
}
