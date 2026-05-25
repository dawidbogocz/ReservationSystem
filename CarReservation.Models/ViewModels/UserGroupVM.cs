using CarReservation.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarReservation.Models.ViewModels
{
    public class UserGroupVM
    {
        public UserGroup UserGroup { get; set; } = new UserGroup();

        public List<string> SelectedManagerIds { get; set; } = new();
        public List<string> SelectedEmployeeIds { get; set; } = new();

        public IEnumerable<SelectListItem> ManagerList { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> EmployeeList { get; set; } = new List<SelectListItem>();
    }
}