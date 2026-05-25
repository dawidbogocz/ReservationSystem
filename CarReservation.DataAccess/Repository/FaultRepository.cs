using CarReservation.DataAccess.Data;
using CarReservation.DataAccess.Repository.IRepository;
using CarReservation.Models;

namespace CarReservation.DataAccess.Repository
{
    /// <summary>
    /// Provides repository methods for managing <see cref="Fault"/> entities.
    /// </summary>
    public class FaultRepository : Repository<Fault>, IFaultRepository
    {
        private ApplicationDbContext _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="FaultRepository"/> class.
        /// </summary>
        /// <param name="db">The application database context.</param>
        public FaultRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        /// <summary>
        /// Updates an existing <see cref="Fault"/> entity in the data store.
        /// </summary>
        /// <param name="obj">The <see cref="Fault"/> entity to update.</param>
        public void Update(Fault obj)
        {
            _db.Fault.Update(obj);
        }
    }
}
