using backend.Interface.Repository;
using backend.Interface.Service;
using backend.Models;
using Microsoft.AspNetCore.Http;

namespace backend.Service
{
    public class UserImageService : IUserImageService
    {
        private readonly IDistributorRepository _distributorRepository;

        public UserImageService(IDistributorRepository userRepository)
        {
            _distributorRepository = userRepository;
        } 

        public async Task<string> UploadProfilePictureAsync(IFormFile image, string userId)
        {
            if (image == null) throw new ArgumentNullException("No image provided");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profile");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            var imageUrl = $"/images/profile/{fileName}";
            await _distributorRepository.UpdateProfilePictureUrlAsync(userId, imageUrl);
            return imageUrl;
        }

        public async Task<string> UploadCitizenshipImageAsync(IFormFile image, string userId)
        {
            if (image == null) throw new ArgumentNullException("No image provided");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/citizenship");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            var imageUrl = $"/images/citizenship/{fileName}";
            await _distributorRepository.UpdateCitizenshipImageUrlAsync(userId, imageUrl);
            return imageUrl;
        }

        public async Task DeleteProfilePictureAsync(string userId)
        {
            var user = await _distributorRepository.GetDistributorByIdAsync(userId);
            if (!string.IsNullOrEmpty(user?.ProfilePictureUrl))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePictureUrl.TrimStart('/', '\\'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                await _distributorRepository.UpdateProfilePictureUrlAsync(userId, null);
            }
        }

        public async Task DeleteCitizenshipImageAsync(string userId)
        {
            var user = await _distributorRepository.GetDistributorByIdAsync(userId);
            if (!string.IsNullOrEmpty(user?.CitizenshipImageUrl))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.CitizenshipImageUrl.TrimStart('/', '\\'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                await _distributorRepository.UpdateCitizenshipImageUrlAsync(userId, null);
            }
        }
    }
}
