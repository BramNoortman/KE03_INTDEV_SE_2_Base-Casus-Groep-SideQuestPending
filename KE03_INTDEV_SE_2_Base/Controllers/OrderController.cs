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
        public async Task<IActionResult> Index(string search)
        {
            var query = _context.Orders.Include(o => o.Customer).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(o => o.Id.ToString().Contains(search) || o.OrderDate.ToString().Contains(search) || o.CustomerId.ToString().Contains(search) || (o.Customer.Name ?? "").ToLower().Contains(search));
            }

            ViewBag.Search = search;
            var orders = await query.OrderBy(o => o.Id).ToListAsync();

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
            _logger.LogInformation("Create GET - products count={Count}", products?.Count ?? 0);
            ViewData["ProductListItems"] = products;
            return View();
        }

        // POST: Order/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderDate,CustomerId")] Order order)
        {
            _logger.LogInformation("Create POST received. Form values: {Form}", Request.Form.ToDictionary(k => k.Key, v => v.Value.ToString()));
            _logger.LogInformation("Bound Order: {@Order}", order);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState invalid on Create: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                // Reload customers and products for display
                var customers = await _context.Customers.ToListAsync();
                ViewData["CustomerList"] = new SelectList(customers, "Id", "Name", order.CustomerId);
                var products = await _context.Products.ToListAsync();
                ViewData["ProductListItems"] = products;
                return View(order);
            }

            // read quantities for all products and add items with qty>0
            var allProducts = await _context.Products.Select(p => p.Id).ToListAsync();
            foreach (var pid in allProducts)
            {
                var qtyStr = Request.Form[$"quantity_{pid}"] .FirstOrDefault();
                if (int.TryParse(qtyStr, out var qty) && qty > 0)
                {
                    order.Items.Add(new OrderItem { ProductId = pid, Quantity = qty });
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
            return View(order);
        }

        // POST: Order/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,OrderDate,CustomerId")] Order order)
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

                    // read quantities for all products and replace items accordingly
                    var allProducts = await _context.Products.Select(p => p.Id).ToListAsync();
                    var newItems = new List<OrderItem>();
                    foreach (var pid in allProducts)
                    {
                        var qtyStr = Request.Form[$"quantity_{pid}"] .FirstOrDefault();
                        if (int.TryParse(qtyStr, out var qty) && qty > 0)
                        {
                            newItems.Add(new OrderItem { ProductId = pid, Quantity = qty });
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
