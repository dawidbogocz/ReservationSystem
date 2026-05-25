using CarReservation.DataAccess.Data;
using CarReservation.DataAccess.Repository.IRepository;
using CarReservation.Models;

namespace CarReservation.DataAccess.Repository
{
    /// <summary>
    /// Provides repository methods for managing <see cref="Reservation"/> entities.
    /// </summary>
    public class ReservationRepository : Repository<Reservation>, IReservationRepository
    {
        private ApplicationDbContext _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationRepository"/> class.
        /// </summary>
        /// <param name="db">The application database context.</param>
        public ReservationRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        /// <summary>
        /// Updates an existing <see cref="Reservation"/> entity in the data store.
        /// </summary>
        /// <param name="obj">The <see cref="Reservation"/> entity to update.</param>
        public void Update(Reservation obj)
        {
            _db.Reservation.Update(obj);
        }
    }
}
