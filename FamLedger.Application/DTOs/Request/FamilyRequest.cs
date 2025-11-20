using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamLedger.Application.DTOs.Request
{
    public class FamilyRequest
    {
        [Required]
        public string? FamilyName { get; set; }
        [Required]
        public int UserId { get; set; }
        public string? InvitationCode { get; set; }
        public string? InvitationLink { get; set; }
    }
}
