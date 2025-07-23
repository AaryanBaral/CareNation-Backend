using backend.Dto;
using backend.Models;

namespace backend.Mapper
{
    public static class DistributorMapper
    {
        // Map DistributorSignUpDto to User entity (update additional distributor info)
        public static User ToUser(this DistributorSignUpDto dto, User user)
        {
            user.DOB = dto.DOB;
            user.CitizenshipNo = dto.CitizenshipNo;
            user.ReferalId = dto.ReferalId;
            user.ParentId = dto.ParentId;
            user.AccountName = dto.AccountName;
            user.AccountNumber = dto.AccountNumber;
            return user;
        }

        // Map User entity to DistributorReadDto
        public static DistributorReadDto ToDistributorReadDto(this User user, string role)
        {
            return new DistributorReadDto
            {
                Id = user.Id,
                FullName = user.FullName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Role = role,
                Address = user.Address ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                DOB = user.DOB ?? string.Empty,
                CitizenshipNo = user.CitizenshipNo ?? string.Empty,
                ReferalId = user.ReferalId ?? string.Empty,
                AccountName = user.AccountName!,
                AccountNumber = user.AccountNumber!,
                BankName = user.BankName!,
                ParentId = user.ParentId!
            };
        }
    }
}
