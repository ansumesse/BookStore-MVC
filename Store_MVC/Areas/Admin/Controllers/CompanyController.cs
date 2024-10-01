using Microsoft.AspNetCore.Mvc;
using Store.DataAccess.Repository.IRepository;
using Store.Models;

namespace Store_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View(unitOfWork.Company.GetAll());
        }

        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            if (id == 0 || id == null) 
            {
                return View(new Company());
            }
            return View(unitOfWork.Company.Get(c => c.Id == id));
        }

        [HttpPost]
        public IActionResult Upsert(Company company)
        {
            if (ModelState.IsValid)
            {
                if(company.Id == 0)
                {
                    // Add
                    unitOfWork.Company.Add(company);
                }
                else
                {
                    // Update
                    unitOfWork.Company.Update(company);
                }
                unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            return View(company);
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            Company company = unitOfWork.Company.Get(c => c.Id == id);
            if (company is null)
                return NotFound();
            return View(company);

        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCompany(int id)
        {
            unitOfWork.Company.Remove(unitOfWork.Company.Get(c => c.Id == id));
            unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        
    }
}
