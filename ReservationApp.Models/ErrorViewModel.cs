namespace ReservationApp.Models
{
    /// <summary>
    /// Represents the model used to display error information.
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// Gets or sets the unique identifier for the current request.
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="RequestId"/> property has a non-empty value.
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
