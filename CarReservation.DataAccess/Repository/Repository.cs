using System.Linq.Expressions;
using CarReservation.DataAccess.Data;
using CarReservation.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace CarReservation.DataAccess.Repository
{
    /// <summary>
    /// Provides a generic repository for data access operations.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _db;
        internal DbSet<T> dbSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="Repository{T}"/> class.
        /// </summary>
        /// <param name="db">The application database context.</param>
        public Repository(ApplicationDbContext db)
        {
            _db = db;
            this.dbSet = _db.Set<T>();
        }

        /// <summary>
        /// Adds the specified entity to the repository.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        public void Add(T entity)
        {
            dbSet.Add(entity);
        }

        /// <summary>
        /// Retrieves all entities from the repository, optionally including related properties.
        /// </summary>
        /// <param name="includeProperties">
        /// A comma-separated list of related properties to include.
        /// </param>
        /// <returns>A collection of entities.</returns>
        public IEnumerable<T> GetAll(string? includeProperties = null)
        {
            return GetAll(null, includeProperties);
        }

        /// <summary>
        /// Retrieves all entities matching the filter from the repository, optionally including related properties.
        /// </summary>
        /// <param name="filter">A lambda expression to filter entities.</param>
        /// <param name="includeProperties">
        /// A comma-separated list of related properties to include.
        /// </param>
        /// <returns>A collection of entities.</returns>
        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = null)
        {
            IQueryable<T> query = dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return query.ToList();
        }

        /// <summary>
        /// Retrieves a single entity that matches the specified filter.
        /// </summary>
        /// <param name="filter">A lambda expression to filter entities.</param>
        /// <param name="includeProperties">
        /// A comma-separated list of related properties to include.
        /// </param>
        /// <returns>The first matching entity, or null if no match is found.</returns>
        public T Get(Expression<Func<T, bool>> filter, string? includeProperties = null)
        {
            IQueryable<T> query = dbSet.Where(filter);
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return query.FirstOrDefault();
        }

        /// <summary>
        /// Removes the specified entity from the repository.
        /// </summary>
        /// <param name="entity">The entity to remove.</param>
        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        /// <summary>
        /// Removes a range of entities from the repository.
        /// </summary>
        /// <param name="entity">The collection of entities to remove.</param>
        public void RemoveRange(IEnumerable<T> entity)
        {
            dbSet.RemoveRange(entity);
        }

        /// <summary>
        /// Asynchronously retrieves all entities.
        /// </summary>
        /// <param name="includeProperties">Comma-separated related properties to include.</param>
        /// <returns>A task that represents the asynchronous operation, containing the list of entities.</returns>
        public async Task<IEnumerable<T>> GetAllAsync(string? includeProperties = null)
        {
            return await GetAllAsync(null, includeProperties);
        }

        /// <summary>
        /// Asynchronously retrieves all entities matching the filter.
        /// </summary>
        /// <param name="filter">The filter expression.</param>
        /// <param name="includeProperties">Comma-separated related properties to include.</param>
        /// <returns>A task that represents the asynchronous operation, containing the list of entities.</returns>
        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null)
        {
            IQueryable<T> query = dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return await query.ToListAsync();
        }

        /// <summary>
        /// Asynchronously retrieves the first entity matching the filter.
        /// </summary>
        /// <param name="filter">The filter expression.</param>
        /// <param name="includeProperties">Comma-separated related properties to include.</param>
        /// <returns>A task that represents the asynchronous operation, containing the first matching entity or null.</returns>
        public async Task<T> GetAsync(Expression<Func<T, bool>> filter, string? includeProperties = null)
        {
            IQueryable<T> query = dbSet.Where(filter);
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return await query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Asynchronously adds an entity to the repository.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns>A task that represents the asynchronous add operation.</returns>
        public async Task AddAsync(T entity) => await dbSet.AddAsync(entity);
    }
}