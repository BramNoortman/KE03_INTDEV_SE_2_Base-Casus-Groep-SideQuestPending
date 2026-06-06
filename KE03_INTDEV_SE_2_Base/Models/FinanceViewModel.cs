using DataAccessLayer.Models;

namespace KE03_INTDEV_SE_2_Base.Models
{
    public class FinanceViewModel
    {
        public List<Order> RecentOrders { get; set; } = new List<Order>();
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal ProfitMargin { get; set; }
        public decimal OutstandingPayments { get; set; }
    }
}