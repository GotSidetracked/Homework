using Homework.DBEntities;
using System.ComponentModel.DataAnnotations;

namespace Homework.Models
{
    public class Order : BaseEntity
    {
        public List<Invoice> OrderedItems { get; set; } = new List<Invoice>();
        public decimal Price { get; set; }
    }
}
