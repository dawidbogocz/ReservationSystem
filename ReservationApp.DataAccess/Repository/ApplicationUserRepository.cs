using ReservationApp.DataAccess.Data;
using ReservationApp.DataAccess.Repository.IRepository;
using ReservationApp.Models;

namespace ReservationApp.DataAccess.Repository
{
    /// <summary>
    /// Provides repository methods for managing <see cref="ApplicationUser"/> entities.
    /// </summary>
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private ApplicationDbContext _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationUserRepository"/> class.
        /// </summary>
        /// <param name="db">The application database context.</param>
        public ApplicationUserRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        /// <summary>
        /// Updates an existing <see cref="ApplicationUser"/> entity in the data store.
        /// </summary>
        /// <param name="obj">The <see cref="ApplicationUser"/> entity to update.</param>
        public void Update(ApplicationUser obj)
        {
            _db.ApplicationUser.Update(obj);
        }
    }
}
