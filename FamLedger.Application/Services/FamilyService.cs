using AutoMapper;
using FamLedger.Application.DTOs.Request;
using FamLedger.Application.DTOs.Response;
using FamLedger.Application.Interfaces;
using FamLedger.Application.Options;
using FamLedger.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FamLedger.Application.Services
{
    public class FamilyService : IFamilyService
    {
        private readonly IFamilyRepository _familyRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<FamilyService> _logger;
        private readonly IMapper _mapper;
        private readonly CommonOptions _options;
        public FamilyService(
            IFamilyRepository familyRepository,
            IUserRepository userRepository,
            ILogger<FamilyService> logger,
            IMapper mapper,
            IOptions<CommonOptions> options
            )
        {
            _familyRepository = familyRepository;
            _userRepository = userRepository;
            _logger = logger;
            _mapper = mapper;
            _options = options.Value;
        }

        public async Task<FamilyResponse?> CreateFamilyAsync(int userId, string familyName)
        {
            try
            {
                //Check the user exisit:
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return null;
                }

                // 1. Get last family to generate code
                var lastFamily = await _familyRepository.GetLastFamilyAsync();

                string newFamilyCode = GenerateFamilyCode(lastFamily?.FamilyCode);
                string invitationCode = GenerateInvitationCode();

                // 2. Create new Family entity
                var family = new Family
                {
                    FamilyName = familyName,
                    FamilyCode = newFamilyCode,
                    InvitationCode = invitationCode,
                    Status = true,
                    CreatedBy = userId,
                    CreatedOn = DateTime.UtcNow,
                    UpdatedOn = DateTime.UtcNow,
                };

                // 3. Save the family
                await _familyRepository.AddFamilyAsync(family);

                //Update user details
                await _userRepository.UpdateFamilyDetailAsync(userId, family.Id);

                return new FamilyResponse
                {
                    FamilyId = family.Id,
                    FamilyCode = newFamilyCode,
                    InvitationCode = invitationCode,
                    InvitationLink = $"{_options.RootUrl}/invite?code={invitationCode}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in CreateFamilyAsync method for User ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<FamilyResponse?> GetFamilyByIdAsync(int familyId)
        {
            try
            {
                var family = await _familyRepository.GetFamilyByIdAsync(familyId);
                if (family == null) return null;

                return new FamilyResponse
                {
                    FamilyId = family.Id,
                    FamilyCode = family.FamilyCode,
                    InvitationCode = family.InvitationCode,
                    InvitationLink = $"{_options.RootUrl}/invite?code={family.InvitationCode}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetFamilyByIdAsync method for Family ID: {FamilyId}", familyId);
                throw;
            }
        }

        private string GenerateFamilyCode(string? lastCode)
        {
            if (string.IsNullOrEmpty(lastCode))
                return "FAM001";

            // Example: "FAM005" → 5 → 6 → "FAM006"
            int number = int.Parse(lastCode.Substring(3));
            return $"FAM{(number + 1).ToString("D3")}";
        }

        private string GenerateInvitationCode()
        {
            return Guid.NewGuid().ToString("N")[..10].ToUpper(); // 10 char random
        }
    }
}
