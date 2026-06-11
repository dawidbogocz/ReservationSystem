using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ReservationApp.Models
{
    public enum AssetType
    {
        Default
    }

    /// <summary>
    /// Represents an asset that can be reserved in the system.
    /// </summary>
    public class Asset
    {
        /// <summary>
        /// Gets or sets the unique tag of the asset.
        /// </summary>
        [Key]
        [Required(ErrorMessage = "Asset tag is required")]
        [MaxLength(30)]
        [MinLength(5, ErrorMessage = "Asset tag must be at least {1} characters")]
        public string AssetTag { get; set; }

        [Required]
        [Display(Name = "Asset type")]
        public AssetType AssetType { get; set; } = AssetType.Default;

        /// <summary>
        /// Gets or sets the make (manufacturer) of the asset.
        /// </summary>
        [Required(ErrorMessage = "Make is required")]
        [MaxLength(30)]
        [MinLength(2, ErrorMessage = "Make must be at least {1} characters")]
        public string Make { get; set; }

        /// <summary>
        /// Gets or sets the model of the asset.
        /// </summary>
        [Required(ErrorMessage = "Model is required")]
        [MaxLength(30)]
        [MinLength(1, ErrorMessage = "Model must be at least {1} character")]
        public string Model { get; set; }

        /// <summary>
        /// Gets or sets the inspection date of the asset.
        /// </summary>
        [Required(ErrorMessage = "Inspection date is required")]
        [DefaultValue("2022-01-01")]
        public DateOnly InspectionDate { get; set; }

        /// <summary>
        /// Gets or sets the service date of the asset.
        /// </summary>
        [Required(ErrorMessage = "Service date is required")]
        [DefaultValue("2022-01-01")]
        public DateOnly ServiceDate { get; set; }

        /// <summary>
        /// Gets or sets the URL of the asset's image.
        /// </summary>
        [ValidateNever]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the collection of faults reported for the asset.
        /// </summary>
        [ValidateNever]
        public ICollection<Fault> Faults { get; set; } = new List<Fault>();

        /// <summary>
        /// Gets or sets the collection of reservations associated with the asset.
        /// </summary>
        [ValidateNever]
        public ICollection<Reservation> Reservations { get; set; }

        /// <summary>
        /// Indicates whether the asset is currently marked as damaged.
        /// </summary>
        public bool IsDamaged { get; set; } = false;

        /// <summary>
        /// Indicates whether the asset has telemetry enabled.
        /// </summary>
        public bool HasTelemetry { get; set; } = false;

        [Display(Name = "Usage count")]
        [Range(0, int.MaxValue)]
        public int UsageCount { get; set; } = 0;

        [Display(Name = "Condition [%]")]
        [Range(0, 100)]
        public int ConditionLevel { get; set; } = 100;

        /// <summary>
        /// Indicates whether the asset has been soft-deleted.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

    }
}
