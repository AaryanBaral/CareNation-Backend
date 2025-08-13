using backend.Data;
using backend.Dto;
using backend.Interface.Repository;
using backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository
{
    public class UserRepository(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, AppDbContext context, IUserIdGenerator userIdGenerator) : IUserRepository
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly AppDbContext _context = context;
        private readonly IUserIdGenerator _userIdGenerator = userIdGenerator;

        public async Task AddUser(User user, string role, string password)
        {
            user.Id = await _userIdGenerator.NextAsync();
            // Check if role exists, create if not
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, role);
            }
            else
            {
                throw new Exception("User creation failed: " + string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }
        public async Task<List<string>> LoginAndGetRole(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email) ?? throw new KeyNotFoundException("Invalid credentials");
            var result = await _userManager.CheckPasswordAsync(user, password);
            if (!result) throw new KeyNotFoundException("Invalid Exception");
            var roles = user != null ? await _userManager.GetRolesAsync(user) : null;
            return [.. roles!];
        }
        public async Task UpdateUser(User user, string role)
        {
            var existing = await _userManager.FindByIdAsync(user.Id);
            if (existing != null)
            {
                existing.UserName = user.UserName;
                existing.Email = user.Email;

                var result = await _userManager.UpdateAsync(existing);
                if (!result.Succeeded)
                {
                    throw new Exception("Update failed: " + string.Join("; ", result.Errors.Select(e => e.Description)));
                }

                // Optionally update role
                var currentRoles = await _userManager.GetRolesAsync(existing);
                if (!currentRoles.Contains(role))
                {
                    await _userManager.RemoveFromRolesAsync(existing, currentRoles);
                    await _userManager.AddToRoleAsync(existing, role);
                }
            }
        }

        public async Task DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
        }

        public async Task<User?> GetUserById(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }
        public async Task<User?> GetUserByEmail(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _userManager.Users.AsNoTracking().ToListAsync();
        }

        public async Task SignUp(User user, string password, string role)
        {
            await AddUser(user, role, password);
        }

        public async Task<User?> Login(UserLoginDto userLoginDto)
        {
            var user = await _userManager.FindByEmailAsync(userLoginDto.Email);
            if (user == null)
                return null;

            var result = await _userManager.CheckPasswordAsync(user, userLoginDto.Password);
            return result ? user : null;
        }
        public async Task<bool> CanBecomeDistributorAsync(string userId)
        {
            // Assuming you have Orders with UserId and TotalAmount
            var totalPurchases = await _context.Orders
                .Where(o => o.UserId == userId)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

            return totalPurchases > 5000m;
        }

        public async Task<bool> SignUpDistributorAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // Remove from old roles (e.g. "User") and add to "Distributor"
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Contains("User"))
            {
                await _userManager.RemoveFromRoleAsync(user, "User");
            }

            if (!currentRoles.Contains("Distributor"))
            {
                var addRoleResult = await _userManager.AddToRoleAsync(user, "Distributor");
                if (!addRoleResult.Succeeded)
                {
                    // Optionally revert changes or handle failure
                    return false;
                }
            }

            return true;
        }
        

    }
}
