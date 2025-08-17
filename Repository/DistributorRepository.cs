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
        private readonly ICommissionPayoutRepository _commissionPayoutRepository;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DistributorRepository(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, AppDbContext context, ICommissionPayoutRepository commissionPayoutRepository)
        {
            _userManager = userManager;
            _context = context;
            _roleManager = roleManager;
            _commissionPayoutRepository = commissionPayoutRepository;
        }

        // Helper: build display name from split parts (fallback to username/email)
        private static string GetDisplayName(User u)
        {
            var parts = new[] { u.FirstName, u.MiddleName, u.LastName }
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s!.Trim());
            var name = string.Join(" ", parts);
            if (!string.IsNullOrWhiteSpace(name)) return name;
            return u.UserName ?? u.Email ?? "User";
        }







        private async Task AddWalletTransactionAsync(User user, decimal amount, string reason, string? triggeredByFullName = null)
        {
            user.TotalWallet += amount;
            await _commissionPayoutRepository.AddAsync(new CommissionPayout
            {
                UserId = user.Id,
                PayoutDate = DateTime.UtcNow,
                Amount = amount,
                Remarks = $"{reason}{(triggeredByFullName != null ? $" (by {triggeredByFullName})" : "")}"
            });
        }

        private async Task<decimal> GetTodayBinaryTotalAsync(string userId)
        {
            var todayUtc = DateTime.UtcNow.Date;
            return await _context.CommissionPayouts
                .Where(c => c.UserId == userId
                    && c.PayoutDate >= todayUtc
                    && c.Remarks == "Binary Commission")
                .SumAsync(c => (decimal?)c.Amount) ?? 0m;
        }

        private decimal GetDailyCapFor(User u)
        {
            return _dailyCapByRank.TryGetValue(u.Type, out var cap) ? cap : 0m;
        }

        private async Task<UserType> GetRankFromOwnPurchasesAsync(string userId)
        {
            var ownPurchase = await _context.Orders
                .Where(o => o.UserId == userId)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

            foreach (var (min, max, r) in _rankByPurchase)
            {
                if (ownPurchase >= min && ownPurchase <= max) return r;
            }
            return UserType.None;
        }

        private static void AddCommissionTransactionAsync(User user, decimal amount, string reason, string? triggeredByFullName = null)
        {
            user.CommisionAmmount += amount;
        }



        // REPLACE your current UpdateUserRank with this:
        private async Task UpdateUserRank(User user)
        {
            user.Type = await GetRankFromOwnPurchasesAsync(user.Id);
        }

        public async Task<List<User>> GetMyDownlineAsync(string userId)
        {
            var allUsers = await _context.Users.ToListAsync();
            var downlines = new List<User>();

            void Traverse(string parentId)
            {
                var children = allUsers.Where(u => u.ParentId == parentId).ToList();
                foreach (var child in children)
                {
                    downlines.Add(child);
                    Traverse(child.Id);
                }
            }

            Traverse(userId);
            return downlines;
        }

        public async Task<int> GetReferralCountAsync(string userId)
        {
            return await _context.Users.CountAsync(u => u.ReferalId == userId);
        }

        public async Task UpdateRanksFromBottomAsync()
        {
            var allUsers = await _context.Users.ToListAsync();
            var userLookup = allUsers
                .Where(u => !string.IsNullOrEmpty(u.ParentId))
                .GroupBy(u => u.ParentId!)
                .ToDictionary(g => g.Key, g => g.ToList());

            var roots = allUsers.Where(u => string.IsNullOrEmpty(u.ParentId)).ToList();

            foreach (var root in roots)
            {
                await UpdateRankDFS(root, userLookup);
            }

            await _context.SaveChangesAsync();
        }

        private async Task UpdateRankDFS(User user, Dictionary<string, List<User>> userLookup)
        {
            if (userLookup.TryGetValue(user.Id, out var children))
            {
                foreach (var child in children)
                {
                    await UpdateRankDFS(child, userLookup);
                }
            }
            await UpdateUserRank(user);
        }

        public async Task<WalletStatementDto> GetWalletStatementAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new KeyNotFoundException("User not found");

            var transactions = new List<WalletTransactionDto>();

            var commissions = await _context.CommissionPayouts
                .Where(c => c.UserId == userId)
                .Select(c => new WalletTransactionDto
                {
                    Date = c.PayoutDate,
                    Type = "Commission",
                    Amount = c.Amount,
                    Remarks = c.Remarks
                })
                .ToListAsync();

            transactions.AddRange(commissions);

            var withdrawals = await _context.WithdrawalRequests
                .Where(w => w.UserId == userId && w.Status == "Approved")
                .Select(w => new WalletTransactionDto
                {
                    Date = w.ProcessedDate ?? w.RequestDate,
                    Type = "Withdrawal",
                    Amount = -w.Amount,
                    Remarks = w.Remarks
                })
                .ToListAsync();
            transactions.AddRange(withdrawals);

            var sentTransfers = await _context.BalanceTransfers
                .Where(t => t.SenderId == userId)
                .Select(t => new WalletTransactionDto
                {
                    Date = t.TransferDate,
                    Type = "Transfer Out",
                    Amount = -t.Amount,
                    Remarks = t.Remarks ?? $"To: {GetDisplayName(t.Receiver)}"
                })
                .ToListAsync();
            transactions.AddRange(sentTransfers);

            var receivedTransfers = await _context.BalanceTransfers
                .Where(t => t.ReceiverId == userId)
                .Select(t => new WalletTransactionDto
                {
                    Date = t.TransferDate,
                    Type = "Transfer In",
                    Amount = t.Amount,
                    Remarks = t.Remarks ?? $"From: {GetDisplayName(t.Sender)}"
                })
                .ToListAsync();
            transactions.AddRange(receivedTransfers);

            transactions = transactions.OrderBy(t => t.Date).ToList();

            decimal running = 0;
            foreach (var t in transactions)
            {
                running += t.Amount;
                t.BalanceAfter = running;
            }

            transactions.Insert(0, new WalletTransactionDto
            {
                Date = transactions.FirstOrDefault()?.Date ?? DateTime.UtcNow,
                Type = "Initial Balance",
                Amount = 0,
                Remarks = "Wallet created",
                BalanceAfter = 0
            });
            return new WalletStatementDto
            {
                WalletBalance = running,
                Transactions = transactions
            };
        }

        public async Task<List<User>> GetDownlineAsync(string userId)
        {
            var allUsers = await _context.Users.ToListAsync();
            var result = new List<User>();

            void GetChildren(string parentId)
            {
                var children = allUsers.Where(u => u.ParentId == parentId).ToList();
                foreach (var child in children)
                {
                    result.Add(child);
                    GetChildren(child.Id);
                }
            }
            GetChildren(userId);
            return result;
        }

        public async Task UpdateProfilePictureUrlAsync(string userId, string? imageUrl)
        {
            var user = await _userManager.FindByIdAsync(userId)
                       ?? throw new KeyNotFoundException("User not found.");

            user.ProfilePictureUrl = imageUrl;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
                throw new InvalidOperationException($"Failed to update profile picture URL. {errors}");
            }
        }

        public async Task UpdateCitizenshipImageUrlAsync(string userId, string? imageUrl)
        {
            var user = await _userManager.FindByIdAsync(userId)
                       ?? throw new KeyNotFoundException("User not found.");

            user.CitizenshipImageUrl = imageUrl;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
                throw new InvalidOperationException($"Failed to update citizenship image URL. {errors}");
            }
        }

        public async Task<bool> CanBecomeDistributorAsync(string userId)
        {
            var totalPurchases = await _context.Orders
                .Where(o => o.UserId == userId)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

            return totalPurchases >= 5000m;
        }

        public async Task<User?> LoginDistributorAsync(string email, string password)
        {
            await UpdateRanksFromBottomAsync();
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return null;

            if (!await _userManager.CheckPasswordAsync(user, password))
                return null;

            return user;
        }

        public async Task ChangeParentAsync(string childId, string newParentId)
        {
            var user = await _userManager.FindByIdAsync(childId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            if (!await ValidatePlacement(childId, newParentId))
                throw new InvalidOperationException("Placement cannot be done");

            var availablePosition = await GetAvailablePosition(newParentId);
            if (availablePosition == null)
                throw new InvalidOperationException("Parent already has two children");

            user.ParentId = newParentId;
            user.Position = availablePosition.Value;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> SignUpDistributorAsync(User user)
        {
            var availablePosition = await GetAvailablePosition(user.ParentId!);
            if (availablePosition == null)
                throw new InvalidOperationException("Parent already has two children");

            user.Position = availablePosition.Value;

            var isValidPlacement = await ValidatePlacement(user.ReferalId!, user.ParentId!);
            if (!isValidPlacement)
                throw new InvalidOperationException("Placement cannot be done");

            _context.Users.Update(user);

            var currentRoles = await _userManager.GetRolesAsync(user);

            if (currentRoles.Contains("User"))
                await _userManager.RemoveFromRoleAsync(user, "User");

            if (!await _roleManager.RoleExistsAsync("Distributor"))
                await _roleManager.CreateAsync(new IdentityRole("Distributor"));

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

        public async Task Addcommitsion(decimal commission, string userId)
        {
            var user = await GetDistributorByIdAsync(userId) ?? throw new KeyNotFoundException("Invalid id");
            var name = GetDisplayName(user);
            AddCommissionTransactionAsync(user, commission, "Manual commission adjustment", name);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> CanPlaceUnder(string parentId)
        {
            if (string.IsNullOrEmpty(parentId))
                return false;

            var childrenCount = await _context.Users.CountAsync(u => u.ParentId == parentId);
            return childrenCount < 2;
        }

        public async Task<bool> IsDescendant(string rootId, string nodeId)
        {
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
                Name = GetDisplayName(user),
                Position = user.Position.ToString(),
                Children = new List<DistributorTreeDto>(),
                LeftWallet = user.LeftWallet,
                RightWallet = user.RightWallet,
                ParentId = user.ParentId!,
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











        /*======================================= Comission Related ===================================
                                This section is for commission related operations
         ======================================== Comission Related =================================== */









        public enum RoyaltyFund { Royalty, Travel, Car, House }




        private static readonly (decimal min, decimal max, UserType rank)[] _rankByPurchase =
        {
            (5000m,     15000m - 0.01m, UserType.Beginner), // 5,000 ≤ x < 15,000
            (15000m,    30000m - 0.01m, UserType.Area),     // 15,000 ≤ x < 30,000
            (30000m,    60000m - 0.01m, UserType.Zonal),    // 30,000 ≤ x < 60,000
            (60000m,    100000m - 0.01m, UserType.Regional),// 60,000 ≤ x < 1,00,000
            (100000m,   decimal.MaxValue, UserType.Nation), // ≥ 1,00,000
        };

        private static readonly Dictionary<UserType, decimal> _dailyCapByRank = new()
        {
            { UserType.Beginner, 12000m },
            { UserType.Area,     24000m },
            { UserType.Zonal,    36000m },
            { UserType.Regional, 48000m },
            { UserType.Nation,   60000m },
            { UserType.None,         0m },
        };
        private static readonly (Rank Rank, decimal MatchedRequired)[] RankByMatchedTeamSales =
        {
    (Rank.GlobalDirector, 10_000_000m),
    (Rank.Crown,           4_000_000m),
    (Rank.Diamond,         2_000_000m),
    (Rank.Pearl,           1_500_000m),
    (Rank.Star,            1_000_000m),
    (Rank.Gold,              700_000m),
    (Rank.Silver,            500_000m),
    (Rank.Rising,            300_000m),
    (Rank.Executive,         100_000m),
};


        private static int GetAllowedRepurchaseLevelsForPurchaser(UserType purchaserRank) => purchaserRank switch
        {
            UserType.Beginner => 3,  // 0..2  => 10, 8, 6
            UserType.Area => 4,  // 0..3  => 10, 8, 6, 5
            UserType.Zonal => 5,  // 0..4  => 10, 8, 6, 5, 4
            UserType.Regional => 7,  // 0..6  => 10, 8, 6, 5, 4, 3, 2
            UserType.Nation => 11, // 0..10 => 10, 8, 6, 5, 4, 3, 2, 1, 1, 1, 1
            _ => 0
        };

        private static readonly decimal[] RepurchasePercents = new decimal[]
        {
            0.10m, 0.08m, 0.06m, 0.05m, 0.04m, 0.03m, 0.02m, 0.01m, 0.01m, 0.01m, 0.01m
        };
        private static Rank GetRankForMatched(decimal matched)
        {
            foreach (var (rank, need) in RankByMatchedTeamSales)
                if (matched >= need) return rank;

            return Rank.None; // below Executive threshold
        }

        // Get the immediate child on a given side (Left/Right)
        private async Task<User?> GetSideRootAsync(string userId, NodePosition side)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => !u.IsDeleted && u.ParentId == userId && u.Position == side);
        }

        // Get ALL descendant user IDs starting from a given root user (BFS)
        private async Task<List<string>> GetDescendantIdsAsync(string rootUserId, bool includeRoot = false)
        {
            var all = await _context.Users
                    .Where(u => !u.IsDeleted)
                    .Select(u => new { u.Id, u.ParentId })
                    .ToListAsync();

            var byParent = all
                .Where(x => !string.IsNullOrEmpty(x.ParentId))
                .GroupBy(x => x.ParentId!)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Id).ToList());

            var result = new List<string>();
            var q = new Queue<string>();

            if (includeRoot) result.Add(rootUserId);
            q.Enqueue(rootUserId);

            while (q.Count > 0)
            {
                var curr = q.Dequeue();
                if (byParent.TryGetValue(curr, out var kids))
                {
                    foreach (var kid in kids)
                    {
                        result.Add(kid);
                        q.Enqueue(kid);
                    }
                }
            }

            return result;
        }

        // Sum team sales (Delivered orders) for a side of a user
        // If you prefer PV: replace o.TotalAmount with sum of ProductPoint*Quantity.
        private async Task<decimal> GetTeamSalesAsync(
            string userId,
            NodePosition side,
            DateTime? from = null,
            DateTime? to = null)
        {
            var sideRoot = await GetSideRootAsync(userId, side);
            if (sideRoot == null) return 0m;

            var teamIds = await GetDescendantIdsAsync(sideRoot.Id, includeRoot: true);
            if (teamIds.Count == 0) return 0m;

            var q = _context.Orders.AsQueryable();

            q = q.Where(o => !o.IsDeleted &&
                             o.Status == OrderStatus.Delivered &&
                             teamIds.Contains(o.UserId));

            if (from.HasValue) q = q.Where(o => o.OrderDate >= from.Value);
            if (to.HasValue) q = q.Where(o => o.OrderDate <= to.Value);

            return await q.SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;
        }


        private async Task<bool> UpdateUserRankFromTeamSalesAsync(
            User user, DateTime? from = null, DateTime? to = null)
        {
            var leftSales = await GetTeamSalesAsync(user.Id, NodePosition.Left, from, to);
            var rightSales = await GetTeamSalesAsync(user.Id, NodePosition.Right, from, to);

            var matched = Math.Min(leftSales, rightSales);
            var newRank = GetRankForMatched(matched);

            if (user.Rank != newRank)
            {
                user.Rank = newRank;
                _context.Users.Update(user);
                return true; // changed
            }
            return false; // unchanged
        }

        // Walk up the ancestry chain and update each ancestor's rank.
        // Use this after a sale event for buyerUserId.
        // Call this AFTER you confirm/record a sale (usually when order is Delivered)
        public async Task UpdateRanksUpChainAsync(string buyerUserId, DateTime? from = null, DateTime? to = null)
        {
            var buyer = await _context.Users
                .FirstOrDefaultAsync(u => !u.IsDeleted && u.Id == buyerUserId);
            if (buyer == null) return;

            // Collect parents (closest first)
            var ancestors = new List<User>();
            var cursor = buyer.ParentId;

            while (!string.IsNullOrEmpty(cursor))
            {
                var p = await _context.Users
                    .FirstOrDefaultAsync(u => !u.IsDeleted && u.Id == cursor);
                if (p == null) break;

                ancestors.Add(p);
                cursor = p.ParentId;
            }

            if (ancestors.Count == 0) return;

            var anyChanged = false;
            foreach (var ancestor in ancestors)
                anyChanged |= await UpdateUserRankFromTeamSalesAsync(ancestor, from, to);

            if (anyChanged)
                await _context.SaveChangesAsync();
        }



        private async Task PayRepurchaseAsync(User receiver, int level, decimal baseAmount, string purchaserName)
        {
            if (level < 0 || level >= RepurchasePercents.Length) return;

            var pct = RepurchasePercents[level];
            if (pct <= 0m) return;

            var bonus = Math.Round(baseAmount * pct, 2, MidpointRounding.AwayFromZero);
            if (bonus <= 0m) return;

            var reason = level == 0
                ? "Repurchase Commission (Self)"
                : $"Repurchase Commission L{level} from {purchaserName}";

            AddCommissionTransactionAsync(receiver, bonus, reason, purchaserName);
            await AddWalletTransactionAsync(receiver, bonus, reason.Replace("Commission", "Wallet Credit"), purchaserName);
        }

        public async Task ProcessCommissionOnSaleAsync(string userId, decimal saleAmount)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) throw new KeyNotFoundException("User not found");

            await ProcessDirectSponsorBonus(user, saleAmount);
            await ProcessIndirectSponsorBonus(user, saleAmount);
            await UpdateUserRank(user);



            var child = user;
            while (!string.IsNullOrEmpty(child.ParentId))
            {
                var parent = await _context.Users.FirstOrDefaultAsync(u => u.Id == child.ParentId);
                if (parent == null) break;
                await UpdateUserRank(parent);

                var buyerName = GetDisplayName(user);

                if (child.Position == NodePosition.Left)
                {
                    parent.LeftWallet += saleAmount;
                }
                else if (child.Position == NodePosition.Right)
                {
                    parent.RightWallet += saleAmount;
                }

                await ProcessBinaryCommission(parent, buyerName);

                child = parent;
            }

            await UpdateRanksUpChainAsync(userId);

            await _context.SaveChangesAsync();
        }

        private async Task ProcessDirectSponsorBonus(User user, decimal saleAmount)
        {
            if (!string.IsNullOrEmpty(user.ReferalId))
            {
                var referer = await _userManager.FindByIdAsync(user.ReferalId);
                if (referer != null)
                {
                    var bonus = saleAmount * 0.10m;
                    var buyerName = GetDisplayName(user);
                    AddCommissionTransactionAsync(referer, bonus, "Direct Sponsor Bonus", buyerName);
                    await AddWalletTransactionAsync(referer, bonus, "Direct Sponsor Bonus", buyerName);
                }
            }
        }

        private async Task ProcessIndirectSponsorBonus(User user, decimal saleAmount)
        {
            var level = 1;
            var current = user;
            var percents = new[] { 0.05m, 0.03m, 0.02m, 0.01m, 0.01m };
            var buyerName = GetDisplayName(user);

            while (!string.IsNullOrEmpty(current.ReferalId) && level <= 5)
            {
                var upline = await _userManager.FindByIdAsync(current.ReferalId);
                if (upline == null) break;

                var bonus = saleAmount * percents[level - 1];
                AddCommissionTransactionAsync(upline, bonus, $"Level {level} Indirect Sponsor Bonus", buyerName);
                await AddWalletTransactionAsync(upline, bonus, $"Level {level} Indirect Sponsor Bonus", buyerName);

                current = upline;
                level++;
            }
        }

        private async Task ProcessBinaryCommission(User parent, string triggeredByFullName)
        {
            var minPV = Math.Min(parent.LeftWallet, parent.RightWallet);
            if (minPV < 5000m) return;

            var pairs = (int)(minPV / 5000m);
            var commission = pairs * 600m;

            parent.LeftWallet -= pairs * 5000m;
            parent.RightWallet -= pairs * 5000m;

            AddCommissionTransactionAsync(parent, commission, "Binary Commission", triggeredByFullName);
            await AddWalletTransactionAsync(parent, commission, "Binary Wallet Credit", triggeredByFullName);


            var cap = GetDailyCapFor(parent);
            if (cap > 0m)
            {
                var todayTotal = await GetTodayBinaryTotalAsync(parent.Id);
                if (todayTotal >= cap)
                {
                    parent.LeftWallet = 0m;
                    parent.RightWallet = 0m;
                }
            }
        }




        public async Task DistributeRepurchaseCommissionAsync(string purchaserUserId, decimal repurchaseBase)
        {
            if (repurchaseBase <= 0m) return;

            var purchaser = await _userManager.FindByIdAsync(purchaserUserId)
                           ?? throw new KeyNotFoundException("Purchaser not found");

            // Make sure purchaser's rank is current
            await UpdateUserRank(purchaser);

            // Keep this value for commission depth, but DON'T early return
            var allowedLevels = GetAllowedRepurchaseLevelsForPurchaser(purchaser.Type);

            var purchaserName = GetDisplayName(purchaser);

            var current = purchaser;
            for (int level = 0; level < RepurchasePercents.Length; level++)
            {
                // Only pay if this node is eligible for this level (within their allowed depth)
                if (level < allowedLevels)
                {
                    await PayRepurchaseAsync(current, level, repurchaseBase, purchaserName);
                }

                // Move up the binary PARENT chain
                if (string.IsNullOrEmpty(current.ParentId)) break;

                var next = await _userManager.FindByIdAsync(current.ParentId);
                if (next == null) break;

                current = next;
            }

            await _context.SaveChangesAsync();
        }

    }
}

