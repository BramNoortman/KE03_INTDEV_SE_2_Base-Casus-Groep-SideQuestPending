using DataAccessLayer.Interfaces;
using KE03_INTDEV_SE_2_Base.Models;
using Microsoft.AspNetCore.Mvc;

namespace KE03_INTDEV_SE_2_Base.Controllers
{
    public class FinanceController : Controller
    {
        private readonly IOrderRepository _orderRepository;

        public FinanceController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public IActionResult Index()
        {
            var orders = _orderRepository.GetAllOrders().ToList();

            var viewModel = new FinanceViewModel
            {
                RecentOrders = orders.OrderByDescending(o => o.OrderDate).Take(10).ToList(),
                TotalRevenue = orders.Sum(o => o.Items.Sum(p => p.Product.Price)),
                AverageOrderValue = orders.Any() ? orders.Average(o => o.Items.Sum(p => p.Product.Price)) : 0,
                ProfitMargin = 32,
                OutstandingPayments = 3450
            };

            return View(viewModel);
        }
    }
}