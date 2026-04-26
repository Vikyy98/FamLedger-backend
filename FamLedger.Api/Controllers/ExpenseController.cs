using FamLedger.Application.DTOs.Request;
using FamLedger.Application.DTOs.Response;
using FamLedger.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamLedger.Api.Controllers
{
    [Authorize]
    [Route("api/expenses")]
    [ApiController]
    public class ExpenseController : ControllerBase
    {
        private readonly IExpenseService _expenseService;
        private readonly ILogger<ExpenseController> _logger;

        public ExpenseController(
            IExpenseService expenseService,
            ILogger<ExpenseController> logger)
        {
            _expenseService = expenseService;
            _logger = logger;
        }

        [HttpGet("/api/families/{familyId}/expenses")]
        public async Task<IActionResult> ExpenseDetails(int familyId)
        {
            try
            {
                var details = await _expenseService.GetExpenseDetailsAsync(familyId);
                return Ok(details ?? new ExpenseResponseDto { FamilyId = familyId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in ExpenseDetails method");
                return StatusCode(500, "An error occurred while retrieving expense details");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddExpense([FromBody] ExpenseRequestDto expenseRequest)
        {
            try
            {
                if (expenseRequest == null || !ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var outcome = await _expenseService.AddExpenseAsync(expenseRequest);
                return outcome.Status switch
                {
                    AddExpenseStatus.Ok when outcome.Response != null =>
                        CreatedAtRoute("GetExpenseByIdRoute", new
                        {
                            familyId = outcome.Response.FamilyId,
                            expenseId = outcome.Response.Id,
                            type = (int)outcome.Response.Type,
                        }, outcome.Response),
                    AddExpenseStatus.Duplicate => Conflict("Duplicate expense entry detected"),
                    AddExpenseStatus.InvalidRequest => BadRequest("Invalid expense data"),
                    AddExpenseStatus.Forbidden => Forbid(),
                    AddExpenseStatus.PersistenceFailed => StatusCode(500, "Failed to add expense"),
                    _ => StatusCode(500, "Failed to add expense"),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in AddExpense method");
                return StatusCode(500, "An error occurred while adding expense");
            }
        }

        [HttpGet("/api/families/{familyId}/expenses/{expenseId}/{type}", Name = "GetExpenseByIdRoute")]
        public async Task<IActionResult> GetExpenseById(int familyId, int expenseId, int type)
        {
            try
            {
                var outcome = await _expenseService.GetExpenseByIdAsync(expenseId, type, familyId);
                return outcome.Status switch
                {
                    GetExpenseByIdStatus.Ok when outcome.Response != null => Ok(outcome.Response),
                    GetExpenseByIdStatus.NotFound => NotFound(),
                    GetExpenseByIdStatus.Forbidden => Forbid(),
                    _ => NotFound(),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetExpenseById method");
                return StatusCode(500, "An error occurred while retrieving expense");
            }
        }

        [HttpPut("/api/families/{familyId}/expenses/{expenseId}/{type}")]
        public async Task<IActionResult> UpdateExpense(int familyId, int expenseId, int type, [FromBody] ExpenseRequestDto expenseRequest)
        {
            try
            {
                if (expenseRequest == null || !ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var outcome = await _expenseService.UpdateExpenseAsync(expenseId, type, familyId, expenseRequest);
                return outcome.Status switch
                {
                    UpdateExpenseStatus.Ok when outcome.Response != null => Ok(outcome.Response),
                    UpdateExpenseStatus.InvalidRequest => BadRequest("Invalid expense data. Type and frequency cannot be changed."),
                    UpdateExpenseStatus.Forbidden => Forbid(),
                    UpdateExpenseStatus.NotFound => NotFound(),
                    UpdateExpenseStatus.PersistenceFailed => StatusCode(500, "Failed to update expense"),
                    _ => StatusCode(500, "Failed to update expense"),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in UpdateExpense method");
                return StatusCode(500, "An error occurred while updating expense");
            }
        }

        [HttpDelete("/api/families/{familyId}/expenses/{expenseId}/{type}")]
        public async Task<IActionResult> DeleteExpense(int familyId, int expenseId, int type)
        {
            try
            {
                var outcome = await _expenseService.DeleteExpenseAsync(expenseId, type, familyId);
                return outcome.Status switch
                {
                    DeleteExpenseStatus.Ok => NoContent(),
                    DeleteExpenseStatus.Forbidden => Forbid(),
                    DeleteExpenseStatus.NotFound => NotFound(),
                    DeleteExpenseStatus.PersistenceFailed => StatusCode(500, "Failed to delete expense"),
                    _ => StatusCode(500, "Failed to delete expense"),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in DeleteExpense method");
                return StatusCode(500, "An error occurred while deleting expense");
            }
        }

        [HttpGet("categories")]
        [AllowAnonymous]
        public IActionResult GetCategories()
        {
            try
            {
                return Ok(_expenseService.GetCategories());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetCategories method");
                return StatusCode(500, "An error occurred while retrieving categories");
            }
        }
    }
}
