using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CarReservation.Models
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
        [Required(ErrorMessage = "Numer rejestracji jest wymagany")]
        [MaxLength(30)]
        [MinLength(5, ErrorMessage = "Numer rejestracji musi mieć przynajmniej {1} znaków")]
        public string AssetTag { get; set; }

        [Required]
        [Display(Name = "Typ pojazdu")]
        public AssetType AssetType { get; set; } = AssetType.Car;

        /// <summary>
        /// Gets or sets the make (manufacturer) of the car.
        /// </summary>
        [Required(ErrorMessage = "Marka jest wymagana")]
        [MaxLength(30)]
        [MinLength(2, ErrorMessage = "Marka musi mieć przynajmniej {1} znaki")]
        public string Make { get; set; }

        /// <summary>
        /// Gets or sets the model of the car.
        /// </summary>
        [Required(ErrorMessage = "Model jest wymagany")]
        [MaxLength(30)]
        [MinLength(1, ErrorMessage = "Model musi mieć przynajmniej {1} znak")]
        public string Model { get; set; }

        /// <summary>
        /// Gets or sets the inspection date of the car.
        /// </summary>
        [Required(ErrorMessage = "Data przeglądu jest wymagana")]
        [DefaultValue("2022-01-01")]
        public DateOnly InspectionDate { get; set; }

        /// <summary>
        /// Gets or sets the service date of the car.
        /// </summary>
        [Required(ErrorMessage = "Data serwisu jest wymagana")]
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

        [Display(Name = "Przebieg [km]")]
        [Range(0, int.MaxValue)]
        public int Mileage { get; set; } = 0;

        [Display(Name = "Poziom paliwa [%]")]
        [Range(0, 100)]
        public int FuelLevel { get; set; } = 100;

        /// <summary>
        /// Indicates whether the asset has been soft-deleted.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

    }
}
