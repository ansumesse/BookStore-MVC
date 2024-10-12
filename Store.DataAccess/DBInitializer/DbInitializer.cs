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
		private readonly UserManager<ApplicationUser> userManager;

		public DbInitializer(ApplicationDbContext db,
			RoleManager<IdentityRole> roleManager,
			UserManager<ApplicationUser> userManager)
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

				ApplicationUser userAdmin = new ApplicationUser()
				{
					UserName = "admin@dotnetmastery.com",
					Email = "admin@dotnetmastery.com",
					Name = "Bhrugen Patel",
					PhoneNumber = "1112223333",
					StreetAddress = "test 123 Ave",
					State = "IL",
					PostalCode = "23422",
					City = "Chicago"
				};

				userManager.CreateAsync(userAdmin, "Admin123*").GetAwaiter().GetResult();
				userManager.AddToRoleAsync(userAdmin, SD.Role_Admin).GetAwaiter().GetResult();
			}
		}
	}
}
