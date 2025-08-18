using Microsoft.AspNetCore.Identity;


namespace backend.Models
{
    public enum NodePosition
    {
        Left,
        Right
    }

    public enum UserType
    {
        None,
        Beginner,
        Area,
        Zonal,
        Regional,
        Nation
    }

    public enum Rank
    {
        None,
        Executive,
        Rising,
        Silver,
        Gold,
        Star,
        Pearl,
        Diamond,
        Crown,
        GlobalDirector
    }


    public class User : IdentityUser
    {
        // MLM Hierarchy
        public string? ReferalId { get; set; }
        public string? ParentId { get; set; }

        // Profile Information
        public string? FirstName { get; set; }
        public bool IsDeleted { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? Gender { get; set; }

        // Father / Spouse Name
        public string? FatherOrSpouseFirstName { get; set; }
        public string? FatherOrSpouseMiddleName { get; set; }
        public string? FatherOrSpouseLastName { get; set; }

        // Date of Birth
        public string? DOB_BS { get; set; }
        public string? DOB_AD { get; set; }

        // Citizenship / Passport
        public string? CitizenshipOrPassportNo { get; set; }
        public string? CitizenshipOrPassportIssuedFrom { get; set; }
        public string? CitizenshipImageUrl { get; set; }
        public string? PassportImageUrl { get; set; }

        // Permanent Address
        public string? PermanentFullAddress { get; set; }
        public string? PermanentZipCode { get; set; }
        public string? PermanentCity { get; set; }
        public string? PermanentCountry { get; set; }

        // Delivery Address
        public bool IsDeliverySameAsPermanent { get; set; } = true;
        public string? DeliveryFullAddress { get; set; }
        public string? DeliveryZipCode { get; set; }
        public string? DeliveryCity { get; set; }
        public string? DeliveryCountry { get; set; }

        // Contact Information
        public string? PhoneNo { get; set; }
        public string? MobileNo { get; set; }
        public string? EmailAddress { get; set; }
        public string? ProfilePictureUrl { get; set; }

        // Nominee Detail
        public string? NomineeName { get; set; }
        public string? NomineeRelation { get; set; }

        // Bank Detail
        public string? BankName { get; set; }
        public string? BankBranchName { get; set; }
        public string? NameOnAccount { get; set; }
        public string? AccountNumber { get; set; }

        // VAT/PAN Detail
        public string? VatPanName { get; set; }
        public string? VatPanRegistrationNumber { get; set; }
        public string? VatPanIssuedFrom { get; set; }

        // MLM Stats
        public NodePosition? Position { get; set; }
        public UserType Type { get; set; } = UserType.None;

        public Rank? Rank { get; set; } // Distributor Rank (stored as string in DB)

        public decimal CommisionAmmount { get; set; } = 0;
        public decimal LeftWallet { get; set; } = 0;
        public decimal RightWallet { get; set; } = 0;
        public decimal TotalWallet { get; set; } = 0;
        public decimal TotalPoints { get; set; } = 0;

        public bool LeadershipBonusGiven { get; set; } = false;
        public bool RankBonusGiven { get; set; } = false;
        public UserType LastRankAwarded { get; set; } = UserType.None;

        public decimal RepurchasePoints { get; set; }

        // Fund pools (from the remaining 58% → 50% of base)
        public decimal RoyaltyPoints { get; set; }
        public decimal TravelPoints { get; set; }
        public decimal CarPoints { get; set; }
        public decimal HousePoints { get; set; }

        // For company account only (8% share + unallocated fund slices)
        public decimal CompanyPoints { get; set; }

        // Flag a special user as the company account
        public bool IsCompanyAccount { get; set; }

        // Metadata
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
