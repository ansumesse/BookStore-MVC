using Microsoft.AspNetCore.Mvc;
using Store.DataAccess.Repository;
using Store.DataAccess.Repository.IRepository;
using Store.Models;
using Store.Models.ViewModels;
using Store.Utility;
using System.Diagnostics;

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
        public IActionResult Index(string status)
        {
            IEnumerable<OrderHeader> orders = unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");

            switch (status)
            {
                case "pending":
                    orders = orders.Where(o => o.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    orders = orders.Where(o => o.OrderStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    orders = orders.Where(o => o.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    orders = orders.Where(o => o.OrderStatus == SD.StatusApproved);
                    break;
                default:
                    break;
            }

            return View(orders);
        }
        public IActionResult Details(int orderId)
        {
            OrderVM orderVM = new()
            {
                OrderHeader = unitOfWork.OrderHeader.Get(o => o.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetails = unitOfWork.OrderDetail.GetAll(o => o.OrderHeaderId == orderId, includeProperties: "Product")
            };
            return View(orderVM);
        }
    }
}
