namespace backend.Dto
{
    public class DistributorSignUpDto
    {
        public required string DOB { get; set; }
        public required string CitizenshipNo { get; set; }
        public required string ReferalId { get; set; }
        public required string AccountName { get; set; }
        public required string AccountNumber { get; set; }
        public required string BankName { get; set; }

        public required string ParentId { get; set; }
    }


    public class DistributorLoginDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    public class DistributorReadDto
    {
        public required string Id { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; }
        public required string Address { get; set; }
        public required string PhoneNumber { get; set; }
        public required string DOB { get; set; }
        public required string CitizenshipNo { get; set; }
        public required string ReferalId { get; set; }
        public required string AccountName { get; set; }
        public required string AccountNumber { get; set; }
        public required string BankName { get; set; }
        public required string ParentId { get; set; }
        public string? Position { get; set; } 
    }
    public class DistributorLoginResponse
    {
        public required string Token { get; set; }
        public required bool IsDistributor { get; set; }

    }
    public class DistributorTreeDto
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Position { get; set; }
    public List<DistributorTreeDto> Children { get; set; } = new();
}

}
