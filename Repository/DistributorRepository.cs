
using backend.Models;
using backend.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using backend.Interface.Repository;
using backend.Dto;

namespace backend.Repository
{
    public class DistributorRepository : IDistributorRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DistributorRepository(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
            _roleManager = roleManager;
        }

        public async Task<bool> CanBecomeDistributorAsync(string userId)
        {
            var totalPurchases = await _context.Orders
                .Where(o => o.UserId == userId)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

            return totalPurchases > 5000m;
        }

        public async Task<User?> LoginDistributorAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return null;

            if (!await _userManager.CheckPasswordAsync(user, password))
                return null;

            return user;
        }

        public async Task<bool> SignUpDistributorAsync(User user)
        {
            // Assign the left or right position based on current children
            var availablePosition = await GetAvailablePosition(user.ParentId!);
            if (availablePosition == null)
                throw new InvalidOperationException("Parent already has two children");

            user.Position = availablePosition.Value;

            // Validate placement
            var isValidPlacement = await ValidatePlacement(user.ReferalId!, user.ParentId!);
            if (!isValidPlacement)
                throw new InvalidOperationException("Placement cannot be done");

            _context.Users.Update(user);

            var currentRoles = await _userManager.GetRolesAsync(user);

            if (currentRoles.Contains("User"))
                await _userManager.RemoveFromRoleAsync(user, "User");

            if (!await _roleManager.RoleExistsAsync("Distributor"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Distributor"));
            }
            if (!currentRoles.Contains("Distributor"))
            {
                var addRoleResult = await _userManager.AddToRoleAsync(user, "Distributor");
                if (!addRoleResult.Succeeded)
                    return false;
            }

            return true;
        }


        public async Task<User?> GetDistributorByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            var roles = user != null ? await _userManager.GetRolesAsync(user) : null;

            if (user == null || roles == null || !roles.Contains("Distributor"))
                return null;

            return user;

        }

        public async Task<List<User>> GetAllDistributorsAsync()
        {
            return (await _userManager.GetUsersInRoleAsync("Distributor")).ToList();
        }

        public async Task<bool> UpdateDistributorAsync(User user)
        {
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> DeleteDistributorAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return false;

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Distributor"))
            {
                var removeRoleResult = await _userManager.RemoveFromRoleAsync(user, "Distributor");
                if (!removeRoleResult.Succeeded)
                    return false;
            }

            var deleteResult = await _userManager.DeleteAsync(user);
            return deleteResult.Succeeded;
        }
        public async Task Addcommitsion(double commision, string userId)
        {
            var user = await GetDistributorByIdAsync(userId) ?? throw new KeyNotFoundException("Invalid id");
            user.CommisionAmmount += commision;
            await _context.SaveChangesAsync();
        }
        public async Task<bool> CanPlaceUnder(string parentId)
        {
            if (string.IsNullOrEmpty(parentId))
                return false;

            // Count how many users have this parentId
            var childrenCount = await _context.Users.CountAsync(u => u.ParentId == parentId);
            var childrens = await _context.Users.Where(u => u.ParentId == parentId).ToListAsync();


            // Return true if less than 2 children
            return childrenCount < 2;
        }

        public async Task<bool> IsDescendant(string rootId, string nodeId)
        {
            // Recursively check if nodeId is under rootId in the tree
            if (rootId == nodeId) return true;

            var children = await _context.Users.Where(u => u.ParentId == rootId).Select(u => u.Id).ToListAsync();
            foreach (var childId in children)
            {
                if (await IsDescendant(childId, nodeId))
                    return true;
            }
            return false;
        }

        public async Task<bool> ValidatePlacement(string referralId, string parentId)
        {
            // Ensure parentId is under referralId's subtree or equal
            return await IsDescendant(referralId, parentId) && await CanPlaceUnder(parentId);
        }


        public async Task<DistributorTreeDto?> GetUserTreeAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            return await BuildTreeAsync(user);
        }

        private async Task<DistributorTreeDto> BuildTreeAsync(User user)
        {
            var children = await _context.Users
                .Where(u => u.ParentId == user.Id)
                .ToListAsync();

            var dto = new DistributorTreeDto
            {
                Id = user.Id,
                Name = user.FullName,
                Position = user.Position.ToString(),
                Children = new List<DistributorTreeDto>()
            };

            foreach (var child in children)
            {
                var childDto = await BuildTreeAsync(child);
                dto.Children.Add(childDto);
            }

            return dto;
        }
        public async Task<NodePosition?> GetAvailablePosition(string parentId)
        {
            var children = await _context.Users
                .Where(u => u.ParentId == parentId)
                .ToListAsync();

            bool hasLeft = children.Any(c => c.Position == NodePosition.Left);
            bool hasRight = children.Any(c => c.Position == NodePosition.Right);

            if (!hasLeft) return NodePosition.Left;
            if (!hasRight) return NodePosition.Right;

            // No available position
            return null;
        }
        public async Task<List<User>> GetPeopleIReferredAsync(string myUserId)
        {
            return await _context.Users
                .Where(u => u.ReferalId == myUserId)
                .ToListAsync();
        }
        public async Task<List<User>> GetMyUplineAsync(string myUserId)
        {
            var upline = new List<User>();
            var current = await _context.Users.FindAsync(myUserId);

            while (current != null && !string.IsNullOrEmpty(current.ParentId))
            {
                var parent = await _context.Users.FindAsync(current.ParentId);
                if (parent == null)
                    break;
                upline.Add(parent);
                current = parent;
            }

            return upline;
        }


    }


}
