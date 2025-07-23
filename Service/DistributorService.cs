using backend.Dto;
using backend.Interface.Repository;
using backend.Interface.Service;
using backend.Mapper;
using backend.Models;
using backend.Service.Jwt;
using Microsoft.AspNetCore.Identity;

namespace backend.Service
{
    public class DistributorService : IDistributorService
    {
        private readonly IDistributorRepository _distributorRepo;
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly UserManager<User> _userManager;

        public DistributorService(IDistributorRepository distributorRepo, IUserRepository userRepository, IJwtService jwtService, UserManager<User> userManager)
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

        public async Task<List<DistributorReadDto>> GetAllDistributorsAsync()
        {
            var users = await _distributorRepo.GetAllDistributorsAsync();
            return users.Select(u => u.ToDistributorReadDto("Distributor")).ToList();
        }

        public async Task<bool> SignUpDistributorAsync(string userId, DistributorSignUpDto dto)
        {
            // if (!await CanBecomeDistributorAsync(userId))
            // throw new ArgumentException("User has not purchased goods worth more than 5000.");

            // _ = await _distributorRepo.GetDistributorByIdAsync(dto.ReferalId) ?? throw new KeyNotFoundException("Referal Id invalid");

            var user = await _userRepository.GetUserById(userId) ?? throw new KeyNotFoundException("User not found.");
            user = dto.ToUser(user);

            return await _distributorRepo.SignUpDistributorAsync(user);
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
            var user = await _distributorRepo.LoginDistributorAsync(dto.Email, dto.Password) ?? throw new InvalidOperationException("Invalid Credentials");
            var distributortor = await _distributorRepo.GetDistributorByIdAsync(user.Id);
            Console.WriteLine(distributortor);
            var token = _jwtService.GenerateJwtToken(user);
            return new DistributorLoginResponse()
            {
                Token = token,
                IsDistributor = distributortor != null
            };
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

        // 2. Get my upline as DTOs
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
    }
}
