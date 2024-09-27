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
     class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext db;

        public ProductRepository(ApplicationDbContext db): base(db)
        {
            this.db = db;
        }
        public void Update(Product product)
        {
            Product? oldProduct = db.Products.FirstOrDefault(p => p.Id == product.Id);
            if(oldProduct is not null)
            {
                oldProduct.ISBN = product.ISBN;
                oldProduct.ListPrice = product.ListPrice;
                oldProduct.Price = product.Price;
                oldProduct.Price100 = product.Price100;
                oldProduct.Price50 = product.Price50;
                oldProduct.Author = product.Author;
                oldProduct.CategoryId = product.CategoryId;
                oldProduct.Description = product.Description;
                oldProduct.Title = product.Title;
                if (product.ImageUrl is not null)
                    oldProduct.ImageUrl = product.ImageUrl;
            }
        }
    }
}
