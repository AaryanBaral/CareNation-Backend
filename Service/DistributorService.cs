using backend.Dto;
using backend.Interface.Repository;
using backend.Interface.Service;
using backend.Mapper;
using backend.Models;
using backend.Service.Jwt;
using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace backend.Service
{
    public class DistributorService : IDistributorService
    {
        private readonly IDistributorRepository _distributorRepo;
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _jwtService;
        private readonly UserManager<User> _userManager;

        public DistributorService(
            IDistributorRepository distributorRepo,
            IUserRepository userRepository,
            ITokenService jwtService,
            UserManager<User> userManager)
        {
            _distributorRepo = distributorRepo;
            _userRepository = userRepository;
            _jwtService = jwtService;
            _userManager = userManager;
        }

        public async Task<bool> CanBecomeDistributorAsync(string userId)
        {
            return await _distributorRepo.CanBecomeDistributorAsync(userId);
        }

        public async Task<DistributorReadDto?> GetDistributorByIdAsync(string id)
        {
            var user = await _distributorRepo.GetDistributorByIdAsync(id);
            if (user == null)
                return null;

            return user.ToDistributorReadDto("Distributor");
        }

        public async Task<List<DownlineUserDto>> GetDownlineAsync(string userId)
        {
            var users = await _distributorRepo.GetDownlineAsync(userId);

            return [.. users.Select(u => new DownlineUserDto
            {
                Id = u.Id,
                // Build FullName from split parts
                FullName = BuildFullName(u.FirstName, u.MiddleName, u.LastName),
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                ParentId = u.ParentId,
                Position = u.Position?.ToString() ?? string.Empty,
                LeftWallet = u.LeftWallet,
                RightWallet = u.RightWallet,
                Rank = u.Rank.ToString()
            })];
        }

        public async Task<List<DistributorReadDto>> GetAllDistributorsAsync()
        {
            var users = await _distributorRepo.GetAllDistributorsAsync();
            return users.Select(u => u.ToDistributorReadDto("Distributor")).ToList();
        }

        public async Task<bool> SignUpDistributorAsync(
            string userId,
            DistributorSignUpDto dto,
            IFormFile citizenshipFile,
            IFormFile? profilePicture = null)
        {
            if (!await CanBecomeDistributorAsync(userId))
                throw new ArgumentException("User has not purchased goods worth more than 5000.");

            _ = await _distributorRepo.GetDistributorByIdAsync(dto.ReferalId)
                ?? throw new KeyNotFoundException("Referal Id invalid");

            var user = await _userRepository.GetUserById(userId)
                ?? throw new KeyNotFoundException("User not found.");

            user = dto.ToUser(user);

            return await _distributorRepo.SignUpDistributorAsync(user);
        }

        public async Task ChangeParentAsync(string userId, string newParentId, string childId)
        {
            _ = await _distributorRepo.GetDistributorByIdAsync(userId)
                ?? throw new KeyNotFoundException("User not found.");

            var child = await _distributorRepo.GetDistributorByIdAsync(childId)
                ?? throw new KeyNotFoundException("Child not found.");

            var newParent = await _distributorRepo.GetDistributorByIdAsync(newParentId)
                ?? throw new KeyNotFoundException("New parent not found.");

            if (newParent.Id == userId)
                throw new ArgumentException("You cannot set yourself as your own parent.");

            await _distributorRepo.ChangeParentAsync(childId, newParentId);
        }

        public async Task<bool> UpdateDistributorAsync(string id, DistributorSignUpDto dto)
        {
            var user = await _distributorRepo.GetDistributorByIdAsync(id);
            if (user == null) return false;

            user = dto.ToUser(user);
            return await _distributorRepo.UpdateDistributorAsync(user);
        }

        public async Task<bool> DeleteDistributorAsync(string id)
        {
            return await _distributorRepo.DeleteDistributorAsync(id);
        }

        public async Task<DistributorLoginResponse> LoginDistributorAsync(DistributorLoginDto dto)
        {
            var user = await _distributorRepo.LoginDistributorAsync(dto.Email, dto.Password)
                ?? throw new InvalidOperationException("Invalid Credentials");

            var distributortor = await _distributorRepo.GetDistributorByIdAsync(user.Id);
            Console.WriteLine(distributortor);

            var token = _jwtService.CreateAccessToken(user);
            return new DistributorLoginResponse()
            {
                Token = token.AccessToken,
                IsDistributor = distributortor != null
            };
        }

        public async Task<int> GetTotalDownlineAsync(string userId)
        {
            var downlines = await _distributorRepo.GetMyDownlineAsync(userId);
            return downlines?.Count ?? 0;
            }

        public async Task<List<UserReadDto>> GetPeoplesBelowMe(string userId)
        {
            var downlines = await _distributorRepo.GetMyDownlineAsync(userId);
            return [.. downlines.Select(d => d.ToReadDto("distributor"))];
        }

        public async Task<int> GetTotalReferralsAsync(string userId)
        {
            return await _distributorRepo.GetReferralCountAsync(userId);
        }

        public Task<DistributorTreeDto?> GetUserTreeAsync(string userId)
        {
            return _distributorRepo.GetUserTreeAsync(userId);
        }

        public async Task<List<DistributorReadDto>> GetPeopleIReferredAsync(string myUserId)
        {
            var users = await _distributorRepo.GetPeopleIReferredAsync(myUserId);
            var dtoList = new List<DistributorReadDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? string.Empty;
                dtoList.Add(user.ToDistributorReadDto(role));
            }

            return dtoList;
        }

        public async Task<List<DistributorReadDto>> GetMyUplineAsync(string myUserId)
        {
            var uplines = await _distributorRepo.GetMyUplineAsync(myUserId);
            var dtoList = new List<DistributorReadDto>();

            foreach (var user in uplines)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? string.Empty;
                dtoList.Add(user.ToDistributorReadDto(role));
            }

            return dtoList;
        }

        public async Task<WalletStatementDto> GetWalletStatementAsync(string userId)
        {
            var statement = await _distributorRepo.GetWalletStatementAsync(userId)
                ?? throw new KeyNotFoundException("Wallet statement not found for user.");
            return statement;
        }

        // --------- helpers ---------
        private static string BuildFullName(string? first, string? middle, string? last)
        {
            var parts = new[] { first, middle, last }
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s!.Trim());
            return string.Join(" ", parts);
        }
    }
}
 