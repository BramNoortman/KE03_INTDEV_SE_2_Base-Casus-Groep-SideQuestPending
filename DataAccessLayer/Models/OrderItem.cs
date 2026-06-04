using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Models
{
    public class OrderItem
    {
        [Required]
        public int OrderId { get; set; }

        public Order? Order { get; set; }

        [Required]
        public int ProductId { get; set; }

        public Product? Product { get; set; }

        [Required]
        public int Quantity { get; set; }
    }
}
