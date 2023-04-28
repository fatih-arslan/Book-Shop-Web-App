using BookShop.DataAccess.Data;
using BookShop.DataAccess.Repository.IRepository;
using BookShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShop.DataAccess.Repository
{
	public class ProductRepository : Repository<Product>, IProductRepository
	{
		private ApplicationDbContext _context;
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
			_context = context;
        }
        public void Update(Product product)
		{
			var productFromDb = _context.Products.FirstOrDefault(p => p.Id == product.Id);
			if(productFromDb != null)
			{
				productFromDb.Title = product.Title;
				productFromDb.ISBN = product.ISBN;
				productFromDb.Price = product.Price;
				productFromDb.Price50  = product.Price50;
				productFromDb.ListPrice = product.ListPrice;
				productFromDb.Price100 = product.Price100;
				productFromDb.Description = product.Description;
				productFromDb.CategoryId = product.CategoryId;
				productFromDb.Author = product.Author;
				if(product.ImageUrl != null)
				{
					productFromDb.ImageUrl = product.ImageUrl;
				}
			}
		}
	}
}
