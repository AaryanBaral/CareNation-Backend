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

        // -------------------- Common helpers --------------------
        private static decimal Round2(decimal d) => Math.Round(d, 2, MidpointRounding.AwayFromZero);

        // Ledger + bucket updater
        private async Task AddPointsAsync(User user, decimal points, PointsType type, string note, string? sourceUserId = null, int? level = null)
        {
            if (points <= 0m) return;

            _context.PointsTransactions.Add(new PointsTransaction
            {
                UserId = user.Id,
                Type = type,
                Points = points,
                Note = note,
                SourceUserId = sourceUserId,
                Level = level
            });

            switch (type)
            {
                case PointsType.RepurchaseLevel: user.RepurchasePoints += points; break;
                case PointsType.RoyaltyFund: user.RoyaltyPoints += points; break;
                case PointsType.TravelFund: user.TravelPoints += points; break;
                case PointsType.CarFund: user.CarPoints += points; break;
                case PointsType.HouseFund: user.HousePoints += points; break;
                case PointsType.CompanyShare: user.CompanyPoints += points; break;
            }

            _context.Users.Update(user);
            await Task.CompletedTask;
        }

        private async Task<User> GetCompanyAccountAsync()
        {
            var company = await _context.Users.FirstOrDefaultAsync(u => !u.IsDeleted && u.IsCompanyAccount);
            if (company == null) throw new InvalidOperationException("Company account not found.");
            return company;
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




        // -------------------- FUNDS (POINTS) — remaining 58% => 8% company + 50% funds --------------------
        private static class FundWeights
        {
            // Royalty (20% pool): Silver 16%; Gold, Star, Pearl, Diamond, Crown, GlobalDirector = 14% each
            public static readonly Dictionary<Rank, decimal> Royalty = new()
        {
            { Rank.Silver,         0.16m },
            { Rank.Gold,           0.14m },
            { Rank.Star,           0.14m },
            { Rank.Pearl,          0.14m },
            { Rank.Diamond,        0.14m },
            { Rank.Crown,          0.14m },
            { Rank.GlobalDirector, 0.14m },
        };

            // Travel (10% pool): Gold 20; Star 19; Pearl 18; Diamond 17; Crown 16; GlobalDirector 10
            public static readonly Dictionary<Rank, decimal> Travel = new()
        {
            { Rank.Gold,           0.20m },
            { Rank.Star,           0.19m },
            { Rank.Pearl,          0.18m },
            { Rank.Diamond,        0.17m },
            { Rank.Crown,          0.16m },
            { Rank.GlobalDirector, 0.10m },
        };

            // Car (10% pool): 20% each for Star..GlobalDirector
            public static readonly Dictionary<Rank, decimal> Car = new()
        {
            { Rank.Star,           0.20m },
            { Rank.Pearl,          0.20m },
            { Rank.Diamond,        0.20m },
            { Rank.Crown,          0.20m },
            { Rank.GlobalDirector, 0.20m },
        };

            // House (10% pool): adjust as per your exact sheet; even split shown
            public static readonly Dictionary<Rank, decimal> House = new()
        {
            { Rank.Star,           0.20m },
            { Rank.Pearl,          0.20m },
            { Rank.Diamond,        0.20m },
            { Rank.Crown,          0.20m },
            { Rank.GlobalDirector, 0.20m },
        };
        }

        /// <summary>
        /// Splits a fund PV pool across ranks by weights, then equally among users in each rank.
        /// Unallocated (no users) goes to company points.
        /// Rounding is handled so total equals the pool.
        /// </summary>
        private async Task DistributeFundPointsAsync(
            PointsType fundType,
            string fundLabel,
            decimal fundPvPool,
            IReadOnlyDictionary<Rank, decimal> weights,
            string note)
        {
            if (fundPvPool <= 0) return;

            var company = await GetCompanyAccountAsync();

            foreach (var (rank, fraction) in weights)
            {
                if (fraction <= 0) continue;

                var rankBucket = Round2(fundPvPool * fraction);
                if (rankBucket <= 0) continue;

                var users = await _context.Users
                    .Where(u => !u.IsDeleted && u.Rank == rank)
                    .Select(u => u) // fetch entities to update points
                    .ToListAsync();

                if (users.Count == 0)
                {
                    await AddPointsAsync(company, rankBucket, fundType, $"{fundLabel} (Unallocated {rank})", null);
                    continue;
                }

                // equal split with remainder distribution to keep totals exact
                var baseShare = Math.Floor((rankBucket / users.Count) * 100m) / 100m; // 2dp floor
                var totalBase = baseShare * users.Count;
                var remainder = Round2(rankBucket - totalBase); // up to 0.99

                // give +0.01 to first N users until remainder is exhausted
                var cents = (int)Math.Round(remainder * 100m, 0, MidpointRounding.AwayFromZero);

                for (int i = 0; i < users.Count; i++)
                {
                    var bonus = baseShare + (i < cents ? 0.01m : 0m);
                    if (bonus <= 0) continue;
                    await AddPointsAsync(users[i], bonus, fundType, $"{fundLabel} - {rank}", null);
                }
            }
        }
        private static void ValidateFundWeights()
        {
            static void Check(string name, Dictionary<Rank, decimal> w)
            {
                var sum = w.Values.Sum();
                if (Math.Abs(sum - 1.00m) > 0.0001m)
                    throw new InvalidOperationException($"{name} weights must sum to 1.00 but are {sum}");
            }

            Check("Royalty", FundWeights.Royalty);
            Check("Travel", FundWeights.Travel);
            Check("Car", FundWeights.Car);
            Check("House", FundWeights.House);
        }



        /// <summary>
        /// After paying the 42% level chain, call this with the SAME PV base to split 58% into:
        /// 8% company + (20% Royalty, 10% Travel, 10% Car, 10% House) — all points.
        /// </summary>
        public async Task DistributeRepurchasePoints_FundsAndCompanyAsync(decimal repurchasePvBase, string? contextNote = null)
        {
            if (repurchasePvBase <= 0) return;

            ValidateFundWeights();

            var note = string.IsNullOrWhiteSpace(contextNote) ? "Repurchase PV Pools" : contextNote!.Trim();
            var company = await GetCompanyAccountAsync();

            // 8% company share (points)
            var companyPv = Round2(repurchasePvBase * 0.08m);
            if (companyPv > 0)
                await AddPointsAsync(company, companyPv, PointsType.CompanyShare, "Company Share 8% (PV)", null);

            // 50% into four funds (of the original PV base)
            var royaltyPool = Round2(repurchasePvBase * 0.20m);
            var travelPool = Round2(repurchasePvBase * 0.10m);
            var carPool = Round2(repurchasePvBase * 0.10m);
            var housePool = Round2(repurchasePvBase * 0.10m);

            await DistributeFundPointsAsync(PointsType.RoyaltyFund, "Royalty Fund (20%)", royaltyPool, FundWeights.Royalty, note);
            await DistributeFundPointsAsync(PointsType.TravelFund, "Travel Fund (10%)", travelPool, FundWeights.Travel, note);
            await DistributeFundPointsAsync(PointsType.CarFund, "Car Fund (10%)", carPool, FundWeights.Car, note);
            await DistributeFundPointsAsync(PointsType.HouseFund, "House Fund (10%)", housePool, FundWeights.House, note);

            await _context.SaveChangesAsync();
        }




        // -------------------- Convenience entrypoint for a repurchase --------------------
        /// <summary>
        /// Call this when a repurchase (PV) is confirmed.
        /// </summary>
        public async Task ProcessRepurchaseAsync(string purchaserUserId, decimal repurchasePvBase)
        {
            // 1) 42% chain points
            await DistributeRepurchaseCommissionAsync(purchaserUserId, repurchasePvBase);

            // 2) remaining 58% → 8% company + 50% funds (all points)
            await DistributeRepurchasePoints_FundsAndCompanyAsync(repurchasePvBase, $"Repurchase PV by {purchaserUserId}");

            // 3) (optional) bump ranks up the chain if your ranks depend on PV
            await UpdateRanksUpChainAsync(purchaserUserId);
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

            // PV points (not cash)
            var points = Math.Round(baseAmount * pct, 2, MidpointRounding.AwayFromZero);
            if (points <= 0m) return;

            var note = level == 0
                ? $"Repurchase PV (Self) from {purchaserName}"
                : $"Repurchase PV L{level} from {purchaserName}";

            // ✅ Store in CompanyPoints for this user (no wallet/commission records)
            await AddPointsAsync(
                user: receiver,
                points: points,
                type: PointsType.CompanyShare,  // puts it in user.CompanyPoints
                note: note,
                sourceUserId: null,
                level: level
            );
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

            var purchaserName = GetDisplayName(purchaser);

            var current = purchaser;
            for (int level = 0; level < RepurchasePercents.Length; level++)
            {
                // check eligibility of the current receiver (NOT purchaser)
                var allowedLevelsForCurrent = GetAllowedRepurchaseLevelsForPurchaser(current.Type);

                if (level < allowedLevelsForCurrent)
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

