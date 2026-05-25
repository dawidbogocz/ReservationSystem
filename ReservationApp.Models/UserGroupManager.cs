using System.ComponentModel.DataAnnotations.Schema;

namespace ReservationApp.Models
{
    /// <summary>
    /// Joins a user group with one of its managers.
    /// </summary>
    public class UserGroupManager
    {
        public int UserGroupId { get; set; }

        [ForeignKey(nameof(UserGroupId))]
        public UserGroup UserGroup { get; set; } = null!;

        public string ManagerId { get; set; } = string.Empty;

        [ForeignKey(nameof(ManagerId))]
        public ApplicationUser Manager { get; set; } = null!;
    }
}
