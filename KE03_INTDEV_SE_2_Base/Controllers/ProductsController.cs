using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer;
using DataAccessLayer.Models;

namespace KE03_INTDEV_SE_2_Base.Controllers
{
    public class ProductsController : Controller
    {
        private readonly MatrixIncDbContext _context;

        public ProductsController(MatrixIncDbContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index(string search, string sortOrder)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                decimal priceSearch;
                bool isPrice = decimal.TryParse(search.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out priceSearch);
                query = query.Where(p => (p.Id.ToString() ?? "").Contains(search) || (p.Name ?? "").ToLower().Contains(search) || (isPrice && p.Price >= priceSearch && p.Price < priceSearch + 1) || (p.Description ?? "").ToLower().Contains(search));
            }

            ViewBag.Search = search;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.IdSort = sortOrder == "id_desc" ? "" : "id_desc";
            ViewBag.NameSort = sortOrder == "name" ? "name_desc" : "name";
            ViewBag.PriceSort = sortOrder == "price" ? "price_desc" : "price";
            ViewBag.DescriptionSort = sortOrder == "description" ? "description_desc" : "description";

            query = sortOrder switch
            {
                "id_desc" => query.OrderByDescending(p => p.Id),
                "name" => query.OrderBy(p => p.Name),
                "name_desc" => query.OrderByDescending(p => p.Name),
                "price" => query.OrderBy(p => (double)p.Price), //double fixes sorting because price is deicimal
                "price_desc" => query.OrderByDescending(p => (double)p.Price),
                "description" => query.OrderBy(p => p.Description),
                "description_desc" => query.OrderByDescending(p => p.Description),
                _ => query.OrderBy(p => p.Id)
            };

            return View(await query.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Price")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
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

            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}