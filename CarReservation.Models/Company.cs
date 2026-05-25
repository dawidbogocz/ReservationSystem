//Company model is not used in the project
/*using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarReservation.Models
{
    public class Company
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public string Location { get; set; }

        [Required]
        public string NIP { get; set; }

        public ICollection<ApplicationUser> Users { get; set; }
        public ICollection<Asset> Cars { get; set; }
    }
}
*/