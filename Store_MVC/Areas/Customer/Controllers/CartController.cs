using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.DataAccess.Repository.IRepository;
using Store.Models;
using Store.Models.ViewModels;
using Store.Utility;
using Stripe.Checkout;
using System.Security.Claims;

namespace Store_MVC.Areas.Customer.Controllers
{
	[Area("Customer")]
	[Authorize]
	public class CartController : Controller
	{
		private readonly IUnitOfWork unitOfWork;
		[BindProperty]
		public ShoppingCartVM ShoppingCartVM { get; set; }
		public CartController(IUnitOfWork unitOfWork)
		{
			this.unitOfWork = unitOfWork;
		}
		public IActionResult Index()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

			ShoppingCartVM = new()
			{
				ShoppingCartList = unitOfWork.ShoppingCart.GetAll(s => s.ApplicationUserId == userId, "Product")
			};
			foreach (var cart in ShoppingCartVM.ShoppingCartList)
			{
				cart.Price = GetPriceBasedOnQuantity(cart);
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}

			return View(ShoppingCartVM);
		}
		public IActionResult Plus(int cartId)
		{
			var cartFromDb = unitOfWork.ShoppingCart.Get(s => s.Id == cartId);
			cartFromDb.Count++;
			unitOfWork.ShoppingCart.Update(cartFromDb);
			unitOfWork.Save();
			return RedirectToAction(nameof(Index));
		}
		public IActionResult Minus(int cartId)
		{
			var cartFromDb = unitOfWork.ShoppingCart.Get(s => s.Id == cartId);
			if (cartFromDb.Count == 1)
			{
                unitOfWork.ShoppingCart.Remove(cartFromDb);
                HttpContext.Session.SetInt32(SD.SessionCart,
                  unitOfWork.ShoppingCart.GetAll(s => s.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);
            }
            else
			{
				cartFromDb.Count--;
				unitOfWork.ShoppingCart.Update(cartFromDb);
			}
			unitOfWork.Save();
			return RedirectToAction(nameof(Index));
		}
		public IActionResult Delete(int cartId)
		{
			var cartFromDb = unitOfWork.ShoppingCart.Get(s => s.Id == cartId);
			unitOfWork.ShoppingCart.Remove(cartFromDb);
            HttpContext.Session.SetInt32(SD.SessionCart,
                   unitOfWork.ShoppingCart.GetAll(s => s.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);
            unitOfWork.Save();
			return RedirectToAction(nameof(Index));
		}
		public IActionResult Summary()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

			ShoppingCartVM = new()
			{
				ShoppingCartList = unitOfWork.ShoppingCart.GetAll(s => s.ApplicationUserId == userId, "Product")
			};
			foreach (var cart in ShoppingCartVM.ShoppingCartList)
			{
				cart.Price = GetPriceBasedOnQuantity(cart);
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}
			ShoppingCartVM.OrderHeader.ApplicationUser = unitOfWork.ApplicationUser.Get(u => u.Id == userId);

			ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
			ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
			ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
			ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
			ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
			ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

			return View(ShoppingCartVM);
		}
		[HttpPost, ActionName("Summary")]
		public IActionResult SummaryPost()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

			ShoppingCartVM.ShoppingCartList = unitOfWork.ShoppingCart.GetAll(s => s.ApplicationUserId == userId, "Product");
			ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
			ShoppingCartVM.OrderHeader.ApplicationUserId = userId;


			// in EF When Adding Class to db and use navigation prop before it it also add the navigation prop
			// so make sure to intialize new instance for the navigation prop that i want
			ApplicationUser applicationUser = unitOfWork.ApplicationUser.Get(u => u.Id == userId);
			//ShoppingCartVM.OrderHeader.ApplicationUser = unitOfWork.ApplicationUser.Get(u => u.Id == userId); 

			foreach (var cart in ShoppingCartVM.ShoppingCartList)
			{
				cart.Price = GetPriceBasedOnQuantity(cart);
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}

			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
			{
				// The User is a regular Customer
				ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
				ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
			}
			else
			{
				// The user is Company Customer
				ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
				ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;

			}

			unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
			unitOfWork.Save();

			foreach (var cart in ShoppingCartVM.ShoppingCartList)
			{
				OrderDetail orderDetail = new()
				{
					OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
					Count = cart.Count,
					Price = cart.Price,
					ProductId = cart.ProductId
				};
				unitOfWork.OrderDetail.Add(orderDetail);
				unitOfWork.Save();
			}

			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
			{
				// Regular User Should Pay
				var domain = Request.Scheme + "://" + Request.Host.Value + "/"; 
				var options = new Stripe.Checkout.SessionCreateOptions
				{
					SuccessUrl = domain +$"Customer/Cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
					CancelUrl = domain + "Customer/Cart/Index",
					LineItems = new List<SessionLineItemOptions>(),
					Mode = "payment",
				};
				foreach (var item in ShoppingCartVM.ShoppingCartList)
				{
					var sessionLineItem = new SessionLineItemOptions()
					{
						PriceData = new SessionLineItemPriceDataOptions()
						{
							Currency = "usd",
							UnitAmount = (long)item.Price * 100, // to be in cents
							ProductData = new SessionLineItemPriceDataProductDataOptions()
							{
								Name = item.Product.Title
							}
						},
						Quantity = item.Count
					};
					options.LineItems.Add(sessionLineItem);
				}

				var service = new SessionService();
				Session session = service.Create(options);
				unitOfWork.OrderHeader.UpdateStripePaymentID(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
				unitOfWork.Save();
				Response.Headers.Add("Location", session.Url);
				return new StatusCodeResult(303);
			}

			return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id });
		}
		public IActionResult OrderConfirmation(int id)
		{
			OrderHeader orderHeader = unitOfWork.OrderHeader.Get(o => o.Id == id);
			if(orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
			{
				// order by regular customer
				SessionService service = new SessionService();
				Session session = service.Get(orderHeader.SessionId);
				if (session.PaymentStatus == "paid")
				{
					unitOfWork.OrderHeader.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
					unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
					unitOfWork.Save();
				}
				var shoppingcart = unitOfWork.ShoppingCart.GetAll(s => s.ApplicationUserId == orderHeader.ApplicationUserId);
				unitOfWork.ShoppingCart.RemoveRange(shoppingcart);
				unitOfWork.Save();
			}
			HttpContext.Session.Clear();
			return View(id);
		}
		private double GetPriceBasedOnQuantity(ShoppingCart cart)
		{
			if (cart.Count <= 50)
				return cart.Product.Price;
			else
			{
				if (cart.Count <= 100)
					return cart.Product.Price50;
				else
					return cart.Product.Price100;
			}
		}
	}
}
