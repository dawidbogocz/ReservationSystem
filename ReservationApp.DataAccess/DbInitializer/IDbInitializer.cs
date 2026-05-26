namespace ReservationApp.DataAccess.DbInitializer
{
    /// <summary>
    /// Provides a contract for initializing the database, including applying migrations and seeding data.
    /// </summary>
    public interface IDbInitializer
    {
        /// <summary>
        /// Initializes the database by applying migrations and seeding initial data.
        /// </summary>
        void Initialize();
    }
}
