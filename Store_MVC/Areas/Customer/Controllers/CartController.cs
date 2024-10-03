using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.DataAccess.Repository.IRepository;
using Store.Models;
using Store.Models.ViewModels;
using System.Security.Claims;

namespace Store_MVC.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

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
                ShoppingCartVM.OrderTotal += (cart.Price * cart.Count);
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
                unitOfWork.ShoppingCart.Remove(cartFromDb);
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
            unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Summary()
        {
            return View();
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
