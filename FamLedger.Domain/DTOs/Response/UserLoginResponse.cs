using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamLedger.Domain.DTOs.Response
{
    public class UserLoginResponse
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string FamilyName {  get; set; }
        public string Role {  get; set; }

    }
}
