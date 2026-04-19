using FamLedger.Application.DTOs.Request;
using FamLedger.Application.DTOs.Response;
using FamLedger.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FamLedger.Api.Controllers
{
    [Authorize]
    [Route("api/families")]
    [ApiController]
    public class FamilyController : ControllerBase
    {
        private readonly ILogger<FamilyController> _logger;
        private readonly IFamilyService _familyService;

        public FamilyController(ILogger<FamilyController> logger, IFamilyService familyService)
        {
            _logger = logger;
            _familyService = familyService;
        }

        [HttpPost]
        public async Task<ActionResult<FamilyResponse>> CreateFamily([FromBody] FamilyRequest familyRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Request is invalid");
                }
                if (familyRequest == null || string.IsNullOrWhiteSpace(familyRequest.FamilyName))
                {
                    return BadRequest("Family name is required");
                }

                if (!TryGetCurrentUserId(out var userId))
                {
                    return Unauthorized(new { message = "Invalid or missing user identity" });
                }

                var outcome = await _familyService.CreateFamilyAsync(userId, familyRequest.FamilyName!);
                return outcome.Status switch
                {
                    FamilyCreateStatus.Ok when outcome.Response != null =>
                        Created($"/api/families/{outcome.Response.FamilyId}", outcome.Response),
                    FamilyCreateStatus.UserNotFound => NotFound(new { message = "User not found" }),
                    FamilyCreateStatus.AlreadyInFamily => Conflict(new { message = "You already belong to a family" }),
                    _ => StatusCode(500, new { message = "An internal error occurred" }),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating family");
                return StatusCode(500, new { message = "An internal error occurred" });
            }
        }

        /// <summary>Admin only. Replaces any previous code. Plain code is returned once; valid 24 hours.</summary>
        [HttpPost("{familyId:int}/invitation")]
        public async Task<ActionResult<FamilyInvitationResponse>> CreateFamilyInvitation(int familyId)
        {
            try
            {
                if (!TryGetCurrentUserId(out var userId))
                {
                    return Unauthorized(new { message = "Invalid or missing user identity" });
                }

                var outcome = await _familyService.CreateFamilyInvitationAsync(familyId, userId);
                return outcome.Status switch
                {
                    FamilyInvitationStatus.Ok when outcome.Response != null => Ok(outcome.Response),
                    FamilyInvitationStatus.Forbidden => Forbid(),
                    FamilyInvitationStatus.NotFound => NotFound(),
                    FamilyInvitationStatus.Failed => StatusCode(500, new { message = "Could not create invitation" }),
                    _ => StatusCode(500, new { message = "An internal error occurred" }),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating family invitation");
                return StatusCode(500, new { message = "An internal error occurred" });
            }
        }

        [HttpGet("{familyId:int}/members")]
        public async Task<ActionResult<IReadOnlyList<FamilyMemberDto>>> GetFamilyMembers(int familyId)
        {
            try
            {
                if (!TryGetCurrentUserId(out var userId))
                {
                    return Unauthorized(new { message = "Invalid or missing user identity" });
                }

                var outcome = await _familyService.GetFamilyMembersAsync(familyId, userId);
                return outcome.Status switch
                {
                    FamilyMembersStatus.Ok when outcome.Members != null => Ok(outcome.Members),
                    FamilyMembersStatus.NotFound => NotFound(),
                    FamilyMembersStatus.Forbidden => Forbid(),
                    _ => StatusCode(500),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while listing family members");
                return StatusCode(500, new { message = "An internal error occurred" });
            }
        }

        /// <summary>Admin-only. Soft-removes a member; their financial records stay in the ledger.</summary>
        [HttpDelete("{familyId:int}/members/{memberUserId:int}")]
        public async Task<IActionResult> RemoveFamilyMember(int familyId, int memberUserId)
        {
            try
            {
                if (!TryGetCurrentUserId(out var userId))
                {
                    return Unauthorized(new { message = "Invalid or missing user identity" });
                }

                var outcome = await _familyService.RemoveFamilyMemberAsync(familyId, memberUserId, userId);
                return MapMutation(outcome);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while removing family member");
                return StatusCode(500, new { message = "An internal error occurred" });
            }
        }

        /// <summary>Admin-only. Promotes a Member to Admin or demotes an Admin to Member.</summary>
        [HttpPatch("{familyId:int}/members/{memberUserId:int}/role")]
        public async Task<IActionResult> UpdateFamilyMemberRole(
            int familyId,
            int memberUserId,
            [FromBody] UpdateMemberRoleRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Role))
                {
                    return BadRequest(new { message = "Role is required" });
                }

                if (!TryGetCurrentUserId(out var userId))
                {
                    return Unauthorized(new { message = "Invalid or missing user identity" });
                }

                var outcome = await _familyService.UpdateFamilyMemberRoleAsync(familyId, memberUserId, userId, request.Role);
                return MapMutation(outcome);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating family member role");
                return StatusCode(500, new { message = "An internal error occurred" });
            }
        }

        private IActionResult MapMutation(FamilyMemberMutationResult outcome) => outcome.Status switch
        {
            FamilyMemberMutationStatus.Ok =>
                outcome.Member != null ? Ok(outcome.Member) : NoContent(),
            FamilyMemberMutationStatus.NotFound => NotFound(new { message = "Member not found" }),
            FamilyMemberMutationStatus.Forbidden => Forbid(),
            FamilyMemberMutationStatus.CannotTargetSelf =>
                Conflict(new { message = "Admins cannot target themselves with this action" }),
            FamilyMemberMutationStatus.LastAdmin =>
                Conflict(new { message = "Cannot remove or demote the only remaining admin" }),
            FamilyMemberMutationStatus.InvalidRequest =>
                BadRequest(new { message = "Invalid role. Accepted values: Admin, Member" }),
            _ => StatusCode(500, new { message = "An internal error occurred" }),
        };

        [HttpGet("{id:int}")]
        public async Task<ActionResult<FamilyResponse>> GetFamilyById(int id)
        {
            try
            {
                if (!TryGetCurrentUserId(out var userId))
                {
                    return Unauthorized(new { message = "Invalid or missing user identity" });
                }

                var outcome = await _familyService.GetFamilyByIdAsync(id, userId);
                return outcome.Status switch
                {
                    FamilyGetStatus.Ok when outcome.Response != null => Ok(outcome.Response),
                    FamilyGetStatus.NotFound => NotFound(),
                    FamilyGetStatus.Forbidden => Forbid(),
                    _ => StatusCode(500),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching family");
                return StatusCode(500, new { message = "An internal error occurred" });
            }
        }

        /// <summary>JWT may expose subject as ClaimTypes.NameIdentifier and/or <see cref="JwtRegisteredClaimNames.Sub"/>.</summary>
        private bool TryGetCurrentUserId(out int userId)
        {
            var raw =
                User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            return int.TryParse(raw, out userId) && userId > 0;
        }
    }
}
