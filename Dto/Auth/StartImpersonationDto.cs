namespace backend.Dto.Auth
{
    public class StartImpersonationDto
    {
        public required string TargetUserId { get; set; }
        public string? Reason { get; set; }
        public string? ReturnUrl { get; set; } // optional: where to send admin back when exiting
    }
    public class StartImpersonationResponse
    {
        public required string Code { get; set; }
        public required string DistributorImpersonationUrl { get; set; } // landing page to POST the code
    }
    public class RedeemImpersonationDto
    {
        public required string Code { get; set; }
    }
        public class ReauthDto
    {
        public required string Password { get; set; }
    }

}
