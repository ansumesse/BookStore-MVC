using Store.DataAccess.Data;
using Store.DataAccess.Repository.IRepository;
using Store.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Store.DataAccess.Repository
{
    internal class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext db;

        public CategoryRepository(ApplicationDbContext db): base(db)
        {
            this.db = db;
        }
        public void Save()
        {
            db.SaveChanges();
        }

        public void Update(Category category)
        {
            db.Categories.Update(category);
        }
    }
}
