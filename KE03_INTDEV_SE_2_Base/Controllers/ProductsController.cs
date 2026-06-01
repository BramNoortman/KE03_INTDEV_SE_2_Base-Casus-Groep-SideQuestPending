using DataAccessLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;


namespace KE03_INTDEV_SE_2_Base.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductRepository _productRepository;

        public ProductsController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

    public IActionResult Index(string search)
        {
            var products = _productRepository.GetAllProducts();
            if (!string.IsNullOrWhiteSpace(search))
            {
                products = products.Where(p => p.Name.ToLower().Contains(search.ToLower()));
            }

            var result = products.OrderBy(p => p.Id).ToList();
            ViewBag.Search = search;
            return View(result);
        }
    }
}
