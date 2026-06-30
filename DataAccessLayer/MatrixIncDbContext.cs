using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer
{
    public class MatrixIncDbContext : DbContext
    {
        public MatrixIncDbContext(DbContextOptions<MatrixIncDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Part> Parts { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Klacht> Klachten { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Customer → Orders (1:n, required)
            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Orders)
                .WithOne(o => o.Customer)
                .HasForeignKey(o => o.CustomerId).IsRequired();

            // Driver → Orders (1:n, optional - order may not have assigned driver)
            modelBuilder.Entity<Driver>()
                .HasMany(d => d.Orders)
                .WithOne(o => o.Driver)
                .HasForeignKey(o => o.DriverId);

            // Order items: composite key (OrderId + ProductId) to allow multiple products per order
            modelBuilder.Entity<OrderItem>()
                .HasKey(oi => new { oi.OrderId, oi.ProductId });

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.Orders)
                .HasForeignKey(oi => oi.ProductId);

            // Part ↔ Product (m:n relationship)
            modelBuilder.Entity<Part>()
                .HasMany(p => p.Products)
                .WithMany(p => p.Parts);

            //  Klacht Customer relation
            modelBuilder.Entity<Klacht>()
            .HasOne(k => k.Customer)
            .WithMany()
            .HasForeignKey(k => k.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

            base.OnModelCreating(modelBuilder);
        }
    }
}
