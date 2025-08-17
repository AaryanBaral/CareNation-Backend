using backend.Interface.Repository;
using backend.Models;
using Microsoft.AspNetCore.Identity;

namespace backend.Repository
{
    public static class AdminRoles
    {
        // Single source of truth for admin sub-roles
        public const string SuperAdmin         = "SuperAdmin";
        public const string BranchManager      = "BranchManager";
        public const string FinanceManager     = "FinanceManager";
        public const string SalesManager       = "SalesManager";
        public const string ProductManager     = "ProductManager";
        public const string DistributorManager = "DistributorManager";

        // Optional legacy "Admin" (keep if you already use it)
        public const string Admin              = "Admin";

        public static readonly string[] All =
        {
            SuperAdmin, BranchManager, FinanceManager, SalesManager, ProductManager, DistributorManager, Admin
        };
    }

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
            _userManager   = userManager;
            _signInManager = signInManager;
            _roleManager   = roleManager;
        }

        // Ensure all admin roles exist (call before first use where needed)
        private async Task EnsureAdminRolesAsync()
        {
            foreach (var role in AdminRoles.All)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        /// <summary>
        /// Get all users who belong to ANY admin sub-role.
        /// </summary>
        public async Task<IEnumerable<User>> GetAllAdminsAsync()
        {
            await EnsureAdminRolesAsync();

            var byId = new Dictionary<string, User>();
            foreach (var role in AdminRoles.All)
            {
                var users = await _userManager.GetUsersInRoleAsync(role);
                foreach (var u in users)
                    byId[u.Id] = u; // de-duplicate across roles
            }
            return byId.Values;
        }

        /// <summary>
        /// Get an admin by Id ONLY if they are in ANY admin sub-role.
        /// </summary>
        public async Task<User?> GetAdminByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return null;

            foreach (var role in AdminRoles.All)
            {
                if (await _userManager.IsInRoleAsync(user, role))
                    return user;
            }
            return null;
        }

        /// <summary>
        /// Create an admin with default role (legacy "Admin").
        /// Prefer the overload that specifies a sub-role.
        /// </summary>
        public async Task<bool> CreateAdminAsync(User user, string password)
        {
            // Keep backward compatibility with your existing calls
            return await CreateAdminAsync(user, password, AdminRoles.Admin);
        }

        /// <summary>
        /// Create an admin with a specific sub-role (e.g., "BranchManager").
        /// </summary>
        public async Task<bool> CreateAdminAsync(User user, string password, string role)
        {
            await EnsureAdminRolesAsync();

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded) return false;

            if (!AdminRoles.All.Contains(role))
                throw new ArgumentException($"Invalid admin role: {role}");

            var roleResult = await _userManager.AddToRoleAsync(user, role);
            return roleResult.Succeeded;
        }

        /// <summary>
        /// Assign an additional admin sub-role to an existing user.
        /// </summary>
        public async Task<bool> AssignAdminRoleAsync(string userId, string role)
        {
            await EnsureAdminRolesAsync();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            if (!AdminRoles.All.Contains(role))
                throw new ArgumentException($"Invalid admin role: {role}");

            var result = await _userManager.AddToRoleAsync(user, role);
            return result.Succeeded;
        }

        /// <summary>
        /// Remove an admin sub-role from a user.
        /// </summary>
        public async Task<bool> RemoveAdminRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            if (!AdminRoles.All.Contains(role))
                throw new ArgumentException($"Invalid admin role: {role}");

            var result = await _userManager.RemoveFromRoleAsync(user, role);
            return result.Succeeded;
        }

        /// <summary>
        /// Replace all admin roles for a user with exactly the provided roles.
        /// </summary>
        public async Task<bool> SetAdminRolesAsync(string userId, IEnumerable<string> roles)
        {
            await EnsureAdminRolesAsync();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var target = roles?.Where(r => AdminRoles.All.Contains(r)).Distinct().ToArray() ?? Array.Empty<string>();
            var current = await _userManager.GetRolesAsync(user);

            // Remove roles that are admin roles but not in target
            foreach (var r in current.Where(r => AdminRoles.All.Contains(r) && !target.Contains(r)))
            {
                var rem = await _userManager.RemoveFromRoleAsync(user, r);
                if (!rem.Succeeded) return false;
            }
            // Add missing
            foreach (var r in target.Where(r => !current.Contains(r)))
            {
                var add = await _userManager.AddToRoleAsync(user, r);
                if (!add.Succeeded) return false;
            }
            return true;
        }

        /// <summary>
        /// Update admin's profile fields (not roles).
        /// </summary>
        public async Task<bool> UpdateAdminAsync(User user)
        {
            var existingUser = await _userManager.FindByIdAsync(user.Id);
            if (existingUser == null) return false;

            existingUser.FirstName  = user.FirstName;
            existingUser.MiddleName = user.MiddleName;
            existingUser.LastName   = user.LastName;
            existingUser.Email      = user.Email;

            var result = await _userManager.UpdateAsync(existingUser);
            return result.Succeeded;
        }

        /// <summary>
        /// Delete a user ONLY if they belong to ANY admin sub-role.
        /// </summary>
        public async Task<bool> DeleteAdminAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return false;

            var isAnyAdmin = false;
            foreach (var role in AdminRoles.All)
            {
                if (await _userManager.IsInRoleAsync(user, role))
                {
                    isAnyAdmin = true;
                    break;
                }
            }
            if (!isAnyAdmin) return false;

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        /// <summary>
        /// Return user if credentials are valid AND user has ANY admin sub-role.
        /// </summary>
        public async Task<User?> LoginAdminAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return null;

            // Must have at least one admin sub-role
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Any(r => AdminRoles.All.Contains(r))) return null;

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
            if (!result.Succeeded) return null;

            return user;
        }

        /// <summary>
        /// Get all admins in a specific sub-role (e.g., "FinanceManager").
        /// </summary>
        public async Task<IEnumerable<User>> GetAdminsByRoleAsync(string role)
        {
            if (!AdminRoles.All.Contains(role))
                throw new ArgumentException($"Invalid admin role: {role}");

            await EnsureAdminRolesAsync();
            return await _userManager.GetUsersInRoleAsync(role);
        }

        /// <summary>
        /// Convenience: get all roles for a user (useful for admin UI)
        /// </summary>
        public async Task<IList<string>> GetUserRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Array.Empty<string>();
            return await _userManager.GetRolesAsync(user);
        }
    }
}
