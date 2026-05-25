using CarReservation.Models;

namespace CarReservation.DataAccess.Repository.IRepository
{
    /// <summary>
    /// Provides a contract for repository operations on <see cref="ApplicationUser"/> entities.
    /// </summary>
    public interface IApplicationUserRepository : IRepository<ApplicationUser>
    {
        /// <summary>
        /// Updates the specified <see cref="ApplicationUser"/> entity in the data store.
        /// </summary>
        /// <param name="obj">The <see cref="ApplicationUser"/> entity to update.</param>
        void Update(ApplicationUser obj);
    }
}
