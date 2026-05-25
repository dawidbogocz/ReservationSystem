using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace ReservationApp.Models
{
    /// <summary>
    /// Represents an application user with extended properties used for car reservations.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Gets or sets the user's first name.
        /// </summary>
        [Required]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the user's last name.
        /// </summary>
        [Required]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the collection of reservations associated with the user.
        /// </summary>
        public ICollection<Reservation> Reservations { get; set; }

        /// <summary>
        /// Gets or sets the organizational group/department assigned to the user.
        /// </summary>
        public int? UserGroupId { get; set; }

        [ForeignKey(nameof(UserGroupId))]
        public UserGroup? UserGroup { get; set; }
    }
}
