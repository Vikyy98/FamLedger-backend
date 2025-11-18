using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamLedger.Application.DTOs.Request
{
    public class CreateFamilyRequest
    {
        public string? FamilyName { get; set; }
        public int UserId { get; set; }
        public string? InvitationCode { get; set; }
        public string? InvitationLink { get; set; }
    }
}
