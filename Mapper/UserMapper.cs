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
                FullName = dto.FullName,
                Email = dto.Email,
                Address = dto.Address,
                UserName = dto.Email.Split('@')[0],
                PhoneNumber = dto.PhoneNumber
            };
        }  

        public static UserReadDto ToReadDto(this User user, string role)
        {
            return new UserReadDto
            {
                Id = user.Id,
                FullName = user.FullName!,
                Email = user.Email!,
                Role = role,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber!

            };
        }

        public static void UpdateUser(this User user, UserReadDto dto)
        {
            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.Address = dto.Address;
        }
    }
}
