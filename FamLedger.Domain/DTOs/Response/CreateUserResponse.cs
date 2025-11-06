using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamLedger.Domain.DTOs.Response
{
    public class CreateUserResponse
    {
        public int UserId { get; set; }
        public int FamilyId { get; set; }
        public string FamilyCode {  get; set; } = string.Empty;
    }
}
