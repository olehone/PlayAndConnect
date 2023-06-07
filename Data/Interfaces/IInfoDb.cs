using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlayAndConnect.Models;
namespace PlayAndConnect.Data.Interfaces
{
    public interface IInfoDb : IBaseDb<PlayAndConnect.Models.Info>
    {
        Task<Info?> GetInfoByUser(User? user);
        Task<Info?> GetInfoByLogin(string? login);
    }
}