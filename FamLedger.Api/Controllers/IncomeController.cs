using FamLedger.Application.DTOs.Request;
using FamLedger.Application.DTOs.Response;
using FamLedger.Application.Interfaces;
using FamLedger.Application.Utilities;
using FamLedger.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

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

        [HttpGet("categories")]
        public IActionResult GetIncomeCategories()
        {
            try
            {
                var categories = Enum.GetValues(typeof(IncomeCategory))
                    .Cast<IncomeCategory>()
                    .Select(category => new IncomeCategoryDto
                    {
                        CategoryId = (int)category,
                        CategoryName = category.GetDescription()
                    })
                    .ToList();

                var response = new IncomeCategoriesResponseDto
                {
                    Categories = categories
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetIncomeCategories method");
                return StatusCode(500, "An error occurred while retrieving income categories");
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


        //Get income details of family by year

        //Post income details

        //Update income details of a memebr in family

        //Delete income details of a memeber in famoly
    }
}
