using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.DataAccess.Repository.IRepository;
using Store.Models;
using Store.Utility;
using System.Diagnostics;
using System.Security.Claims;

namespace Store_MVC.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            this.unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = unitOfWork.Product.GetAll(includeProperties: "Category");
            return View(productList);
        }
        [HttpGet]
        
        public IActionResult Details(int productId)
        {
            ShoppingCart Cart = new()
            {
                Product = unitOfWork.Product.Get(p => p.Id == productId, includeProperties: "Category"),
                Count = 1,
                ProductId = productId
            };
            return View(Cart);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult Details(ShoppingCart cart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            cart.ApplicationUserId = userId;

            var cartFromDb = unitOfWork.ShoppingCart.Get(s => s.ProductId == cart.ProductId && s.ApplicationUserId == cart.ApplicationUserId);

            if (cartFromDb is not null)
            {
                // Update
                cartFromDb.Count += cart.Count;
                unitOfWork.ShoppingCart.Update(cartFromDb);
                unitOfWork.Save();
            }
            else
            {
                // Add
                unitOfWork.ShoppingCart.Add(cart);
                unitOfWork.Save();
                HttpContext.Session.SetInt32(SD.SessionCart,
                    unitOfWork.ShoppingCart.GetAll(s => s.ApplicationUserId == cart.ApplicationUserId).Count());
            }
            TempData["Success"] = "Cart updated successfully";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
