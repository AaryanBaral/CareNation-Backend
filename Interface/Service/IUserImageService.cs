
namespace backend.Interface.Service
{
    public interface IUserImageService
    {
        Task<string> UploadProfilePictureAsync(IFormFile image, string userId);
        Task<string> UploadCitizenshipImageAsync(IFormFile image, string userId);
        Task DeleteProfilePictureAsync(string userId);
        Task DeleteCitizenshipImageAsync(string userId);
    }
}
