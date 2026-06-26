using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Models
{
    public class Driver
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string BusNumber { get; set; }

        [Required]
        public string RouteNumber { get; set; }

        public bool Active { get; set; } = true;

        // Orders assigned to this driver
        public ICollection<Order> Orders { get; } = new List<Order>();
    }
}
