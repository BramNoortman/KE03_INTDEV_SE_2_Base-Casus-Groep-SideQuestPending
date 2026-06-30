using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public static class MatrixIncDbInitializer
    {
        public static void Initialize(MatrixIncDbContext context)
        {
            // Ensure database created
            context.Database.EnsureCreated();

            // TODO: Hier moet ik nog wat namen verzinnen die betrekking hebben op de matrix.
            // - Denk aan de m3 boutjes, moertjes en ringetjes.
            // - Denk aan namen van schepen
            // - Denk aan namen van vliegtuigen            
            var customers = new Customer[]
            {
                new Customer { Name = "Neo", Address = "123 Elm St" , Active=true},
                new Customer { Name = "Morpheus", Address = "456 Oak St", Active = true },
                new Customer { Name = "Trinity", Address = "789 Pine St", Active = true }
            };
            // Seed customers if missing
            if (!context.Customers.Any())
            {
                context.Customers.AddRange(customers);
                context.SaveChanges();
            }

            // Seed products if missing
            if (!context.Products.Any())
            {
                var products = new Product[]
                {
                    new Product { Name = "Nebuchadnezzar", Description = "Het schip waarop Neo voor het eerst de echte wereld leert kennen", Price = 10000.00m },
                    new Product { Name = "Jack-in Chair", Description = "Stoel met een rugsteun en metalen armen waarin mensen zitten om ingeplugd te worden in de Matrix via een kabel in de nekpoort", Price = 500.50m },
                    new Product { Name = "EMP (Electro-Magnetic Pulse) Device", Description = "Wapentuig op de schepen van Zion", Price = 129.99m }
                };
                context.Products.AddRange(products);
                context.SaveChanges();
            }

            // Seed parts if missing
            if (!context.Parts.Any())
            {
                var parts = new Part[]
                {
                    new Part { Name = "Tandwiel", Description = "Overdracht van rotatie in bijvoorbeeld de motor of luikmechanismen"},
                    new Part { Name = "M5 Boutje", Description = "Bevestiging van panelen, buizen of interne modules"},
                    new Part { Name = "Hydraulische cilinder", Description = "Openen/sluiten van zware luchtsluizen of bewegende onderdelen"},
                    new Part { Name = "Koelvloeistofpomp", Description = "Koeling van de motor of elektronische systemen."}
                };
                context.Parts.AddRange(parts);
                context.SaveChanges();
            }

            // Seed orders if missing (requires customers)
            if (!context.Orders.Any())
            {
                var existingCustomers = context.Customers.ToArray();
                if (existingCustomers.Length >= 3)
                {
                    var orders = new Order[]
                    {
                        new Order { Customer = existingCustomers[0], OrderDate = DateTime.Parse("2021-01-01")},
                        new Order { Customer = existingCustomers[0], OrderDate = DateTime.Parse("2021-02-01")},
                        new Order { Customer = existingCustomers[1], OrderDate = DateTime.Parse("2021-02-01")},
                        new Order { Customer = existingCustomers[2], OrderDate = DateTime.Parse("2021-03-01")}
                    };
                    context.Orders.AddRange(orders);
                    context.SaveChanges();

                    // link some products to the first order as example via OrderItem
                    var firstOrder = context.Orders.Include(o => o.Items).FirstOrDefault();
                    var firstProducts = context.Products.Take(2).ToList();
                    if (firstOrder != null && firstProducts.Any())
                    {
                        foreach (var p in firstProducts)
                        {
                            context.OrderItems.Add(new OrderItem { OrderId = firstOrder.Id, ProductId = p.Id, Quantity = 1 });
                        }
                        context.SaveChanges();
                    }
                }
            }

            // Seed drivers if missing
            if (!context.Drivers.Any())
            {
                var drivers = new Driver[]
                {
                    new Driver { Name = "Agent Smith", BusNumber = "M-001", RouteNumber = "R1", Active = true },
                    new Driver { Name = "Agent Johnson", BusNumber = "M-002", RouteNumber = "R2", Active = true },
                    new Driver { Name = "The Architect", BusNumber = "M-003", RouteNumber = "R3", Active = false },
                    new Driver { Name = "Oracle Courier", BusNumber = "M-004", RouteNumber = "R4", Active = true }
                };
                context.Drivers.AddRange(drivers);
                context.SaveChanges();
            }

            // Seed klachten if missing
            // Seed klachten if missing
            if (!context.Klachten.Any())
            {
                var existingCustomers = context.Customers.ToArray();

                if (existingCustomers.Length >= 3)
                {
                    var klachten = new Klacht[]
                    {
            new Klacht
            {
                Onderwerp = "Levering vertraagd in de Matrix",
                Beschrijving = "Pakket blijft hangen in virtuele routing layer",
                Status = "Open",
                AangemaaktOp = DateTime.Now.AddDays(-3),

                Customer = existingCustomers[0]
            },
            new Klacht
            {
                Onderwerp = "Defect EMP device ontvangen",
                Beschrijving = "Klant kreeg een beschadigd EMP apparaat",
                Status = "In behandeling",
                AangemaaktOp = DateTime.Now.AddDays(-1),

                Customer = existingCustomers[1]
            },
            new Klacht
            {
                Onderwerp = "Onjuiste levering",
                Beschrijving = "Verkeerde onderdelen geleverd bij order",
                Status = "Open",
                AangemaaktOp = DateTime.Now.AddDays(-5),

                Customer = existingCustomers[2]
            }
                    };

                    context.Klachten.AddRange(klachten);
                    context.SaveChanges();
                }
            }
        }
    }
}
