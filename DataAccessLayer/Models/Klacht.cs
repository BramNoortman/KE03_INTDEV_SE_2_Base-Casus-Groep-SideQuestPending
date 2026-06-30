using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Models
{
    public class Klacht
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Onderwerp { get; set; } = string.Empty;

        [Required]
        public string Beschrijving { get; set; } = string.Empty;

        public string Status { get; set; } = "Open";

        public DateTime AangemaaktOp { get; set; } = DateTime.Now;

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }
    }
}