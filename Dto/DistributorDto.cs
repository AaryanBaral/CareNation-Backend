namespace backend.Dto
{
    public class DistributorSignUpDto
    {
        public required string DOB { get; set; }
        public required string CitizenshipNo { get; set; }
        public string? CitizenshipImageUrl { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public required string ReferalId { get; set; } // Witness ID equivalent
        public required string AccountName { get; set; }
        public required string AccountNumber { get; set; }
        public required string BankName { get; set; }
        public required string ParentId { get; set; }  // Location ID equivalent
        public string? NomineeName { get; set; }
        public string? NomineeRelation { get; set; }
        public string? BankBranchName { get; set; }
        public string? VatPanName { get; set; }
        public string? VatPanRegistrationNumber { get; set; }
        public string? VatPanIssuedFrom { get; set; }
    }

    public class DistributorLoginDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    public class DistributorReadDto
    {
        public required string Id { get; set; }
        public required string FirstName { get; set; }
        public string? MiddleName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; }
        public required string PermanentFullAddress { get; set; }
        public required string PhoneNumber { get; set; }
        public required string DOB { get; set; }
        public required string CitizenshipNo { get; set; }
        public string? CitizenshipImageUrl { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public required string ReferalId { get; set; }
        public required string AccountName { get; set; }
        public required string AccountNumber { get; set; }
        public required string BankName { get; set; }
        public string? BankBranchName { get; set; }
        public required string ParentId { get; set; }
        public string? NomineeName { get; set; }
        public string? NomineeRelation { get; set; }
        public string? VatPanName { get; set; }
        public string? VatPanRegistrationNumber { get; set; }
        public string? VatPanIssuedFrom { get; set; }
        public string? Position { get; set; }
        public required decimal LeftWallet { get; set; }
        public required decimal TotalWallet { get; set; }
        public required decimal RightWallet { get; set; }
        public required decimal Totalcomission { get; set; }
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
        public string ParentId { get; set; } = null!;
        public decimal LeftWallet { get; set; }
        public decimal RightWallet { get; set; }
        public List<DistributorTreeDto> Children { get; set; } = new();
    }

    public class DownlineUserDto
    {
        public string Id { get; set; } = default!;
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ParentId { get; set; }
        public string Position { get; set; } = default!;
        public decimal LeftWallet { get; set; }
        public decimal RightWallet { get; set; }
        public string Rank { get; set; } = default!;
    }

    public class ChangeParentRequest
    {
        public required string NewParentId { get; set; }
        public required string ChildId { get; set; }
    }
}
