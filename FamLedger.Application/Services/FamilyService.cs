using AutoMapper;
using FamLedger.Application.DTOs.Request;
using FamLedger.Application.DTOs.Response;
using FamLedger.Application.Interfaces;
using FamLedger.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FamLedger.Application.Services
{
    public class FamilyService : IFamilyService
    {
        private readonly IFamilyRepository _familyRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<FamilyService> _logger;
        private readonly IMapper _mapper;
        public FamilyService(
            IFamilyRepository familyRepository,
            IUserRepository userRepository,
            ILogger<FamilyService> logger,
            IMapper mapper
            )
        {
            _familyRepository = familyRepository;
            _userRepository = userRepository;
            _logger = logger;
            _mapper = mapper;
        }


        public async Task<CreateFamilyResponse> CreateFamilyAsync(int userId, string familyName)
        {
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

            Console.WriteLine(family.FamilyId);

            // 4. Add user as a family member (Owner)
            //await _userRepository.AddFamilyMemberAsync(new FamilyMember
            //{
            //    FamilyId = family.FamilyId,
            //    UserId = userId,
            //    Role = "Owner",
            //    JoinedOn = DateTime.UtcNow
            //});

            return new CreateFamilyResponse
            {
                FamilyId = family.FamilyId,
                FamilyCode = newFamilyCode,
                InvitationCode = invitationCode,
                InvitationLink = $"https://famledger.com/invite?code={invitationCode}"
            };
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
