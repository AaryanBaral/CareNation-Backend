using backend.Repository;

namespace backend.Dto
{
    public class AdminCreateDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // Optional: specify exact admin sub-role at creation (default to "Admin" or "SuperAdmin")
        public string Role { get; set; } = AdminRoles.Admin;
    }

    public class AdminLoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
        public class AdminReadDto
    {
        public string Id { get; set; }

        public string FirstName  { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string LastName   { get; set; } = string.Empty;

        public string Email      { get; set; } = string.Empty;

        // Admin role (Admin, SuperAdmin, etc.)
        public string Role { get; set; } = AdminRoles.Admin;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
