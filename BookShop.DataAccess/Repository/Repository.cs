using BookShop.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using BookShop.DataAccess.Repository.IRepository;

namespace BookShop.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
	{
		private readonly ApplicationDbContext _context;
		internal DbSet<T> dbSet; 
        public Repository(ApplicationDbContext context)
        {
            _context = context;
			this.dbSet = _context.Set<T>();
        }
        public void Add(T entity)
		{
			dbSet.Add(entity);
		}

		public T? Get(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false)
		{
			IQueryable<T> query;
            if (tracked)
			{
				query = dbSet;
                
            }
			else
			{
                query = dbSet.AsNoTracking();                
            }
            query = query.Where(filter);
            if (!String.IsNullOrEmpty(includeProperties))
            {
                foreach (var property in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(property);
                }
            }
            return query.FirstOrDefault();
        }

		
		public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = null)
		{
			// When getting items we are including the properties that are from other tables with the foreign keys 
			// Like getting the category name for a product with the CategoryId key
			// The includeProperties argument must be given as comma seperated values like "Category,CoverType"
			IQueryable<T> query = dbSet;
			if(filter != null)
			{
                query = query.Where(filter);
            }
            if (!String.IsNullOrEmpty(includeProperties))
			{
				foreach(var property in includeProperties.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries)) 
				{
					query = query.Include(property);
				}
			}
			return query.ToList();
		}

		public void Remove(T entity)
		{
			dbSet.Remove(entity);
		}

		public void RemoveRange(IEnumerable<T> entities)
		{
			dbSet.RemoveRange(entities);
		}
	}
}
