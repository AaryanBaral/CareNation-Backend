using backend.Dto;
using backend.Models;

namespace backend.Mapper
{
    public static class AdminMapper
    {
        public static AdminReadDto ToReadAdminDto(this User admin, string role)
        {
            return new AdminReadDto
            {
                Id         = admin.Id,
                FirstName  = admin.FirstName!,
                MiddleName = admin.MiddleName!,
                LastName   = admin.LastName!,
                Email      = admin.Email!,
                Role       = role,
                CreatedAt  = admin.CreatedAt,
            };
        }
    }
}
