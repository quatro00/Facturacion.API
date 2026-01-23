using Facturacion.API.Models.Dto;
using Facturacion.API.Models.Interfaces;
using Facturacion.API.Repositories.Interface;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Facturacion.API.Data;

namespace Facturacion.API.Repositories.Implementation
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly FacturacionContext _context;
        private readonly DbSet<T> _dbSet;
        public GenericRepository(FacturacionContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }
        public async Task<T?> GetByIdAsyncString(string id)
        {
            return await _dbSet.FindAsync(id);
        }
        public async Task<T?> GetByIdAsync(Guid id, string idFieldName, params string[] includePaths)
        {
            IQueryable<T> query = _context.Set<T>();

            foreach (var include in includePaths)
            {
                query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync(e => EF.Property<Guid>(e, idFieldName) == id);
        }

        public async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(T entity)
        {
            _dbSet.Attach(entity);

            // Marcar toda la entidad como modificada
            _context.Entry(entity).State = EntityState.Modified;

            // Evitar modificar propiedades identity (por ejemplo: No)
            var identityProps = _context.Entry(entity).Properties
                .Where(p => p.Metadata.IsPrimaryKey() || p.Metadata.ValueGenerated == ValueGenerated.OnAdd)
                .ToList();

            foreach (var prop in identityProps)
            {
                prop.IsModified = false;
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<T>> ListAsync(ISpecification<T> spec)
        {
            IQueryable<T> query = _dbSet;

            if (spec.Criteria != null)
                query = query.Where(spec.Criteria);

            if (spec.IncludeStrings != null)
            {
                foreach (var includeStr in spec.IncludeStrings)
                {
                    query = query.Include(includeStr);
                }
            }

            return await query.ToListAsync();
        }

        public async Task<List<T>> ListAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<PaginatedResult<T>> ListAsync(ISpecification<T> spec, PaginationFilter pagination)
        {
            var query = _dbSet.Where(spec.Criteria);

            query = ApplyOrdering(query, pagination.OrderBy, pagination.Descending);

            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();

            return new PaginatedResult<T>
            {
                Items = items,
                TotalItems = totalItems,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize
            };
        }

        public async Task<PaginatedResult<TResult>> ListAsync<TResult>(
            ISpecification<T> spec,
            PaginationFilter pagination,
            Expression<Func<T, TResult>> selector)
        {
            var query = _dbSet.Where(spec.Criteria);

            query = ApplyOrdering(query, pagination.OrderBy, pagination.Descending);

            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(selector)
                .ToListAsync();

            return new PaginatedResult<TResult>
            {
                Items = items,
                TotalItems = totalItems,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize
            };
        }

        private IQueryable<T> ApplyOrdering(IQueryable<T> query, string? orderBy, bool descending)
        {
            if (string.IsNullOrWhiteSpace(orderBy)) return query;

            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.PropertyOrField(parameter, orderBy);
            var lambda = Expression.Lambda(property, parameter);

            string method = descending ? "OrderByDescending" : "OrderBy";
            var expression = Expression.Call(typeof(Queryable), method,
                new Type[] { query.ElementType, property.Type },
                query.Expression, Expression.Quote(lambda));

            return query.Provider.CreateQuery<T>(expression);
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public async Task<int> UpdateWhereAsync(Expression<Func<T, bool>> predicate, Action<T> updateAction)
        {
            var items = await _context.Set<T>().Where(predicate).ToListAsync();

            foreach (var item in items)
            {
                updateAction(item);
            }

            return await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
