using ReservationApp.Models;
using System.Linq.Expressions;

namespace ReservationApp.DataAccess.Repository.IRepository
{
    /// <summary>
    /// Provides a contract for repository operations on <see cref="Asset"/> entities.
    /// </summary>
    public interface IAssetRepository : IRepository<Asset>
    {
        /// <summary>
        /// Updates the specified <see cref="Asset"/> entity in the data store.
        /// </summary>
        /// <param name="obj">The <see cref="Asset"/> entity to update.</param>
        void Update(Asset obj);

        /// <summary>
        /// Returns an IQueryable for the Asset entity set, allowing server-side composition of filters.
        /// </summary>
        IQueryable<Asset> GetAllQueryable(Expression<Func<Asset, bool>>? filter = null, string? includeProperties = null);
    }
}