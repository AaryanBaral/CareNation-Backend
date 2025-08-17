using backend.Dto;
using backend.Interface.Repository;
using backend.Interface.Service;
using backend.Mapper;
using backend.Models;
using backend.Service.Jwt;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace backend.Service
{
    public class DistributorService : IDistributorService
    {
        private readonly IDistributorRepository _distributorRepo;
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _jwtService;
        private readonly UserManager<User> _userManager;
            private readonly IFileStorageService _fileStorage;

        public DistributorService(
            IDistributorRepository distributorRepo,
            IUserRepository userRepository,
            ITokenService jwtService,
            UserManager<User> userManager,
            IFileStorageService fileStorage)
        {
            _distributorRepo = distributorRepo;
            _userRepository = userRepository;
            _jwtService = jwtService;
            _userManager = userManager;
            _fileStorage = fileStorage;
        }

        private static string BuildFullName(User u)
        {
            var parts = new[] { u.FirstName, u.MiddleName, u.LastName }
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s!.Trim());
            var name = string.Join(" ", parts);
            if (!string.IsNullOrWhiteSpace(name)) return name;
            return u.UserName ?? u.Email ?? "User";
        }

        public async Task<bool> CanBecomeDistributorAsync(string userId)
        {
            return await _distributorRepo.CanBecomeDistributorAsync(userId);
        }

        public async Task<DistributorReadDto?> GetDistributorByIdAsync(string id)
        {
            var user = await _distributorRepo.GetDistributorByIdAsync(id);
            if (user == null) return null;
            return user.ToDistributorReadDto("Distributor");
        }

        public async Task<List<DownlineUserDto>> GetDownlineAsync(string userId)
        {
            var users = await _distributorRepo.GetDownlineAsync(userId);

            return [.. users.Select(u => new DownlineUserDto
            {
                Id = u.Id,
                // was: FullName = u.FullName,
                FullName = BuildFullName(u),
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                ParentId = u.ParentId,
                Position = u.Position.ToString() ?? "",
                LeftWallet = u.LeftWallet,
                RightWallet = u.RightWallet,
                // keep Rank as-is if your model has it; otherwise switch to Type
                Rank = u.Rank.ToString()!
                // If your model uses Type instead of Rank, use:
                // Rank = u.Type.ToString()
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

        // Map the rest of distributor fields
        user = dto.ToUser(user);

        // Upload files → set URLs on user before saving
        string? citizenUrl = null;
        string? profileUrl = null;

        try
        {
            if (citizenshipFile == null || citizenshipFile.Length == 0)
                throw new ArgumentException("Citizenship image is required.");

            citizenUrl = await _fileStorage.UploadAsync(citizenshipFile, $"citizenship/{user.Id ?? userId}");
            user.CitizenshipImageUrl = citizenUrl;

            if (profilePicture is { Length: > 0 })
            {
                profileUrl = await _fileStorage.UploadAsync(profilePicture, $"profile/{user.Id ?? userId}");
                user.ProfilePictureUrl = profileUrl;
            }

            // Persist everything (including the image URLs)
            var ok = await _distributorRepo.SignUpDistributorAsync(user);
            if (!ok)
            {
                // rollback uploaded files if save fails
                if (!string.IsNullOrWhiteSpace(profileUrl)) await _fileStorage.DeleteByUrlAsync(profileUrl);
                if (!string.IsNullOrWhiteSpace(citizenUrl)) await _fileStorage.DeleteByUrlAsync(citizenUrl);
            }
            return ok;
        }
        catch
        {
            // rollback on any exception too
            if (!string.IsNullOrWhiteSpace(profileUrl)) await _fileStorage.DeleteByUrlAsync(profileUrl);
            if (!string.IsNullOrWhiteSpace(citizenUrl)) await _fileStorage.DeleteByUrlAsync(citizenUrl);
            throw;
        }
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

            var distributor = await _distributorRepo.GetDistributorByIdAsync(user.Id);
            var token = await _jwtService.CreateAccessToken(user);

            return new DistributorLoginResponse
            {
                Token = token.AccessToken,
                IsDistributor = distributor != null
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

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            return await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        // Persist via UserManager (no DbContext in this service)
        public async Task UpdateProfilePictureUrlAsync(string userId, string? imageUrl)
        {
            var user = await GetUserByIdAsync(userId) ?? throw new KeyNotFoundException("User not found.");
            user.ProfilePictureUrl = imageUrl;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
                throw new InvalidOperationException($"Failed to update profile picture URL. {errors}");
            }
        }

        public async Task UpdateCitizenshipImageUrlAsync(string userId, string? imageUrl)
        {
            var user = await GetUserByIdAsync(userId) ?? throw new KeyNotFoundException("User not found.");
            user.CitizenshipImageUrl = imageUrl;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
                throw new InvalidOperationException($"Failed to update citizenship image URL. {errors}");
            }
        }

        public async Task<WalletStatementDto> GetWalletStatementAsync(string userId)
        {
            var statement = await _distributorRepo.GetWalletStatementAsync(userId)
                ?? throw new KeyNotFoundException("Wallet statement not found for user.");
            return statement;
        }
    }
}
