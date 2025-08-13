namespace backend.Dto
{
    public class UserSignUpDto
    {
        // Name details
        public required string FirstName { get; set; }
        public string? MiddleName { get; set; }
        public required string LastName { get; set; }

        // Contact & Login
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string PhoneNumber { get; set; }

        // Permanent Address (replaces Address)
        public required string PermanentFullAddress { get; set; }

        public string Role { get; set; } = "user";
    }

    public class UserLoginDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    public class UserReadDto
    {
        public required string Id { get; set; }

        // Name details
        public required string FirstName { get; set; }
        public string? MiddleName { get; set; }
        public required string LastName { get; set; }

        // Contact & Role
        public required string Email { get; set; }
        public required string Role { get; set; }
        public required string PhoneNumber { get; set; }

        // Permanent Address (replaces Address)
        public required string PermanentFullAddress { get; set; }
    }
}
