using Homework.DBEntities;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Homework.Models
{
    public class Product : BaseEntity
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; }

        [Range(0.01, int.MaxValue)]
        public decimal Price { get; set; }
    }
}
