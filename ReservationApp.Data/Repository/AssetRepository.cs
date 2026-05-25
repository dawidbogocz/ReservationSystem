using System.Linq.Expressions;
using ReservationApp.DataAccess.Data;
using ReservationApp.DataAccess.Repository.IRepository;
using ReservationApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ReservationApp.DataAccess.Repository
{
    /// <summary>
    /// Provides repository methods for managing <see cref="Asset"/> entities.
    /// </summary>
    public class AssetRepository : Repository<Asset>, IAssetRepository
    {
        private ApplicationDbContext _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetRepository"/> class.
        /// </summary>
        /// <param name="db">The application database context.</param>
        public AssetRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        /// <summary>
        /// Updates an existing <see cref="Asset"/> entity in the data store.
        /// </summary>
        /// <param name="obj">The <see cref="Asset"/> entity to update.</param>
        public void Update(Asset obj)
        {
            _db.Asset.Update(obj);
        }

        public IQueryable<Asset> GetAllQueryable(Expression<Func<Asset, bool>>? filter = null, string? includeProperties = null)
        {
            IQueryable<Asset> query = _db.Set<Asset>();
            if (filter != null)
                query = query.Where(filter);
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return query;
        }
    }
}