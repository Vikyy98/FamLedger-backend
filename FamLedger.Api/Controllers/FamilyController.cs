using AutoMapper;
using FamLedger.Application.DTOs.Request;
using FamLedger.Application.DTOs.Response;
using FamLedger.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FamLedger.Api.Controllers
{
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

                var familyResponse = await _familyService.CreateFamilyAsync(familyRequest.UserId, familyRequest.FamilyName);
                if (familyResponse == null)
                {
                    return NotFound("User not found");
                }

                // Return 201 Created with Location header
                return Created($"/api/families/{familyResponse.FamilyId}", familyResponse);
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
                var family = await _familyService.GetFamilyByIdAsync(id);
                if (family == null) return NotFound();
                return Ok(family);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching family");
                return StatusCode(500, new { message = "An internal error occurred" });
            }
        }
    }
}
