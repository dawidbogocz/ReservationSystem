using System.ComponentModel.DataAnnotations;

namespace CarReservation.Models
{
    /// <summary>
    /// Represents an organizational group/department used to route reservation notifications.
    /// </summary>
    public class UserGroup
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public ICollection<UserGroupManager> Managers { get; set; } = new List<UserGroupManager>();
    }
}
