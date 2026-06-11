using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReservationApp.Models
{
    public enum FeedbackKind { Pickup = 0, Return = 1 }

    public enum FeedbackStatus { Pending = 0, Provided = 1, Expired = 2 }

    public class FeedbackLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ReservationId { get; set; }
        [ForeignKey(nameof(ReservationId))]
        public Reservation Reservation { get; set; }

        [Required, MaxLength(30)]
        public string AssetTag { get; set; } = default!;

        [Required]
        public string UserId { get; set; } = default!;
        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; }

        [Required]
        public FeedbackKind Kind { get; set; }

        [Required]
        public FeedbackStatus Status { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int? Mileage { get; set; }
        public int? FuelLevel { get; set; }
        public bool? IsAssetDamaged { get; set; }
        public bool? HasFaults { get; set; }
        public string? Faults { get; set; }
    }
}