using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer;
using DataAccessLayer.Models;

namespace KE03_INTDEV_SE_2_Base.Controllers
{
    public class KlachtenController : Controller
    {
        private readonly MatrixIncDbContext _context;

        public KlachtenController(MatrixIncDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string search, string sortOrder, string sortField, string direction)
        {
            var query = _context.Klachten.AsQueryable();

            // If dropdown is used, convert it to sortOrder (same pattern as Products)
            if (!string.IsNullOrEmpty(sortField))
            {
                sortOrder = sortField switch
                {
                    "id" => direction == "desc" ? "id_desc" : "id",
                    "onderwerp" => direction == "desc" ? "onderwerp_desc" : "onderwerp",
                    "status" => direction == "desc" ? "status_desc" : "status",
                    "date" => direction == "desc" ? "date_desc" : "date",
                    _ => sortOrder
                };
            }

            // SEARCH
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();

                query = query.Where(k =>
                    k.Id.ToString().Contains(search) ||
                    (k.Onderwerp ?? "").ToLower().Contains(search) ||
                    (k.Beschrijving ?? "").ToLower().Contains(search) ||
                    (k.Status ?? "").ToLower().Contains(search));
            }

            // VIEWBAGS (same pattern as Products)
            ViewBag.Search = search;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.SortField = sortField;
            ViewBag.Direction = direction;

            ViewBag.IdSort = sortOrder == "id_desc" ? "id" : "id_desc";
            ViewBag.OnderwerpSort = sortOrder == "onderwerp" ? "onderwerp_desc" : "onderwerp";
            ViewBag.StatusSort = sortOrder == "status" ? "status_desc" : "status";
            ViewBag.DateSort = sortOrder == "date" ? "date_desc" : "date";

            // SORTING
            query = sortOrder switch
            {
                "id" => query.OrderBy(k => k.Id),
                "id_desc" => query.OrderByDescending(k => k.Id),

                "onderwerp" => query.OrderBy(k => k.Onderwerp),
                "onderwerp_desc" => query.OrderByDescending(k => k.Onderwerp),

                "status" => query.OrderBy(k => k.Status),
                "status_desc" => query.OrderByDescending(k => k.Status),

                "date" => query.OrderBy(k => k.AangemaaktOp),
                "date_desc" => query.OrderByDescending(k => k.AangemaaktOp),

                _ => query.OrderByDescending(k => k.Id)
            };

            return View(await query.ToListAsync());
        }

        // GET: Klachten/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var klacht = await _context.Klachten
                .FirstOrDefaultAsync(k => k.Id == id);

            if (klacht == null)
                return NotFound();

            return View(klacht);
        }

        // GET: Klachten/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Klachten/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Onderwerp,Beschrijving,Status")] Klacht klacht)
        {
            if (ModelState.IsValid)
            {
                klacht.AangemaaktOp = DateTime.Now;

                _context.Add(klacht);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(klacht);
        }

        // GET: Klachten/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var klacht = await _context.Klachten.FindAsync(id);

            if (klacht == null)
                return NotFound();

            return View(klacht);
        }

        // POST: Klachten/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Onderwerp,Beschrijving,Status,AangemaaktOp")] Klacht klacht)
        {
            if (id != klacht.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(klacht);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KlachtExists(klacht.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(klacht);
        }

        // GET: Klachten/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var klacht = await _context.Klachten
                .FirstOrDefaultAsync(k => k.Id == id);

            if (klacht == null)
                return NotFound();

            return View(klacht);
        }

        // POST: Klachten/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var klacht = await _context.Klachten.FindAsync(id);

            if (klacht != null)
            {
                _context.Klachten.Remove(klacht);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool KlachtExists(int id)
        {
            return _context.Klachten.Any(e => e.Id == id);
        }
    }
}