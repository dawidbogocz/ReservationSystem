using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ReservationApp.Models
{
    public enum AssetType
    {
        Car,
        Lift
    }

    /// <summary>
    /// Represents an asset that can be reserved in the system.
    /// </summary>
    public class Asset
    {
        /// <summary>
        /// Gets or sets the unique tag of the car.
        /// </summary>
        [Key]
        [Required(ErrorMessage = "Registration number is required")]
        [MaxLength(30)]
        [MinLength(5, ErrorMessage = "Registration number must be at least {1} characters")]
        public string AssetTag { get; set; }

        [Required]
        [Display(Name = "Asset type")]
        public AssetType AssetType { get; set; } = AssetType.Car;

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
        /// Gets or sets the collection of faults reported for the car.
        /// </summary>
        [ValidateNever]
        public ICollection<Fault> Faults { get; set; } = new List<Fault>();

        /// <summary>
        /// Gets or sets the collection of reservations associated with the car.
        /// </summary>
        [ValidateNever]
        public ICollection<Reservation> Reservations { get; set; }

        /// <summary>
        /// Indicates whether the asset is currently marked as damaged.
        /// </summary>
        public bool IsDamaged { get; set; } = false;

        /// <summary>
        /// Indicates whether the asset has tracking enabled.
        /// </summary>
        public bool HasTracking { get; set; } = false;

        [Display(Name = "Mileage [km]")]
        [Range(0, int.MaxValue)]
        public int Mileage { get; set; } = 0;

        [Display(Name = "Fuel level [%]")]
        [Range(0, 100)]
        public int FuelLevel { get; set; } = 100;

        /// <summary>
        /// Indicates whether the asset has been soft-deleted.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

    }
}
