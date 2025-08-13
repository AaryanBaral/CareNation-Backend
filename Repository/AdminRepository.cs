using backend.Interface.Repository;
using backend.Models;
using Microsoft.AspNetCore.Identity;

namespace backend.Repository
{
    public class AdminRepository : IAdminRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminRepository(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        public async Task<IEnumerable<User>> GetAllAdminsAsync()
        {
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            return adminUsers;
        }

        public async Task<User?> GetAdminByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
                return user;
            return null;
        }

        public async Task<bool> CreateAdminAsync(User user, string password)
        {
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded) return false;

            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "Admin");
            return roleResult.Succeeded;
        }

        public async Task<bool> UpdateAdminAsync(User user)
        {
            var existingUser = await _userManager.FindByIdAsync(user.Id);
            if (existingUser == null) return false;

            // Replace FullName with split fields
            existingUser.FirstName  = user.FirstName;
            existingUser.MiddleName = user.MiddleName;
            existingUser.LastName   = user.LastName;
            existingUser.Email       = user.Email;

            var result = await _userManager.UpdateAsync(existingUser);
            return result.Succeeded;
        }

        public async Task<bool> DeleteAdminAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return false;

            if (!await _userManager.IsInRoleAsync(user, "Admin")) return false;

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        public async Task<User?> LoginAdminAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return null;

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (!isAdmin) return null;

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
            if (!result.Succeeded) return null;

            return user;
        }
    }
}
