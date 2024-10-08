using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using Store.DataAccess.Repository;
using Store.DataAccess.Repository.IRepository;
using Store.Models;
using Store.Models.ViewModels;
using Store.Utility;
using Stripe;
using System.Diagnostics;
using System.Security.Claims;

namespace Store_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        [BindProperty]
        public OrderVM OrderVM { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public IActionResult Index(string status)
        {
            IEnumerable<OrderHeader> orders;

			if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
				orders = unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
				orders = unitOfWork.OrderHeader.GetAll(o => o.ApplicationUserId == userId, includeProperties: "ApplicationUser");
			}

			switch (status)
            {
                case "pending":
                    orders = orders.Where(o => o.PaymentStatus == SD.PaymentStatusPending || o.PaymentStatus == SD.PaymentStatusDelayedPayment);
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
            OrderVM = new()
            {
                OrderHeader = unitOfWork.OrderHeader.Get(o => o.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetails = unitOfWork.OrderDetail.GetAll(o => o.OrderHeaderId == orderId, includeProperties: "Product")
            };
            return View(OrderVM);
        }
        [HttpPost, Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult UpdateOrderDetails()
        {
            var orderHeaderFromDb = unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeaderFromDb.Name = OrderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = OrderVM.OrderHeader.City;
            orderHeaderFromDb.State = OrderVM.OrderHeader.State;
            orderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;
            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.Carrier))
            {
                orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingNumber))
            {
				orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
			}
		
		    unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            unitOfWork.Save();

            TempData["Success"] = "Order Details Updated Successfully.";


            return RedirectToAction(nameof(Details), new
            {
                orderId = orderHeaderFromDb.Id
            });
        
        }

		[HttpPost, Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		public IActionResult StartProcessing()
        {
            unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProcess);
            unitOfWork.Save();
			TempData["Success"] = "Order Details Updated Successfully.";


			return RedirectToAction(nameof(Details), new
			{
				orderId = OrderVM.OrderHeader.Id
			});
		}

		[HttpPost, Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		public IActionResult ShipOrder()
		{
            OrderHeader orderHeader = unitOfWork.OrderHeader.Get(o => o.Id == OrderVM.OrderHeader.Id, tracked:true);
            orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeader.ShippingDate = DateTime.Now;
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
                orderHeader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
			unitOfWork.Save();
			unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusShipped);
			unitOfWork.Save();
			TempData["Success"] = "Order Shipped Successfully.";


			return RedirectToAction(nameof(Details), new
			{
				orderId = OrderVM.OrderHeader.Id
			});
		}

		[HttpPost, Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		public IActionResult CancelOrder()
        {
            OrderHeader orderHeader = unitOfWork.OrderHeader.Get(o => o.Id == OrderVM.OrderHeader.Id);
            if(orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions()
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };

                RefundService service = new RefundService();
                Refund refund = service.Create(options);
                unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
				unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
			}
			unitOfWork.Save();
			TempData["Success"] = "Order Canceled Successfully.";


			return RedirectToAction(nameof(Details), new
			{
				orderId = OrderVM.OrderHeader.Id
			});
		}
	}
}
