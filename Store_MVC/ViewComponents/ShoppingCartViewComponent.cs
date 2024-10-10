using Microsoft.AspNetCore.Mvc;
using Store.DataAccess.Repository.IRepository;
using Store.Utility;
using Stripe;
using System.Security.Claims;

namespace Store_MVC.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork unitOfWork;

        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim is not null)
            {
                if(HttpContext.Session.GetInt32(SD.SessionCart) is not null)
                {
                    HttpContext.Session.SetInt32(SD.SessionCart,
                    unitOfWork.ShoppingCart.GetAll(s => s.ApplicationUserId == claim.Value).Count());
                }
                return View(HttpContext.Session.GetInt32(SD.SessionCart));
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }
        }
    }
}
