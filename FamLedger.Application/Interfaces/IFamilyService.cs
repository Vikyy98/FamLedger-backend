using FamLedger.Application.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamLedger.Application.Interfaces
{
    public interface IFamilyService
    {
        Task<CreateFamilyResponse> CreateFamilyAsync(int userId, string familyName);
    }
}
