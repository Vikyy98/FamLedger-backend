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

        [HttpGet("details/{familyId}")]
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
                return NotFound(new IncomeResponseDto());
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


        //Get income details by family id

        //Get income details of family by year

        //Post income details

        //Update income details of a memebr in family

        //Delete income details of a memeber in famoly
    }
}
