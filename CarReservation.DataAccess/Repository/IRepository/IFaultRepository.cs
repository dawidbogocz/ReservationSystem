using CarReservation.Models;

namespace CarReservation.DataAccess.Repository.IRepository
{
    /// <summary>
    /// Provides a contract for repository operations on <see cref="Fault"/> entities.
    /// </summary>
    public interface IFaultRepository : IRepository<Fault>
    {
        /// <summary>
        /// Updates the specified <see cref="Fault"/> entity in the data store.
        /// </summary>
        /// <param name="obj">The <see cref="Fault"/> entity to update.</param>
        void Update(Fault obj);
    }
}
