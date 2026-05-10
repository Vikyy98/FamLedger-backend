using FamLedger.Application.DTOs.Request;
using FamLedger.Application.DTOs.Response;
using FamLedger.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamLedger.Api.Controllers
{
    [Authorize]
    [Route("api/debts")]
    [ApiController]
    public class DebtController : ControllerBase
    {
        private readonly IDebtService _debtService;
        private readonly ILogger<DebtController> _logger;

        public DebtController(
            IDebtService debtService,
            ILogger<DebtController> logger)
        {
            _debtService = debtService;
            _logger = logger;
        }

        [HttpGet("/api/families/{familyId}/debts")]
        public async Task<IActionResult> DebtDetails(int familyId)
        {
            try
            {
                var details = await _debtService.GetDebtDetailsAsync(familyId);
                return Ok(details ?? new DebtResponseDto { FamilyId = familyId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in DebtDetails method");
                return StatusCode(500, "An error occurred while retrieving debt details");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddDebt([FromBody] DebtRequestDto request)
        {
            try
            {
                if (request == null || !ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var outcome = await _debtService.AddDebtAsync(request);
                return outcome.Status switch
                {
                    AddDebtStatus.Ok when outcome.Response != null =>
                        CreatedAtRoute("GetDebtByIdRoute", new
                        {
                            familyId = outcome.Response.FamilyId,
                            debtId = outcome.Response.Id,
                        }, outcome.Response),
                    AddDebtStatus.Duplicate => Conflict("Duplicate debt entry detected"),
                    AddDebtStatus.InvalidRequest => BadRequest("Invalid debt data"),
                    AddDebtStatus.Forbidden => Forbid(),
                    AddDebtStatus.PersistenceFailed => StatusCode(500, "Failed to add debt"),
                    _ => StatusCode(500, "Failed to add debt"),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in AddDebt method");
                return StatusCode(500, "An error occurred while adding debt");
            }
        }

        [HttpGet("/api/families/{familyId}/debts/{debtId}", Name = "GetDebtByIdRoute")]
        public async Task<IActionResult> GetDebtById(int familyId, int debtId)
        {
            try
            {
                var outcome = await _debtService.GetDebtByIdAsync(debtId, familyId);
                return outcome.Status switch
                {
                    GetDebtByIdStatus.Ok when outcome.Response != null => Ok(outcome.Response),
                    GetDebtByIdStatus.NotFound => NotFound(),
                    GetDebtByIdStatus.Forbidden => Forbid(),
                    _ => NotFound(),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetDebtById method");
                return StatusCode(500, "An error occurred while retrieving debt");
            }
        }

        [HttpPut("/api/families/{familyId}/debts/{debtId}")]
        public async Task<IActionResult> UpdateDebt(int familyId, int debtId, [FromBody] DebtRequestDto request)
        {
            try
            {
                if (request == null || !ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var outcome = await _debtService.UpdateDebtAsync(debtId, familyId, request);
                return outcome.Status switch
                {
                    UpdateDebtStatus.Ok when outcome.Response != null => Ok(outcome.Response),
                    UpdateDebtStatus.InvalidRequest => BadRequest("Invalid debt data"),
                    UpdateDebtStatus.Forbidden => Forbid(),
                    UpdateDebtStatus.NotFound => NotFound(),
                    UpdateDebtStatus.PersistenceFailed => StatusCode(500, "Failed to update debt"),
                    _ => StatusCode(500, "Failed to update debt"),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in UpdateDebt method");
                return StatusCode(500, "An error occurred while updating debt");
            }
        }

        [HttpDelete("/api/families/{familyId}/debts/{debtId}")]
        public async Task<IActionResult> DeleteDebt(int familyId, int debtId)
        {
            try
            {
                var outcome = await _debtService.DeleteDebtAsync(debtId, familyId);
                return outcome.Status switch
                {
                    DeleteDebtStatus.Ok => NoContent(),
                    DeleteDebtStatus.Forbidden => Forbid(),
                    DeleteDebtStatus.NotFound => NotFound(),
                    DeleteDebtStatus.PersistenceFailed => StatusCode(500, "Failed to delete debt"),
                    _ => StatusCode(500, "Failed to delete debt"),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in DeleteDebt method");
                return StatusCode(500, "An error occurred while deleting debt");
            }
        }

        [HttpGet("categories")]
        [AllowAnonymous]
        public IActionResult GetCategories()
        {
            try
            {
                return Ok(_debtService.GetCategories());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetCategories method");
                return StatusCode(500, "An error occurred while retrieving categories");
            }
        }
    }
}
