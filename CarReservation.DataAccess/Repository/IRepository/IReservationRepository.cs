using CarReservation.Models;

namespace CarReservation.DataAccess.Repository.IRepository
{
    /// <summary>
    /// Provides a contract for repository operations on <see cref="Reservation"/> entities.
    /// </summary>
    public interface IReservationRepository : IRepository<Reservation>
    {
        /// <summary>
        /// Updates the specified <see cref="Reservation"/> entity in the data store.
        /// </summary>
        /// <param name="obj">The <see cref="Reservation"/> entity to update.</param>
        void Update(Reservation obj);
    }
}
