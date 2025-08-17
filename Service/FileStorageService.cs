using backend.Interface.Service;


namespace backend.Service
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _http;

        public FileStorageService(IWebHostEnvironment env, IHttpContextAccessor http)
        {
            _env = env;
            _http = http;
        }

        public async Task<string> UploadAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Empty file.");

            // Validate file type
            var allowed = new[] { "image/jpeg", "image/png", "image/webp", "image/jpg" };
            if (!allowed.Contains(file.ContentType.ToLower()))
                throw new ArgumentException("Only JPG/PNG/WEBP images are allowed.");

            // Save under wwwroot/images/{folder}
            var uploadsRoot = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "images", folder);
            Directory.CreateDirectory(uploadsRoot);

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(uploadsRoot, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative path only
            var urlPath = $"/images/{folder}/{fileName}".Replace("\\", "/");
            return urlPath;
        }

        public Task DeleteByUrlAsync(string url)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(url)) return Task.CompletedTask;

                var uri = new Uri(url, UriKind.RelativeOrAbsolute);
                var path = uri.IsAbsoluteUri ? uri.AbsolutePath : uri.OriginalString;

                if (!path.StartsWith("/images/", StringComparison.OrdinalIgnoreCase))
                    return Task.CompletedTask;

                var local = Path.Combine(
                    _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"),
                    path.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
                );

                if (File.Exists(local))
                    File.Delete(local);
            }
            catch
            {
                // Ignore delete errors
            }

            return Task.CompletedTask;
        }
    }
}
