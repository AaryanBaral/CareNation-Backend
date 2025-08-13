using backend.Dto;
using backend.Models;

namespace backend.Mapper
{
    public static class DistributorMapper
    {
        // Map DistributorSignUpDto -> existing User (distributor-specific fields only)
        public static User ToUser(this DistributorSignUpDto dto, User user)
        {
            // Dates
            user.DOB_AD = dto.DOB; // store incoming DOB in AD field

            // Identity docs & images
            user.CitizenshipOrPassportNo = dto.CitizenshipNo;
            if (!string.IsNullOrWhiteSpace(dto.CitizenshipImageUrl))
                user.CitizenshipImageUrl = dto.CitizenshipImageUrl;
            if (!string.IsNullOrWhiteSpace(dto.ProfilePictureUrl))
                user.ProfilePictureUrl = dto.ProfilePictureUrl;

            // Hierarchy
            user.ReferalId = dto.ReferalId;
            user.ParentId = dto.ParentId;

            // Bank / account
            user.NameOnAccount = dto.AccountName;
            user.AccountNumber = dto.AccountNumber;
            user.BankName = dto.BankName;
            if (!string.IsNullOrWhiteSpace(dto.BankBranchName))
                user.BankBranchName = dto.BankBranchName;

            // Nominee
            if (!string.IsNullOrWhiteSpace(dto.NomineeName))
                user.NomineeName = dto.NomineeName;
            if (!string.IsNullOrWhiteSpace(dto.NomineeRelation))
                user.NomineeRelation = dto.NomineeRelation;

            // VAT / PAN
            if (!string.IsNullOrWhiteSpace(dto.VatPanName))
                user.VatPanName = dto.VatPanName;
            if (!string.IsNullOrWhiteSpace(dto.VatPanRegistrationNumber))
                user.VatPanRegistrationNumber = dto.VatPanRegistrationNumber;
            if (!string.IsNullOrWhiteSpace(dto.VatPanIssuedFrom))
                user.VatPanIssuedFrom = dto.VatPanIssuedFrom;

            return user;
        }

        // Map User -> DistributorReadDto (using new field names)
        public static DistributorReadDto ToDistributorReadDto(this User user, string role)
        {
            return new DistributorReadDto
            {
                Id = user.Id,
                // Names (DTO now expects split fields)
                FirstName = user.FirstName ?? string.Empty,
                MiddleName = user.MiddleName,
                LastName = user.LastName ?? string.Empty,

                Email = user.Email ?? string.Empty,
                Role = role,

                // Address (prefer permanent, fallback to delivery)
                PermanentFullAddress = user.PermanentFullAddress
                                       ?? user.DeliveryFullAddress
                                       ?? string.Empty,

                PhoneNumber = user.PhoneNumber ?? string.Empty,

                // Dates
                DOB = user.DOB_AD ?? user.DOB_BS ?? string.Empty,

                // Identity docs & images
                CitizenshipNo = user.CitizenshipOrPassportNo ?? string.Empty,
                CitizenshipImageUrl = user.CitizenshipImageUrl,
                ProfilePictureUrl = user.ProfilePictureUrl,

                // Hierarchy
                ReferalId = user.ReferalId ?? string.Empty,
                ParentId = user.ParentId ?? string.Empty,

                // Bank / account
                AccountName = user.NameOnAccount ?? string.Empty,
                AccountNumber = user.AccountNumber ?? string.Empty,
                BankName = user.BankName ?? string.Empty,
                BankBranchName = user.BankBranchName,

                // Nominee
                NomineeName = user.NomineeName,
                NomineeRelation = user.NomineeRelation,

                // VAT / PAN
                VatPanName = user.VatPanName,
                VatPanRegistrationNumber = user.VatPanRegistrationNumber,
                VatPanIssuedFrom = user.VatPanIssuedFrom,

                // Position / wallets
                Position = user.Position?.ToString(),
                LeftWallet = user.LeftWallet,
                RightWallet = user.RightWallet,
                TotalWallet = user.TotalWallet,
                Totalcomission = user.CommisionAmmount
            };
        }
    }
}
