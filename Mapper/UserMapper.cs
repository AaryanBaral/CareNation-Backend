using backend.Dto;
using backend.Models;

namespace backend.Mapper
{
    public static class UserMapper
    {
        public static User ToUser(this UserSignUpDto dto)
        {
            return new User
            {
                FirstName = dto.FirstName,
                MiddleName = dto.MiddleName,
                LastName = dto.LastName,
                Email = dto.Email,
                PermanentFullAddress = dto.PermanentFullAddress,
                UserName = dto.Email.Split('@')[0],
                PhoneNumber = dto.PhoneNumber
            };
        }  

        public static UserReadDto ToReadDto(this User user, string role)
        {
            return new UserReadDto
            {
                Id = user.Id,
                FirstName = user.FirstName!,
                MiddleName = user.MiddleName,
                LastName = user.LastName!,
                Email = user.Email!,
                Role = role,
                PermanentFullAddress = user.PermanentFullAddress!,
                PhoneNumber = user.PhoneNumber!
            };
        }

        public static void UpdateUser(this User user, UserReadDto dto)
        {
            user.FirstName = dto.FirstName;
            user.MiddleName = dto.MiddleName;
            user.LastName = dto.LastName;
            user.Email = dto.Email;
            user.PermanentFullAddress = dto.PermanentFullAddress;
        }
    }
}
