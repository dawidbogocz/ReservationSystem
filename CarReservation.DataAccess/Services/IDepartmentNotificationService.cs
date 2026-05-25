using CarReservation.Models;

namespace CarReservation.DataAccess.Services
{
    public interface IDepartmentNotificationService
    {
        Task<List<ApplicationUser>> GetAdminsAsync();
        Task<List<ApplicationUser>> GetGroupManagersAsync(string? userId);
        Task<List<ApplicationUser>> GetGroupManagersAndAdminsAsync(string? userId);

        Task SendToAdminsAsync(string subject, string body);
        Task SendToGroupManagersAndAdminsAsync(string? userId, string subject, string body);
    }
}