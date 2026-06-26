using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    public class Order
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        public int CustomerId { get; set; }

        public Customer? Customer { get; set; }

        public int? DriverId { get; set; }

        // Navigation property for assigned driver (nullable - order may not have driver yet)
        public Driver? Driver { get; set; }

        // Delivery status: NogNietVerzonden (0), Onderweg (1), Bezorgd (2)
        public OrderStatus Status { get; set; } = OrderStatus.NogNietVerzonden;

        // Warehouse rack location: A, B, C, or D
        public char? Rack { get; set; }

        // Order items with quantity (many-to-many through OrderItem join table)
        public ICollection<OrderItem> Items { get; } = new List<OrderItem>();
    }
}
