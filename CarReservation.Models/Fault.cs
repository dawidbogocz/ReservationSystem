using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CarReservation.Models
{
    /// <summary>
    /// Represents a fault or issue reported for an car.
    /// </summary>
    public class Fault
    {
        /// <summary>
        /// Gets or sets the unique identifier for the fault.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the tag of the asset associated with this fault.
        /// </summary>
        [Required]
        public string AssetTag { get; set; }

        /// <summary>
        /// Gets or sets the asset for which the fault was reported.
        /// </summary>
        [ForeignKey("AssetTag")]
        [ValidateNever]
        public Asset Asset { get; set; }

        /// <summary>
        /// Gets or sets the description of the fault.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the date when the fault was reported.
        /// </summary>
        [Required]
        public DateTime DateReported { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user who reported the fault.
        /// </summary>
        [Required]
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the user who reported the fault.
        /// </summary>
        [ForeignKey("UserId")]
        [ValidateNever]
        public ApplicationUser User { get; set; }

        /// <summary>
        /// Indicates whether the fault has been fixed.
        /// </summary>
        public bool IsFixed { get; set; } = false;

        /// <summary>
        /// Description of the fix applied to the fault.
        /// </summary>
        public string? FixDescription { get; set; }

        public DateTime? FixDate { get; set; }

        /// <summary>
        /// Indicates whether the fault is minor or major.
        /// </summary>
        public bool IsDrivable { get; set; } = false;
        [Display(Name = "Komentarz administratora"), MaxLength(500)]

        /// <summary>
        /// Description of the decision.
        /// </summary>
        public string? DrivableComment { get; set; }

        public string? FixedByUserId { get; set; }

        [ForeignKey("FixedByUserId")]
        [ValidateNever]
        public ApplicationUser? FixedByUser { get; set; }

    }
}