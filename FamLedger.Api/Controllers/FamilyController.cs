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
