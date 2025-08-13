using backend.Models;
using backend.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using backend.Interface.Repository;
using backend.Dto;
using System.Linq;

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

        // Mirrors your slide table
        private sealed record RankRewardCfg(
            string Rank, decimal Milestone, string RewardItem,
            decimal? RoyaltyPct, decimal? TravelPct, decimal? CarPct, decimal? HousePct);

        private static readonly RankRewardCfg[] _rankRewards =
        {
            new("Executive",       100000m,   "CNI Bag",          null,  null,  null,  null),
            new("Rising",          300000m,   "Alkaline bottle",  null,  null,  null,  null),
            new("Silver",          500000m,   "Suit/Sari",        0.16m, null,  null,  null),
            new("Gold",            700000m,   "Bio Bracelet",     0.14m, 0.20m, null,  null),
            new("Star",           1000000m,   "BMI Machine",      0.14m, 0.19m, 0.20m, null),
            new("Pearl",          1500000m,   "Alkaline Filter",  0.14m, 0.18m, 0.20m, 0.25m),
            new("Diamond",        2000000m,   "Bio Watch",        0.14m, 0.17m, 0.20m, 0.25m),
            new("Crown",          4000000m,   "Laptop",           0.14m, 0.16m, 0.20m, 0.25m),
            new("Global Director",10000000m,  "Cash 1 lakh",      0.14m, 0.10m, 0.20m, 0.25m),
        };

        private static readonly (decimal min, decimal max, Rank rank)[] _rankByPurchase =
        {
            (5000m,     15000m - 0.01m, Rank.Beginner), // 5,000 ≤ x < 15,000
            (15000m,    30000m - 0.01m, Rank.Area),     // 15,000 ≤ x < 30,000
            (30000m,    60000m - 0.01m, Rank.Zonal),    // 30,000 ≤ x < 60,000
            (60000m,    100000m - 0.01m, Rank.Regional),// 60,000 ≤ x < 1,00,000
            (100000m,   decimal.MaxValue, Rank.Nation), // ≥ 1,00,000
        };

        private static readonly Dictionary<Rank, decimal> _dailyCapByRank = new()
        {
            { Rank.Beginner, 12000m },
            { Rank.Area,     24000m },
            { Rank.Zonal,    36000m },
            { Rank.Regional, 48000m },
            { Rank.Nation,   60000m },
            { Rank.None,         0m },
        };

        private static int GetAllowedRepurchaseLevels(Rank rank) => rank switch
        {
            Rank.Beginner => 3, // levels 0..2
            Rank.Area => 4, // 0..3
            Rank.Zonal => 5, // 0..4
            Rank.Regional => 7, // 0..6
            Rank.Nation => 11,// 0..10
            _ => 0  // Rank.None: not eligible
        };

        private static readonly decimal[] RepurchasePercents = new decimal[]
        {
            0.10m, 0.08m, 0.06m, 0.05m, 0.04m, 0.03m, 0.02m, 0.01m, 0.01m, 0.01m, 0.01m
        };

        private async Task PayRepurchaseAsync(User receiver, int level, decimal baseAmount, string purchaserName)
        {
            var allowed = GetAllowedRepurchaseLevels(receiver.Rank);
            if (level > allowed - 1) return;
            if (level < 0 || level >= RepurchasePercents.Length) return;

            var pct = RepurchasePercents[level];
            if (pct <= 0m) return;

            var bonus = Math.Round(baseAmount * pct, 2, MidpointRounding.AwayFromZero);
            if (bonus <= 0m) return;

            var reason = level == 0
                ? "Repurchase Commission (Self)"
                : $"Repurchase Commission L{level} from {purchaserName}";

            await AddCommissionTransactionAsync(receiver, bonus, reason, purchaserName);
            await AddWalletTransactionAsync(receiver, bonus, reason.Replace("Commission", "Wallet Credit"), purchaserName);
        }

        public async Task ProcessRankRewardsOnOrderAsync(string buyerUserId, decimal orderAmount)
        {
            var child = await _context.Users.FirstOrDefaultAsync(u => u.Id == buyerUserId);
            if (child == null) throw new KeyNotFoundException("Buyer not found");

            while (!string.IsNullOrEmpty(child.ParentId))
            {
                var parent = await _context.Users.FirstOrDefaultAsync(u => u.Id == child.ParentId);
                if (parent == null) break;

                var prog = await _context.Set<TeamSalesProgress>().FindAsync(parent.Id);
                if (prog == null)
                {
                    prog = new TeamSalesProgress { UserId = parent.Id };
                    _context.Add(prog);
                }

                if (child.Position == NodePosition.Left) prog.LeftTeamSales += orderAmount;
                else prog.RightTeamSales += orderAmount;

                var matched = Math.Min(prog.LeftTeamSales, prog.RightTeamSales);

                var newlyHit = _rankRewards
                    .Where(cfg => cfg.Milestone > prog.MatchedVolumeConsumed &&
                                  cfg.Milestone <= matched)
                    .OrderBy(cfg => cfg.Milestone)
                    .ToList();

                foreach (var cfg in newlyHit)
                {
                    decimal milestone = cfg.Milestone;
                    decimal royalty = cfg.RoyaltyPct.HasValue ? Math.Round(milestone * cfg.RoyaltyPct.Value, 2) : 0m;
                    decimal travel = cfg.TravelPct.HasValue ? Math.Round(milestone * cfg.TravelPct.Value, 2) : 0m;
                    decimal car = cfg.CarPct.HasValue ? Math.Round(milestone * cfg.CarPct.Value, 2) : 0m;
                    decimal house = cfg.HousePct.HasValue ? Math.Round(milestone * cfg.HousePct.Value, 2) : 0m;

                    _context.Add(new RewardPayout
                    {
                        UserId = parent.Id,
                        MilestoneAmount = milestone,
                        RankLabel = cfg.Rank,
                        RewardItem = cfg.RewardItem,
                        RoyaltyAmount = royalty,
                        TravelFundAmount = travel,
                        CarFundAmount = car,
                        HouseFundAmount = house,
                        PayoutDate = DateTime.UtcNow
                    });

                    if (royalty > 0)
                    {
                        await AddCommissionTransactionAsync(parent, royalty, $"Rank Reward Royalty ({cfg.Rank} {milestone:N0})");
                        await AddWalletTransactionAsync(parent, royalty, $"Rank Reward Royalty ({cfg.Rank} {milestone:N0})");
                    }

                    if (travel > 0) await AddFundContributionAsync(parent.Id, FundType.Travel, travel, cfg.Rank);
                    if (car > 0) await AddFundContributionAsync(parent.Id, FundType.Car, car, cfg.Rank);
                    if (house > 0) await AddFundContributionAsync(parent.Id, FundType.House, house, cfg.Rank);
                }

                if (newlyHit.Count > 0)
                    prog.MatchedVolumeConsumed = matched;

                await _context.SaveChangesAsync();

                child = parent;
            }
        }

        private Task AddFundContributionAsync(string userId, FundType type, decimal amount, string rankName)
        {
            if (amount <= 0m) return Task.CompletedTask;

            _context.FundContributions.Add(new FundContribution
            {
                UserId = userId,
                Type = type,
                Amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero),
                ContributionDate = DateTime.UtcNow,
                Remarks = $"Fund allocation for rank {rankName}"
            });

            return Task.CompletedTask;
        }

        /// <summary>
        /// Distribute repurchase commission for a repurchase PV/amount.
        /// level 0 = purchaser (10%), level 1 = direct upline (8%), etc.
        /// </summary>
        public async Task DistributeRepurchaseCommissionAsync(string purchaserUserId, decimal repurchaseBase)
        {
            if (repurchaseBase <= 0m) return;

            var purchaser = await _userManager.FindByIdAsync(purchaserUserId)
                           ?? throw new KeyNotFoundException("Purchaser not found");

            await UpdateUserRank(purchaser);

            var purchaserName = GetDisplayName(purchaser);

            await PayRepurchaseAsync(purchaser, level: 0, baseAmount: repurchaseBase, purchaserName: purchaserName);

            var current = purchaser;
            for (int level = 1; level < RepurchasePercents.Length; level++)
            {
                if (string.IsNullOrEmpty(current.ParentId)) break;

                var upline = await _userManager.FindByIdAsync(current.ParentId);
                if (upline == null) break;

                await UpdateUserRank(upline);

                await PayRepurchaseAsync(upline, level, repurchaseBase, purchaserName);

                current = upline;
            }

            await _context.SaveChangesAsync();
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
            return _dailyCapByRank.TryGetValue(u.Rank, out var cap) ? cap : 0m;
        }

        private async Task<Rank> GetRankFromOwnPurchasesAsync(string userId)
        {
            var ownPurchase = await _context.Orders
                .Where(o => o.UserId == userId)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

            foreach (var (min, max, r) in _rankByPurchase)
            {
                if (ownPurchase >= min && ownPurchase <= max) return r;
            }
            return Rank.None;
        }

        private async Task AddCommissionTransactionAsync(User user, decimal amount, string reason, string? triggeredByFullName = null)
        {
            user.CommisionAmmount += amount;
            await _commissionPayoutRepository.AddAsync(new CommissionPayout
            {
                UserId = user.Id,
                PayoutDate = DateTime.UtcNow,
                Amount = amount,
                Remarks = $"{reason}{(triggeredByFullName != null ? $" (by {triggeredByFullName})" : "")}"
            });
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
            await AddCommissionTransactionAsync(user, commission, "Manual commission adjustment", name);
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

                var buyerName = GetDisplayName(user);

                if (child.Position == NodePosition.Left)
                {
                    parent.LeftWallet += saleAmount;
                    await AddWalletTransactionAsync(parent, saleAmount, "Left Wallet Increment", buyerName);
                }
                else if (child.Position == NodePosition.Right)
                {
                    parent.RightWallet += saleAmount;
                    await AddWalletTransactionAsync(parent, saleAmount, "Right Wallet Increment", buyerName);
                }

                await ProcessBinaryCommission(parent, buyerName);
                await ProcessLeadershipBonus(parent);
                await ProcessRankAchievementBonus(parent);

                child = parent;
            }

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
                    await AddCommissionTransactionAsync(referer, bonus, "Direct Sponsor Bonus", buyerName);
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
                await AddCommissionTransactionAsync(upline, bonus, $"Level {level} Indirect Sponsor Bonus", buyerName);
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

            await AddCommissionTransactionAsync(parent, commission, "Binary Commission", triggeredByFullName);
            await AddWalletTransactionAsync(parent, commission, "Binary Wallet Credit", triggeredByFullName);

            if (!string.IsNullOrEmpty(parent.ParentId))
                await ProcessMatchingBonus(parent.ParentId, commission, triggeredByFullName);

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

        private async Task ProcessMatchingBonus(string beneficiaryUserId, decimal binaryCommission, string triggeredByFullName)
        {
            var beneficiary = await _userManager.FindByIdAsync(beneficiaryUserId);
            if (beneficiary == null || string.IsNullOrEmpty(beneficiary.ReferalId)) return;

            var sponsor = await _userManager.FindByIdAsync(beneficiary.ReferalId);
            if (sponsor == null) return;

            decimal matchPercent = 0.10m;
            decimal bonus = binaryCommission * matchPercent;

            await AddWalletTransactionAsync(sponsor, bonus, "Matching Bonus", triggeredByFullName);
            await AddCommissionTransactionAsync(sponsor, bonus, "Matching Bonus", triggeredByFullName);

            var matchPayout = new CommissionPayout
            {
                UserId = sponsor.Id,
                PayoutDate = DateTime.UtcNow,
                Amount = bonus,
                Remarks = $"Matching bonus from {triggeredByFullName}'s Rs. {binaryCommission} binary income"
            };
            await _commissionPayoutRepository.AddAsync(matchPayout);
        }

        private async Task ProcessLeadershipBonus(User user)
        {
            if (!user.LeadershipBonusGiven && user.Rank >= Rank.Zonal)
            {
                var name = GetDisplayName(user);
                await AddWalletTransactionAsync(user, 5000, "Leadership Bonus", name);
                await AddCommissionTransactionAsync(user, 5000, "Leadership Bonus", name);
                user.LeadershipBonusGiven = true;
            }
        }

        private async Task ProcessRankAchievementBonus(User user)
        {
            if (user.RankBonusGiven) return;

            var rankBonusMap = new Dictionary<Rank, decimal>
            {
                { Rank.Beginner, 5000 },
                { Rank.Area, 10000 },
                { Rank.Zonal, 20000 },
                { Rank.Regional, 50000 },
                { Rank.Nation, 50000 }
            };

            if (user.Rank > user.LastRankAwarded && rankBonusMap.ContainsKey(user.Rank))
            {
                var bonus = rankBonusMap[user.Rank];
                var name = GetDisplayName(user);
                await AddWalletTransactionAsync(user, bonus, "Rank Achievement Bonus", name);
                await AddCommissionTransactionAsync(user, bonus, "Rank Achievement Bonus", name);
                user.LastRankAwarded = user.Rank;
            }
        }

        // REPLACE your current UpdateUserRank with this:
        private async Task UpdateUserRank(User user)
        {
            user.Rank = await GetRankFromOwnPurchasesAsync(user.Id);
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

        private async Task ProcessRepurchaseBonus(string purchaserUserId, decimal pvAmount)
        {
            var purchaser = await _userManager.FindByIdAsync(purchaserUserId);
            if (purchaser == null || string.IsNullOrEmpty(purchaser.ReferalId)) return;

            var current = purchaser;
            var percentages = new[] { 0.10m, 0.05m, 0.03m, 0.02m, 0.01m };
            var buyerName = GetDisplayName(purchaser);

            for (int level = 0; level < percentages.Length; level++)
            {
                if (string.IsNullOrEmpty(current.ReferalId)) break;

                var upline = await _userManager.FindByIdAsync(current.ReferalId);
                if (upline == null) break;

                decimal bonus = pvAmount * percentages[level];
                await AddWalletTransactionAsync(upline, bonus, $"Level {level + 1} Repurchase Bonus", buyerName);
                await AddCommissionTransactionAsync(upline, bonus, $"Level {level + 1} Repurchase Bonus", buyerName);

                await _commissionPayoutRepository.AddAsync(new CommissionPayout
                {
                    UserId = upline.Id,
                    PayoutDate = DateTime.UtcNow,
                    Amount = bonus,
                    Remarks = $"Level {level + 1} Repurchase bonus from {buyerName}'s repurchase"
                });

                current = upline;
            }
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
    }
}
