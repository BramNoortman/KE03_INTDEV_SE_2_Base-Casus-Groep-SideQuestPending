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
    public class DriversController : Controller
    {
        private readonly MatrixIncDbContext _context;

        public DriversController(MatrixIncDbContext context)
        {
            _context = context;
        }

        // GET: Drivers
        public async Task<IActionResult> Index(string search, string sortOrder, string sortField, string direction)
        {
            var query = _context.Drivers.AsQueryable();

            // dropdown
            if (!string.IsNullOrEmpty(sortField))
            {
                sortOrder = sortField switch
                {
                    "id" => direction == "desc" ? "id_desc" : "",
                    "name" => direction == "desc" ? "name_desc" : "name",
                    "busnumber" => direction == "desc" ? "busnumber_desc" : "busnumber",
                    "routenumber" => direction == "desc" ? "routenumber_desc" : "routenumber",
                    "active" => direction == "desc" ? "active_desc" : "active",
                    _ => sortOrder
                };
            }

            // search
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();

                query = query.Where(d =>
                    d.Id.ToString().Contains(search) ||
                    (d.Name ?? "").ToLower().Contains(search) ||
                    (d.BusNumber ?? "").ToLower().Contains(search) ||
                    (d.RouteNumber ?? "").ToLower().Contains(search) ||
                    d.Active.ToString().ToLower().Contains(search)
                );
            }

            ViewBag.Search = search;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.SortField = sortField;
            ViewBag.Direction = direction;

            // sorting
            query = sortOrder switch
            {
                "id_desc" => query.OrderByDescending(d => d.Id),
                "name" => query.OrderBy(d => d.Name),
                "name_desc" => query.OrderByDescending(d => d.Name),
                "busnumber" => query.OrderBy(d => d.BusNumber),
                "busnumber_desc" => query.OrderByDescending(d => d.BusNumber),
                "routenumber" => query.OrderBy(d => d.RouteNumber),
                "routenumber_desc" => query.OrderByDescending(d => d.RouteNumber),
                "active" => query.OrderBy(d => d.Active),
                "active_desc" => query.OrderByDescending(d => d.Active),
                _ => query.OrderBy(d => d.Id)
            };

            return View(await query.ToListAsync());
        }

        // GET: Drivers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Load driver with all assigned orders and customer details
            var driver = await _context.Drivers
                .Include(d => d.Orders)
                .ThenInclude(o => o.Customer)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (driver == null)
            {
                return NotFound();
            }

            return View(driver);
        }

        // GET: Drivers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Drivers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,BusNumber,RouteNumber,Active")] Driver driver)
        {
            if (ModelState.IsValid)
            {
                _context.Add(driver);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(driver);
        }

        // GET: Drivers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var driver = await _context.Drivers.FindAsync(id);
            if (driver == null)
            {
                return NotFound();
            }
            return View(driver);
        }

        // POST: Drivers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,BusNumber,RouteNumber,Active")] Driver driver)
        {
            if (id != driver.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(driver);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DriverExists(driver.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(driver);
        }

        // GET: Drivers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var driver = await _context.Drivers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (driver == null)
            {
                return NotFound();
            }

            return View(driver);
        }

        // POST: Drivers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var driver = await _context.Drivers.FindAsync(id);
            if (driver != null)
            {
                _context.Drivers.Remove(driver);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DriverExists(int id)
        {
            return _context.Drivers.Any(e => e.Id == id);
        }
    }
}
