using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Store.DataAccess.Data;
using Store.Models;
using Store.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Store.DataAccess.DBInitializer
{
	public class DbInitializer : IDbInitalizer
	{
		private readonly ApplicationDbContext db;
		private readonly RoleManager<IdentityRole> roleManager;
		private readonly UserManager<IdentityUser> userManager;

		public DbInitializer(ApplicationDbContext db,
			RoleManager<IdentityRole> roleManager,
			UserManager<IdentityUser> userManager)
        {
			this.db = db;
			this.roleManager = roleManager;
			this.userManager = userManager;
		}
        public void Initialize()
		{
			// 
			try
			{
				if(db.Database.GetPendingMigrations().Count() > 0)
				{
					db.Database.Migrate();
				}
			}
			catch (Exception ex)
			{
			}

			//
			if (!roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
			{
				roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
				roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
				roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
				roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();

				userManager.CreateAsync(new ApplicationUser
				{
					UserName = "admin@gmail.com",
					Email = "admin@gmail.com",
					Name = "Mohamed Anas",
					PhoneNumber = "1112223333",
					StreetAddress = "test 123 Ave",
					State = "IL",
					PostalCode = "23422",
					City = "Chicago"
				}, "Admin123*").GetAwaiter().GetResult();


				ApplicationUser user = db.ApplicationUsers.FirstOrDefault(u => u.Email == "admin@gmail.com");
				userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
			}
		}
	}
}
