using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Store.DataAccess.Repository.IRepository;
using Store.Models;
using Store.Models.ViewModels;

namespace Store_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public ProductController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<Product> products = unitOfWork.Product.GetAll().ToList();
            return View(products);
        }
        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new()
            {
                Product = (id == 0 || id == null) ? new Product() : unitOfWork.Product.Get(p => p.Id == id),
                CategoryList = unitOfWork.Category.GetAll().Select(c => new SelectListItem { Text = c.Name, Value = c.Id.ToString() })
            };
            return View(productVM);
        }
        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? image)
        {
            if (ModelState.IsValid)
            {
                TempData["Success"] = "Product created Successfully";
                unitOfWork.Product.Add(productVM.Product);
                unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            productVM.CategoryList = unitOfWork.Category.GetAll().Select(c => new SelectListItem { Text = c.Name, Value = c.Id.ToString() });
           
            return View(productVM);
        }
        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            ProductVM productVM = new()
            {
                Product = unitOfWork.Product.Get(c => c.Id == id),
                CategoryList = unitOfWork.Category.GetAll().Select(c => new SelectListItem { Text = c.Name, Value = c.Id.ToString() })
            };
            if (productVM.Product is null)
                return NotFound();

            return View(productVM);
        }
        [HttpPost]
        public IActionResult Edit(Product product)
        {

            if (ModelState.IsValid)
            {
                TempData["Success"] = "Product Updated Successfully";
                unitOfWork.Product.Update(product);
                unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }
        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            ProductVM productVM = new()
            {
                Product = unitOfWork.Product.Get(c => c.Id == id),
                CategoryList = unitOfWork.Category.GetAll().Select(c => new SelectListItem { Text = c.Name, Value = c.Id.ToString() })
            };
            if (productVM.Product is null)
                return NotFound();

            return View(productVM);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            TempData["Success"] = "Product deleted Successfully";
            Product product = unitOfWork.Product.Get(c => c.Id == id);
            unitOfWork.Product.Remove(product);
            unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
    }
}
