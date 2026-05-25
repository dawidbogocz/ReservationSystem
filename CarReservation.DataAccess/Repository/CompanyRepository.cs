//Company model is not used in the project
/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CarReservation.DataAccess.Data;
using CarReservation.DataAccess.Repository.IRepository;
using CarReservation.Models;

namespace CarReservation.DataAccess.Repository
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        private ApplicationDbContext _db;
        public CompanyRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Company obj)
        {
            _db.Company.Update(obj);
        }
    }
}
*/