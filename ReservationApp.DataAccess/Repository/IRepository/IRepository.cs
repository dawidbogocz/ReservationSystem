using System.Linq.Expressions;

namespace ReservationApp.DataAccess.Repository.IRepository
{
    /// <summary>
    /// Provides a generic contract for repository operations.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Retrieves all entities, optionally including specified related properties.
        /// </summary>
        /// <param name="includeProperties">A comma-separated list of related properties to include.</param>
        /// <returns>A collection of entities.</returns>
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);

        /// <summary>
        /// Retrieves a single entity that satisfies the specified filter expression,
        /// optionally including related properties.
        /// </summary>
        /// <param name="filter">A lambda expression used to filter entities.</param>
        /// <param name="includeProperties">A comma-separated list of related properties to include.</param>
        /// <returns>The first entity that matches the filter, or null if no match is found.</returns>
        T Get(Expression<Func<T, bool>> filter, string? includeProperties = null);

        /// <summary>
        /// Adds the specified entity to the repository.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        void Add(T entity);

        /// <summary>
        /// Removes the specified entity from the repository.
        /// </summary>
        /// <param name="entity">The entity to remove.</param>
        void Remove(T entity);

        /// <summary>
        /// Removes a range of entities from the repository.
        /// </summary>
        /// <param name="entity">The collection of entities to remove.</param>
        void RemoveRange(IEnumerable<T> entity);

        /// <summary>
        /// Asynchronously retrieves all entities.
        /// </summary>
        /// <param name="includeProperties">Comma-separated related properties to include.</param>
        /// <returns>A task that represents the asynchronous operation, containing the list of entities.</returns>
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);

        /// <summary>
        /// Asynchronously retrieves the first entity matching the filter.
        /// </summary>
        /// <param name="filter">The filter expression.</param>
        /// <param name="includeProperties">Comma-separated related properties to include.</param>
        /// <returns>A task that represents the asynchronous operation, containing the first matching entity or null.</returns>
        Task<T> GetAsync(Expression<Func<T, bool>> filter, string? includeProperties = null);

        /// <summary>
        /// Asynchronously adds an entity to the repository.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns>A task that represents the asynchronous add operation.</returns>
        Task AddAsync(T entity);
    }
}
