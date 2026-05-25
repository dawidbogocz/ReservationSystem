using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ReservationApp.Models.ViewModels
{
    /// <summary>
    /// Represents the view model for displaying or editing fault information.
    /// </summary>
    public class FaultVM
    {
        /// <summary>
        /// Gets or sets the fault entity.
        /// </summary>
        public Fault Fault { get; set; }

        /// <summary>
        /// Gets or sets the list of assets for selection in the fault reporting interface.
        /// </summary>
        [ValidateNever]
        public IEnumerable<SelectListItem> AssetList { get; set; }

        /// <summary>
        /// Gets or sets the list of users for selection in the fault reporting interface.
        /// </summary>
        [ValidateNever]
        public IEnumerable<SelectListItem> UserList { get; set; }
    }
}
