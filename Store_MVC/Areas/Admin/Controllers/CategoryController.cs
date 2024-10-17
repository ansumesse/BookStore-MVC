using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.DataAccess.Repository.IRepository;
using Store.Models;
using Store.Utility;

namespace BookStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<Category> categories = unitOfWork.Category.GetAll().ToList();
            return View(categories);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (category.Name.ToLower() == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("", "Category Name Cann't be same as Display Order");
            }
            if (ModelState.IsValid)
            {
                TempData["Success"] = "Category created Successfully";
                unitOfWork.Category.Add(category);
                unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }
        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category? category = unitOfWork.Category.Get(c => c.Id == id);
            if (category is null)
                return NotFound();

            return View(category);
        }
        [HttpPost]
        public IActionResult Edit(Category category)
        {

            if (ModelState.IsValid)
            {
                TempData["Success"] = "Category Updated Successfully";
                unitOfWork.Category.Update(category);
                unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }
        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category? category = unitOfWork.Category.Get(c => c.Id == id);
            if (category is null)
                return NotFound();

            return View(category);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            TempData["Success"] = "Category deleted Successfully";
            Category category = unitOfWork.Category.Get(c => c.Id == id);
            unitOfWork.Category.Remove(category);
            unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
    }
}
