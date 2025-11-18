using FamLedger.Application.DTOs.Response;
using FamLedger.Application.Interfaces;
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


        //Get income details by family id

        //Get income details of family by year

        //Post income details

        //Update income details of a memebr in family

        //Delete income details of a memeber in famoly
    }
}
