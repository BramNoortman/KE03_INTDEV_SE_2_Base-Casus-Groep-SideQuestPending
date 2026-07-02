using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer;
using DataAccessLayer.Models;

namespace KE03_INTDEV_SE_2_Base.Controllers
{
    public class OrderController : Controller
    {
        private readonly MatrixIncDbContext _context;
        private readonly Microsoft.Extensions.Logging.ILogger<OrderController> _logger;

        public OrderController(MatrixIncDbContext context, Microsoft.Extensions.Logging.ILogger<OrderController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Order
        public async Task<IActionResult> Index(
            string search,
            int? status,
            int? customerId,
            DateTime? dateFrom,
            DateTime? dateTo)
        {
            var query = _context.Orders
                .Include(o => o.Customer)
                .AsQueryable();

            // Tekstzoek
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();

                query = query.Where(o =>
                    o.Id.ToString().Contains(search) ||
                    o.OrderDate.ToString().Contains(search) ||
                    o.CustomerId.ToString().Contains(search) ||
                    (o.Customer.Name ?? "").ToLower().Contains(search) ||
                    o.Status.ToString().ToLower().Contains(search)
                );
            }

            // Filter op status
            if (status.HasValue)
            {
                query = query.Where(o => (int)o.Status == status.Value);
                ViewBag.Status = status.Value;
            }

            // Filter op klant
            if (customerId.HasValue)
            {
                query = query.Where(o => o.CustomerId == customerId.Value);
                ViewBag.CustomerId = customerId.Value;
            }

            // Filter op datum range
            if (dateFrom.HasValue)
            {
                var df = dateFrom.Value.Date;
                query = query.Where(o => o.OrderDate >= df);
                ViewBag.DateFrom = df.ToString("yyyy-MM-dd");
            }

            if (dateTo.HasValue)
            {
                var dt = dateTo.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(o => o.OrderDate <= dt);
                ViewBag.DateTo = dateTo.Value.ToString("yyyy-MM-dd");
            }

            // Voeg klantlijst en statuslijst toe voor de view
            var customers = await _context.Customers.ToListAsync();
            ViewData["CustomerListItems"] = customers;

            var statusItems = new[]
            {
                new { Value = (int)OrderStatus.NogNietVerzonden, Text = "Nog niet verzonden" },
                new { Value = (int)OrderStatus.InDeBus, Text = "In de bus" },
                new { Value = (int)OrderStatus.Onderweg, Text = "Onderweg" },
                new { Value = (int)OrderStatus.Bezorgd, Text = "Bezorgd" }
            };

            ViewData["StatusListItems"] = statusItems;

            ViewBag.Search = search;

            var orders = await query
                .OrderBy(o => o.Id)
                .ToListAsync();

            return View(orders);
        }

        // GET: Order/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Order/Create
        public async Task<IActionResult> Create()
        {
            var customers = await _context.Customers.ToListAsync();
            ViewData["CustomerList"] = new SelectList(customers, "Id", "Name");

            var products = await _context.Products.ToListAsync();
            ViewData["ProductListItems"] = products;

            // Status dropdown with Dutch labels (NogNietVerzonden=0, Onderweg=1, Bezorgd=2)
            var statusItems = new[]
            {
                new { Value = (int)OrderStatus.NogNietVerzonden, Text = "Nog niet verzonden" },
                new { Value = (int)OrderStatus.Onderweg, Text = "Onderweg" },
                new { Value = (int)OrderStatus.Bezorgd, Text = "Bezorgd" }
            };
            ViewData["StatusList"] = new SelectList(statusItems, "Value", "Text");

            // Rack assignment dropdown (A, B, C, D)
            var rackItems = new[]
            {
                new { Value = 'A', Text = "Rek A" },
                new { Value = 'B', Text = "Rek B" },
                new { Value = 'C', Text = "Rek C" },
                new { Value = 'D', Text = "Rek D" }
            };
            ViewData["RackList"] = new SelectList(rackItems, "Value", "Text");

            return View();
        }

        // POST: Order/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderDate,CustomerId,Status,Rack")] Order order)
        {
            _logger.LogInformation("Create POST received. Form values: {Form}", Request.Form.ToDictionary(k => k.Key, v => v.Value.ToString()));
            _logger.LogInformation("Bound Order: {@Order}", order);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState invalid on Create: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

                var customers = await _context.Customers.ToListAsync();
                ViewData["CustomerList"] = new SelectList(customers, "Id", "Name", order.CustomerId);

                var products = await _context.Products.ToListAsync();
                ViewData["ProductListItems"] = products;

                return View(order);
            }

