using Microsoft.AspNetCore.Mvc;
using Store.DataAccess.Data;
using Store.Models;

namespace Store_MVC.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext db;

        public CategoryController(ApplicationDbContext db)
        {
            this.db = db;
        }
        public IActionResult Index()
        {
            List<Category> categories = db.Categories.ToList();
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
            if(category.Name.ToLower() == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("", "Category Name Cann't be same as Display Order");
            }
            if (ModelState.IsValid)
            {
                TempData["Success"] = "Category created Successfully";
                db.Categories.Add(category);
                db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }
        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if(id == null || id == 0)
            {
                return NotFound();
            }
            Category? category = db.Categories.Find(id);
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
				db.Categories.Update(category);
                db.SaveChanges();
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
            Category? category = db.Categories.Find(id);
            if (category is null)
                return NotFound();

            return View(category);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
			TempData["Success"] = "Category deleted Successfully";
			Category category = db.Categories.Find(id)!;
            db.Categories.Remove(category);
            db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
