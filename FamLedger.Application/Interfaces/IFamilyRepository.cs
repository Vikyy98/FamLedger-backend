using FamLedger.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamLedger.Application.Interfaces
{
    public interface IFamilyRepository
    {
        Task<Family?> GetLastFamilyAsync();
        Task<Family?> GetFamilyByIdAsync(int familyId);
        Task AddFamilyAsync(Family family); 
    }
}
