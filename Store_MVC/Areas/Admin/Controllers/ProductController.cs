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
        private readonly IWebHostEnvironment hostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            this.unitOfWork = unitOfWork;
            this.hostEnvironment = hostEnvironment;
        }
        public IActionResult Index()
        {
            List<Product> products = unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
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
        public IActionResult Upsert(ProductVM productVM)
        {
            if (ModelState.IsValid)
            {
                if(productVM.Image is not null)
                {
                    string wwwrootPath = hostEnvironment.WebRootPath;
                    string imageName = Guid.NewGuid() + Path.GetExtension(productVM.Image.FileName);
                    string imagesPath = Path.Combine(wwwrootPath, @"images\product");
                    string imagePath = Path.Combine(imagesPath, imageName);

                    if(!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        string oldImage = productVM.Product.ImageUrl;
                        if (System.IO.File.Exists(oldImage))
                        {
                            System.IO.File.Delete(oldImage);
                        }
                    }
                    using var Stream = System.IO.File.Create(imagePath);
                    productVM.Image.CopyTo(Stream);
                    productVM.Product.ImageUrl = Path.Combine(@"\images\product", imageName);
                }
                if(productVM.Product.Id == 0)
                {
                    TempData["Success"] = "Product created Successfully";
                    unitOfWork.Product.Add(productVM.Product);
                }
                else
                {
                    TempData["Update"] = "Product Updated Successfully";
                    unitOfWork.Product.Update(productVM.Product);
                }
                unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            productVM.CategoryList = unitOfWork.Category.GetAll().Select(c => new SelectListItem { Text = c.Name, Value = c.Id.ToString() });
            return View(productVM);
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
