namespace ReservationApp.DataAccess.Services
{
    public interface IReservationService
    {
        Task CheckAndSendFeedbackReminders();
        Task SendUpcomingReservationReminders();
    }
}
