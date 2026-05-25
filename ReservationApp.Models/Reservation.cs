using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ReservationApp.Models
{
    /// <summary>
    /// Represents the approval status of a reservation.
    /// </summary>
    public enum Approval
    {
        /// <summary>
        /// The reservation is pending approval.
        /// </summary>
        Oczekujace,     // Pending

        /// <summary>
        /// The reservation has been accepted.
        /// </summary>
        Zaakceptowane,  // Accepted

        /// <summary>
        /// The reservation has been rejected.
        /// </summary>
        Odrzucone,       // Rejected

        /// <summary>
        /// The reservation has been canceled.
        /// </summary>
        Anulowana       // Canceled
    }

    /// <summary>
    /// Represents a reservation made by a user for an car.
    /// </summary>
    public class Reservation
    {
        /// <summary>
        /// Gets or sets the unique identifier for the reservation.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the tag of the asset being reserved.
        /// </summary>
        [Required]
        public string AssetTag { get; set; }

        /// <summary>
        /// Gets or sets the asset associated with this reservation.
        /// </summary>
        [ForeignKey("AssetTag")]
        [ValidateNever]
        public Asset Asset { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user making the reservation.
        /// </summary>
        [Required]
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the user who made the reservation.
        /// </summary>
        [ForeignKey("UserId")]
        [ValidateNever]
        public ApplicationUser User { get; set; }

        /// <summary>
        /// Gets or sets the pickup date for the reservation.
        /// </summary>
        [Required(ErrorMessage = "Data odebrania jest wymagana")]
        public DateTime PickupDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the return date for the reservation.
        /// </summary>
        [Required(ErrorMessage = "Data oddania jest wymagana")]
        public DateTime ReturnDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the destination for the reservation.
        /// </summary>
        [Required(ErrorMessage = "Cel podrózy jest wymagany")]
        [MaxLength(50)]
        [MinLength(2, ErrorMessage = "Cel wypożyczenia musi mieć przynajmniej {1} znaki")]
        public string Destination { get; set; }

        /// <summary>
        /// Gets or sets the approval status of the reservation.
        /// </summary>
        [Required]
        public Approval Approval { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user has accepted the terms and conditions.
        /// </summary>
        [Display(Name = "Przeczytałem i akceptuję regulamin")]
        [Required(ErrorMessage = "Musisz zaakceptować regulamin przed zarezerwowaniem samochodu.")]
        public bool AcceptStatute { get; set; }

        /// <summary>
        /// Indicates whether an email reminder has been sent for the reservation.
        /// </summary>
        public bool EmailReminderSent { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the asset was marked as dirty at pickup.
        /// </summary>
        public bool? IsCarDirtyAtPickup { get; set; }

        /// <summary>
        /// Gets or sets any faults reported at pickup.
        /// </summary>
        public string? PickupFaults { get; set; }

        public DateTime? PickupFeedbackDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the asset was marked as dirty at return.
        /// </summary>
        public bool? IsCarDirtyAtReturn { get; set; }

        /// <summary>
        /// Gets or sets any faults reported at return.
        /// </summary>
        public string? ReturnFaults { get; set; }

        public DateTime? ReturnFeedbackDate { get; set; }

        /// <summary>
        /// Gets or sets the identifier (or name) of the user who approved or rejected the reservation.
        /// </summary>
        public string? ApprovedBy { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the reservation was approved or rejected.
        /// </summary>
        public DateTime? ApprovalDate { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }

        public int? PickupMileage { get; set; }

        public int? ReturnMileage { get; set; }

    }
}