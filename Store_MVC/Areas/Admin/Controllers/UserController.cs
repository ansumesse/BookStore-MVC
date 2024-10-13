using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Store.DataAccess.Data;
using Store.Models;
using Store.Utility;

namespace Store_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext db;

        public UserController(ApplicationDbContext db)
        {
            this.db = db;
        }
        public IActionResult Index()
        {
            var users = db.ApplicationUsers.Include(u => u.Company).ToList();

            var roles = db.Roles.ToList();
            var userRoles = db.UserRoles.ToList();
            foreach (var user in users)
            {
                user.Company ??= new Company { Name = "N / A" };

                var userRoleId = userRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                user.Role = roles.FirstOrDefault(r => r.Id == userRoleId).Name;
            }
            return View(users);
        }
        [HttpPost]
        public IActionResult LockUnlock([FromBody]string id)
        {
            var user = db.ApplicationUsers.FirstOrDefault(u => u.Id == id);
            if(user.LockoutEnd is not null && user.LockoutEnd > DateTime.Now)
            {
                // user is locked we need to unlock him
                user.LockoutEnd = DateTime.Now;
            }
            else
            {
                user.LockoutEnd = DateTime.Now.AddYears(100);
            }
            db.SaveChanges();
            return Json(new { success = true, message = "Operation Successful" });
        }
    }
}