            // Read quantities from form fields (format: quantity_{productId}) and create OrderItems
            // Only adds items with qty > 0 to avoid empty order lines
            var allProducts = await _context.Products.Select(p => p.Id).ToListAsync();

            foreach (var pid in allProducts)
            {
                var qtyStr = Request.Form[$"quantity_{pid}"].FirstOrDefault();

                if (int.TryParse(qtyStr, out var qty) && qty > 0)
                {
                    order.Items.Add(new OrderItem
                    {
                        ProductId = pid,
                        Quantity = qty
                    });
                }
            }

            _context.Add(order);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Order created with Id {Id} and {Count} items", order.Id, order.Items.Count);

            return RedirectToAction(nameof(Index));
        }

        // GET: Order/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            var customers = await _context.Customers.ToListAsync();
            ViewData["CustomerList"] = new SelectList(customers, "Id", "Name", order.CustomerId);

            var products = await _context.Products.ToListAsync();
            ViewData["ProductListItems"] = products;

            var statusItems = new[]
            {
                new { Value = (int)OrderStatus.NogNietVerzonden, Text = "Nog niet verzonden" },
                new { Value = (int)OrderStatus.Onderweg, Text = "Onderweg" },
                new { Value = (int)OrderStatus.Bezorgd, Text = "Bezorgd" }
            };
            ViewData["StatusList"] = new SelectList(statusItems, "Value", "Text", (int)order.Status);

            var rackItems = new[]
            {
                new { Value = 'A', Text = "Rek A" },
                new { Value = 'B', Text = "Rek B" },
                new { Value = 'C', Text = "Rek C" },
                new { Value = 'D', Text = "Rek D" }
            };
            ViewData["RackList"] = new SelectList(rackItems, "Value", "Text", order.Rack);

            return View(order);
        }

        // POST: Order/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,OrderDate,CustomerId,Status,Rack")] Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            _logger.LogInformation("Edit POST received for id {Id}. Form: {Form}", id, Request.Form.ToDictionary(k => k.Key, v => v.Value.ToString()));
            _logger.LogInformation("Bound Order: {@Order}", order);

            if (ModelState.IsValid)
            {
                try
                {
                    var existingOrder = await _context.Orders
                        .Include(o => o.Items)
                        .FirstOrDefaultAsync(o => o.Id == id);

                    if (existingOrder == null)
                    {
                        return NotFound();
                    }

                    // update scalar
                    existingOrder.OrderDate = order.OrderDate;
                    existingOrder.CustomerId = order.CustomerId;
                    existingOrder.Status = order.Status;
                    existingOrder.Rack = order.Rack;

                    // read quantities for all products and replace items accordingly
                    var allProducts = await _context.Products.Select(p => p.Id).ToListAsync();
                    var newItems = new List<OrderItem>();

                    foreach (var pid in allProducts)
                    {
                        var packed = Request.Form[$"packed_{pid}"] == "true";
                        var qtyStr = Request.Form[$"quantity_{pid}"].FirstOrDefault();

                        if (int.TryParse(qtyStr, out var qty) && qty > 0)
                        {
                            newItems.Add(new OrderItem
                            {
                                ProductId = pid,
                                Quantity = qty,
                                Packed = packed
                            });
                        }
                    }

                    // replace items
                    existingOrder.Items.Clear();

                    foreach (var it in newItems)
                    {
                        existingOrder.Items.Add(it);
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                _logger.LogInformation("Order {Id} updated", order.Id);

                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("ModelState invalid on Edit: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

            var customers2 = await _context.Customers.ToListAsync();
            ViewData["CustomerList"] = new SelectList(customers2, "Id", "Name", order.CustomerId);

            var statusItems2 = new[]
            {
                new { Value = (int)OrderStatus.NogNietVerzonden, Text = "Nog niet verzonden" },
                new { Value = (int)OrderStatus.InDeBus, Text = "Onderweg" },
                new { Value = (int)OrderStatus.Onderweg, Text = "Bezorgd" }
            };

            ViewData["StatusList"] = new SelectList(statusItems2, "Value", "Text", (int)order.Status);

            var rackItems2 = new[]
            {
                new { Value = 'A', Text = "Rek A" },
                new { Value = 'B', Text = "Rek B" },
                new { Value = 'C', Text = "Rek C" },
                new { Value = 'D', Text = "Rek D" }
            };

            ViewData["RackList"] = new SelectList(rackItems2, "Value", "Text", order.Rack);

            return View(order);
        }

        // GET: Order/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Order/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order != null)
            {
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}