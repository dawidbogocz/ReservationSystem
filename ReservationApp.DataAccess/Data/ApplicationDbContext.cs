using ReservationApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ReservationApp.DataAccess.Data
{
    /// <summary>
    /// Represents the Entity Framework Core database context for the Reservation application.
    /// Inherits from <see cref="IdentityDbContext{IdentityUser}"/> to integrate ASP.NET Identity.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class using the specified options.
        /// </summary>
        /// <param name="options">The options to be used by the DbContext.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the DbSet of <see cref="Asset"/> entities.
        /// </summary>
        public DbSet<Asset> Asset { get; set; }

        /// <summary>
        /// Gets or sets the DbSet of <see cref="Reservation"/> entities.
        /// </summary>
        public DbSet<Reservation> Reservation { get; set; }

        /// <summary>
        /// Gets or sets the DbSet of <see cref="ApplicationUser"/> entities.
        /// </summary>
        public DbSet<ApplicationUser> ApplicationUser { get; set; }

        /// <summary>
        /// Gets or sets the DbSet of <see cref="UserGroup"/> entities.
        /// </summary>
        public DbSet<UserGroup> UserGroups { get; set; }

        /// <summary>
        /// Gets or sets the DbSet of <see cref="UserGroupManager"/> entities.
        /// </summary>
        public DbSet<UserGroupManager> UserGroupManagers { get; set; }

        /// <summary>
        /// Gets or sets the DbSet of <see cref="Fault"/> entities.
        /// </summary>
        public DbSet<Fault> Fault { get; set; }

        /// <summary>
        /// Gets or sets the DbSet of <see cref="FeedbackLogs"/> entities.
        /// </summary>
        public DbSet<FeedbackLog> FeedbackLogs { get; set; }


        /// <summary>
        /// Configures the model and seeds initial data into the database.
        /// This method is called when the model for a derived context has been initialized.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply global query filter for soft-delete
            modelBuilder.Entity<Asset>().HasQueryFilter(c => !c.IsDeleted);

            // Configure ApplicationUser discriminator explicitly.
            // After switching from IdentityDbContext<IdentityUser> to
            // IdentityDbContext<ApplicationUser>, EF Core's TPH convention
            // needs an explicit discriminator value to avoid inserting NULL
            // into the AspNetUsers.Discriminator column.
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.HasDiscriminator<string>("Discriminator")
                    .HasValue("ApplicationUser");
            });

            // Seed initial data for Asset entities.
            modelBuilder.Entity<Asset>().HasData(
                new Asset { AssetTag = "ABC123", Make = "Toyota", Model = "Corolla", InspectionDate = new System.DateOnly(2022, 1, 1), ServiceDate = new System.DateOnly(2022, 1, 1), ImageUrl = "" },
                new Asset { AssetTag = "DEF456", Make = "Audi", Model = "A4", InspectionDate = new System.DateOnly(2022, 1, 1), ServiceDate = new System.DateOnly(2022, 1, 1), ImageUrl = "" }
            );

            // Seed initial data for Reservation entities.
            modelBuilder.Entity<Reservation>().HasData(
                new Reservation { Id = 1, AssetTag = "ABC123", UserId = "1", PickupDate = new System.DateTime(2022, 1, 1), ReturnDate = new System.DateTime(2022, 1, 2), Destination = "Wadowice", Approval = Approval.Oczekujace },
                new Reservation { Id = 2, AssetTag = "DEF456", UserId = "2", PickupDate = new System.DateTime(2022, 1, 3), ReturnDate = new System.DateTime(2022, 1, 4), Destination = "Kraków", Approval = Approval.Zaakceptowane },
                new Reservation { Id = 3, AssetTag = "ABC123", UserId = "3", PickupDate = new System.DateTime(2022, 1, 5), ReturnDate = new System.DateTime(2022, 1, 6), Destination = "Warszawa", Approval = Approval.Odrzucone }
            );

            // Seed initial data for Fault entities.
            modelBuilder.Entity<Fault>().HasData(
                new Fault { Id = 1, Description = "Awaria1", AssetTag = "ABC123", UserId = "1" },
                new Fault { Id = 2, Description = "Awaria2", AssetTag = "DEF456", UserId = "2" }
            );

            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.UserGroup)
                .WithMany(g => g.Users)
                .HasForeignKey(u => u.UserGroupId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<UserGroupManager>()
                .HasKey(x => new { x.UserGroupId, x.ManagerId });

            modelBuilder.Entity<UserGroupManager>()
                .HasOne(x => x.UserGroup)
                .WithMany(g => g.Managers)
                .HasForeignKey(x => x.UserGroupId);

            modelBuilder.Entity<UserGroupManager>()
                .HasOne(x => x.Manager)
                .WithMany()
                .HasForeignKey(x => x.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed initial data for ApplicationUser entities.
            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => new { u.Email, u.FirstName, u.LastName })
                .IsUnique();

            modelBuilder.Entity<ApplicationUser>().HasData(
                new ApplicationUser { Id = "1", FirstName = "Jan", LastName = "Kowalski" },
                new ApplicationUser { Id = "2", FirstName = "Anna", LastName = "Nowak" },
                new ApplicationUser { Id = "3", FirstName = "Piotr", LastName = "Wiśniewski" }
            );

            modelBuilder.Entity<FeedbackLog>()
                .HasOne<Reservation>(f => f.Reservation)
                .WithMany()
                .HasForeignKey(f => f.ReservationId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<FeedbackLog>()
                .HasOne<ApplicationUser>(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<FeedbackLog>()
                .HasIndex(f => new { f.ReservationId, f.Kind })
                .IsUnique();
        }
    }
}