using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer;

namespace KE03_INTDEV_SE_2_Base.Controllers
{
    public class WarehouseController : Controller
    {
        private readonly MatrixIncDbContext _context;

        public WarehouseController(MatrixIncDbContext context)
        {
            _context = context;
        }

        // GET: Warehouse
        public async Task<IActionResult> Index()
        {
            // Fetch all orders sorted by rack (A-D) then by ID for consistent display
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Driver)
                .OrderBy(o => o.Rack)
                .ThenBy(o => o.Id)
                .ToListAsync();

            return View(orders);
        }
    }
}
