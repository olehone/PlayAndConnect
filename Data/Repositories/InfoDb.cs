using PlayAndConnect.Models;
using PlayAndConnect.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace PlayAndConnect.Data.Interfaces
{
    public class InfoDb : IInfoDb
    {
        private readonly ApplicationDbContext _db;
        public InfoDb(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<Info?> GetInfoByUser(User? user)
        {
            if (user != null)
                return await _db.Infos.FirstOrDefaultAsync<Info>(i => i.UserId == user.Id);
            else
                return null;
        }
        public async Task<Info?> GetInfoByLogin(string? login)
        {
            if (login != null)
                return await _db.Infos.FirstOrDefaultAsync<Info>(i => i.User.Login == login);
            else
                return null;
        }
        public async Task<Info?> GetById(int? id)
        {
            if (id != null)
                return await _db.Infos.FirstOrDefaultAsync<Info>(i => i.Id == id);
            else
                return null;
        }
    }
}