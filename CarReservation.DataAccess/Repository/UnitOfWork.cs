using CarReservation.DataAccess.Data;
using CarReservation.DataAccess.Repository.IRepository;

namespace CarReservation.DataAccess.Repository
{
    /// <summary>
    /// Implements the Unit of Work pattern to encapsulate a set of repository operations into a single transaction.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db;

        /// <summary>
        /// Gets the asset repository.
        /// </summary>
        public IAssetRepository Asset { get; private set; }

        /// <summary>
        /// Gets the reservation repository.
        /// </summary>
        public IReservationRepository Reservation { get; private set; }

        /// <summary>
        /// Gets the application user repository.
        /// </summary>
        public IApplicationUserRepository ApplicationUser { get; private set; }


        /// <summary>
        /// Gets the fault repository.
        /// </summary>
        public IFaultRepository Fault { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
        /// </summary>
        /// <param name="db">The application database context.</param>
        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            Asset = new AssetRepository(_db);
            Reservation = new ReservationRepository(_db);
            ApplicationUser = new ApplicationUserRepository(_db);
            Fault = new FaultRepository(_db);
        }

        /// <summary>
        /// Gets the underlying application database context.
        /// </summary>
        public ApplicationDbContext Context => _db;

        /// <summary>
        /// Commits all changes made in the context to the database.
        /// </summary>
        public void Save()
        {
            _db.SaveChanges();
        }
    }
}