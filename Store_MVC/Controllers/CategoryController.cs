using Microsoft.AspNetCore.Mvc;
using Store_MVC.Data;
using Store_MVC.Models;

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
    }
}
