using CarReservation.DataAccess.Data;

namespace CarReservation.DataAccess.Repository.IRepository
{
    /// <summary>
    /// Defines the contract for a unit of work that encapsulates multiple repository operations.
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Gets the repository for <see cref="Asset"/> entities.
        /// </summary>
        IAssetRepository Asset { get; }

        /// <summary>
        /// Gets the repository for <see cref="Reservation"/> entities.
        /// </summary>
        IReservationRepository Reservation { get; }

        /// <summary>
        /// Gets the repository for <see cref="ApplicationUser"/> entities.
        /// </summary>
        IApplicationUserRepository ApplicationUser { get; }


        /// <summary>
        /// Gets the repository for <see cref="Fault"/> entities.
        /// </summary>
        IFaultRepository Fault { get; }

        /// <summary>
        /// Gets the underlying application database context.
        /// </summary>
        ApplicationDbContext Context { get; }

        /// <summary>
        /// Persists all changes made in the unit of work to the database.
        /// </summary>
        void Save();
    }
}