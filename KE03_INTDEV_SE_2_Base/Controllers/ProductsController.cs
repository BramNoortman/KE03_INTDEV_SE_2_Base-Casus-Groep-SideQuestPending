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

    public IActionResult Index()
        {
            var products = _productRepository.GetAllProducts().OrderBy(p => p.Id).ToList();
            return View(products);
        }
    }
}
