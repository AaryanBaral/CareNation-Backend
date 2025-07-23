namespace backend.Dto
{
    public class UserSignUpDto
    {
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string Address { get; set; }
        public required string PhoneNumber{ get; set; }
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
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; }
        public required string Address { get; set; }
        public required string PhoneNumber{ get; set; }
    }
}
