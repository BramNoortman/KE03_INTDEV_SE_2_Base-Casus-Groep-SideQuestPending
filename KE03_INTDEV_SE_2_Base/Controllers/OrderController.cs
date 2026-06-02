using DataAccessLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace KE03_INTDEV_SE_2_Base.Controllers
{
    public class OrderController: Controller
    {
        private readonly IOrderRepository _orderRepository;

        public OrderController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

     public IActionResult Index(string search)
        {
            var orders = _orderRepository.GetAllOrders();
            if (!string.IsNullOrWhiteSpace(search))
            {
                orders = orders.Where(o => o.Customer.Name.ToLower().Contains(search.ToLower()));
            }

            var result = orders.OrderBy(p => p.Id).ToList();
            ViewBag.Search = search;
            return View(result);
        }
    }
}