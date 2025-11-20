using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamLedger.Application.DTOs.Response
{
    public class FamilyResponse
    {
        public int FamilyId {  get; set; }
        public string? FamilyCode { get; set; }
        public string? InvitationCode { get; set; }
        public string? InvitationLink {  get; set; }
    }
}
