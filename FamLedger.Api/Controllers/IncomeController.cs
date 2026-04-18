using FamLedger.Application.DTOs.Request;
using FamLedger.Application.DTOs.Response;
using FamLedger.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamLedger.Api.Controllers
{
    [Authorize]
    [Route("api/income")]
    [ApiController]
    public class IncomeController : ControllerBase
    {

        private readonly IIncomeService _incomeService;
        private readonly ILogger<IncomeController> _logger;

        public IncomeController(
            IIncomeService incomeService,
            ILogger<IncomeController> logger)
        {
            _incomeService = incomeService;
            _logger = logger;
        }

        [HttpGet("/api/families/{familyId}/incomes")]
        public async Task<IActionResult> IncomeDetails(int familyId)
        {
            try
            {
                var incomeDetails = await _incomeService.GetIncomeDetailsAsync(familyId);
                return Ok(incomeDetails ?? new IncomeResponseDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in IncomeDetails method");
                return StatusCode(500, "An error occurred while retrieving income details");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddIncome([FromBody] IncomeRequestDto incomeRequest)
        {
            try
            {
                if (incomeRequest == null || !ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var outcome = await _incomeService.AddIncomeAsync(incomeRequest);
                return outcome.Status switch
                {
                    AddIncomeStatus.Ok when outcome.Response != null =>
                        CreatedAtRoute("GetIncomeByIdRoute", new
                        {
                            familyId = outcome.Response.FamilyId,
                            incomeId = outcome.Response.Id,
                            type = (int)outcome.Response.Type
                        }, outcome.Response),
                    AddIncomeStatus.Duplicate => Conflict("Duplicate income entry detected"),
                    AddIncomeStatus.InvalidRequest => BadRequest("Invalid income data"),
                    AddIncomeStatus.Forbidden => Forbid(),
                    AddIncomeStatus.PersistenceFailed => StatusCode(500, "Failed to add income"),
                    _ => StatusCode(500, "Failed to add income"),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in AddIncome method");
                return StatusCode(500, "An error occurred while adding income");
            }
        }

        [HttpGet("/api/families/{familyId}/incomes/{incomeId}/{type}", Name = "GetIncomeByIdRoute")]
        public async Task<IActionResult> GetIncomeById(int familyId, int incomeId, int type)
        {
            try
            {
                var outcome = await _incomeService.GetIncomeByIdAsync(incomeId, type, familyId);
                return outcome.Status switch
                {
                    GetIncomeByIdStatus.Ok when outcome.Response != null => Ok(outcome.Response),
                    GetIncomeByIdStatus.NotFound => NotFound(),
                    GetIncomeByIdStatus.Forbidden => Forbid(),
                    _ => NotFound(),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetIncomeById method");
                return StatusCode(500, "An error occurred while retrieving income");
            }
        }

        [HttpPut("/api/families/{familyId}/incomes/{incomeId}/{type}")]
        public async Task<IActionResult> UpdateIncome(int familyId, int incomeId, int type, [FromBody] IncomeRequestDto incomeRequest)
        {
            try
            {
                if (incomeRequest == null || !ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var outcome = await _incomeService.UpdateIncomeAsync(incomeId, type, familyId, incomeRequest);
                return outcome.Status switch
                {
                    UpdateIncomeStatus.Ok when outcome.Response != null => Ok(outcome.Response),
                    UpdateIncomeStatus.InvalidRequest => BadRequest("Income type cannot be changed. Edit source, amount, and date only."),
                    UpdateIncomeStatus.Forbidden => Forbid(),
                    UpdateIncomeStatus.NotFound => NotFound(),
                    UpdateIncomeStatus.PersistenceFailed => StatusCode(500, "Failed to update income"),
                    _ => StatusCode(500, "Failed to update income"),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in UpdateIncome method");
                return StatusCode(500, "An error occurred while updating income");
            }
        }

        [HttpDelete("/api/families/{familyId}/incomes/{incomeId}/{type}")]
        public async Task<IActionResult> DeleteIncome(int familyId, int incomeId, int type)
        {
            try
            {
                var outcome = await _incomeService.DeleteIncomeAsync(incomeId, type, familyId);
                return outcome.Status switch
                {
                    DeleteIncomeStatus.Ok => NoContent(),
                    DeleteIncomeStatus.Forbidden => Forbid(),
                    DeleteIncomeStatus.NotFound => NotFound(),
                    DeleteIncomeStatus.PersistenceFailed => StatusCode(500, "Failed to delete income"),
                    _ => StatusCode(500, "Failed to delete income"),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in DeleteIncome method");
                return StatusCode(500, "An error occurred while deleting income");
            }
        }

    }
}
