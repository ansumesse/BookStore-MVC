using Microsoft.AspNetCore.Mvc;
using Store.DataAccess.Repository;
using Store.DataAccess.Repository.IRepository;
using Store.Models;

namespace Store_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public OrderController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<OrderHeader> orders = unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
            return View(orders);
        }
    }
}
