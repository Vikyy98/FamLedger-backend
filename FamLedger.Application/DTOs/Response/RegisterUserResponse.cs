using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamLedger.Application.DTOs.Response
{
    public class RegisterUserResponse
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public int? FamilyId { get; set; }
        public string? FamilyCode { get; set; }
    }
}
