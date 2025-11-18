using AutoMapper;
using FamLedger.Application.DTOs.Request;
using FamLedger.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FamLedger.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FamilyController : ControllerBase
    {
        private readonly ILogger<FamilyController> _logger;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IFamilyService _familyService;

        public FamilyController(ILogger<FamilyController> logger, IMapper mapper, IUserService userService, IFamilyService familyService)
        {
            _logger = logger;
            _mapper = mapper;
            _userService = userService;
            _familyService = familyService;
        }

        [HttpPost("family")]
        public async Task<IActionResult> CreateFamily([FromBody] CreateFamilyRequest familyRequest)
        {
            try
            {
                var familyResponse = await _familyService.CreateFamilyAsync(familyRequest.UserId, familyRequest.FamilyName);


                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}
