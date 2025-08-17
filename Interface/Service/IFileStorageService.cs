

namespace backend.Interface.Service
{
    public interface IFileStorageService
    {
        /// <summary>Uploads a file and returns a public URL.</summary>
        Task<string> UploadAsync(IFormFile file, string folder);
        /// <summary>Optionally delete by URL (not used here but handy).</summary>
        Task DeleteByUrlAsync(string url);

    }
}
