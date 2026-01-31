using FamLedger.Application.DTOs.Request;
using FamLedger.Application.DTOs.Response;
using FamLedger.Application.Interfaces;
using FamLedger.Application.Utilities;
using FamLedger.Domain.Entities;
using FamLedger.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FamLedger.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IncomeController : ControllerBase
    {

        private readonly IIncomeService _incomeService;
        private readonly ILogger<IncomeController> _logger;

        public IncomeController(IIncomeService incomeService, ILogger<IncomeController> logger)
        {
            _incomeService = incomeService;
            _logger = logger;
        }

        [HttpGet("/api/families/{familyId}/incomes")]
        public async Task<IActionResult> IncomeDetails(int familyId)
        {
            try
            {
                var incomeDetails = await _incomeService.GetIncomeDetails(familyId);
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

                var incomeResponse = await _incomeService.AddIncomeAsync(incomeRequest);
                if (incomeResponse == null)
                {
                    return StatusCode(500, "Failed to add income");
                }

                return CreatedAtAction(nameof(GetIncomeById), new { familyId = incomeResponse.FamilyId, incomeId = incomeResponse.Id }, incomeResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in AddIncome method");
                return StatusCode(500, "An error occurred while adding income");
            }
        }

        [HttpGet("/api/families/{familyId}/incomes/{incomeId}")]
        public async Task<IActionResult> GetIncomeById(int familyId, int incomeId)
        {
            try
            {
                var income = await _incomeService.GetIncomeByIdAsync(incomeId);
                if (income == null) return NotFound();
                if (income.FamilyId != familyId) return NotFound();
                return Ok(income);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetIncomeById method");
                return StatusCode(500, "An error occurred while retrieving income");
            }
        }


        //Get income details by family id

        //Get income details of family by year

        //Post income details

        //Update income details of a memebr in family

        //Delete income details of a memeber in famoly
    }
}
