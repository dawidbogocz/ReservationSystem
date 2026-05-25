using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ReservationApp.Models.ViewModels
{
    /// <summary>
    /// Represents the view model for displaying or editing reservation details.
    /// </summary>
    public class ReservationVM
    {
        /// <summary>
        /// Gets or sets the reservation entity.
        /// </summary>
        public Reservation Reservation { get; set; }

        /// <summary>
        /// Gets or sets the list of available assets for selection in the reservation interface.
        /// </summary>
        [ValidateNever]
        public IEnumerable<SelectListItem> AssetList { get; set; }

        /// <summary>
        /// Gets or sets the list of users for selection in the reservation interface.
        /// </summary>
        [ValidateNever]
        public IEnumerable<SelectListItem> UserList { get; set; }
    }
}
