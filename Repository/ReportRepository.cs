using Microsoft.EntityFrameworkCore;
using backend.Dto;
using backend.Data;
using backend.Interface.Repository;
using Microsoft.AspNetCore.Identity;
using backend.Models;
using System.Linq;

public class ReportRepository : IReportRepository
{
    private readonly UserManager<User> _userManager;
    private readonly AppDbContext _context;
    public ReportRepository(AppDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Helper for in-memory name composition
    private static string BuildFullName(string? first, string? middle, string? last)
    {
        var parts = new[] { first, middle, last }
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s!.Trim());
        return string.Join(" ", parts);
    }

    public async Task<List<SalesByProductDto>> GetSalesByProductAsync(DateTime? from = null, DateTime? to = null)
    {
        var query = _context.OrderItems
            .Include(oi => oi.Product)
            .Include(oi => oi.Order)
            .AsQueryable();

        if (from != null)
            query = query.Where(oi => oi.Order.OrderDate >= from.Value);
        if (to != null)
            query = query.Where(oi => oi.Order.OrderDate <= to.Value);

        var result = await query
            .GroupBy(oi => new { oi.ProductId, oi.Product.Title })
            .Select(g => new SalesByProductDto
            {
                ProductId = g.Key.ProductId,
                ProductTitle = g.Key.Title,
                TotalQuantitySold = g.Sum(x => x.Quantity),
                TotalSales = g.Sum(x => x.Quantity * x.Price)
            })
            .OrderByDescending(x => x.TotalSales)
            .ToListAsync();

        return result;
    }

    public async Task<List<SalesByCategoryDto>> GetSalesByCategoryAsync(DateTime? from = null, DateTime? to = null)
    {
        var query = _context.OrderItems
            .Include(oi => oi.Product)
            .Include(oi => oi.Order)
            .AsQueryable();

        if (from != null)
            query = query.Where(oi => oi.Order.OrderDate >= from.Value);
        if (to != null)
            query = query.Where(oi => oi.Order.OrderDate <= to.Value);

        var result = await query
            .GroupBy(oi => oi.Product.CategoryId)
            .Select(g => new SalesByCategoryDto
            {
                CategoryId = g.Key,
                CategoryName = _context.Categories.Where(c => c.Id == g.Key).Select(c => c.Name).FirstOrDefault() ?? "",
                TotalQuantitySold = g.Sum(x => x.Quantity),
                TotalSales = g.Sum(x => x.Quantity * x.Price)
            })
            .OrderByDescending(x => x.TotalSales)
            .ToListAsync();

        return result;
    }

    public async Task<List<SalesByTimeDto>> GetSalesByTimeAsync(DateTime? from = null, DateTime? to = null, string period = "day")
    {
        var query = _context.Orders.AsQueryable();

        if (from != null)
            query = query.Where(o => o.OrderDate >= from.Value);
        if (to != null)
            query = query.Where(o => o.OrderDate <= to.Value);

        var data = await query
            .Select(o => new { o.OrderDate, o.TotalAmount })
            .ToListAsync(); // Fetch to memory

        List<SalesByTimeDto> result;

        switch (period.ToLower())
        {
            case "year":
                result = data
                    .GroupBy(o => o.OrderDate.Year)
                    .Select(g => new SalesByTimeDto
                    {
                        Date = new DateTime(g.Key, 1, 1),
                        TotalSales = g.Sum(x => x.TotalAmount)
                    })
                    .OrderBy(x => x.Date)
                    .ToList();
                break;

            case "quarter":
                result = data
                    .GroupBy(o => new
                    {
                        o.OrderDate.Year,
                        Quarter = (o.OrderDate.Month - 1) / 3 + 1
                    })
                    .Select(g => new SalesByTimeDto
                    {
                        Date = new DateTime(g.Key.Year, (g.Key.Quarter - 1) * 3 + 1, 1),
                        TotalSales = g.Sum(x => x.TotalAmount)
                    })
                    .OrderBy(x => x.Date)
                    .ToList();
                break;

            case "month":
                result = data
                    .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                    .Select(g => new SalesByTimeDto
                    {
                        Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                        TotalSales = g.Sum(x => x.TotalAmount)
                    })
                    .OrderBy(x => x.Date)
                    .ToList();
                break;

            case "day":
            default:
                result = data
                    .GroupBy(o => o.OrderDate.Date)
                    .Select(g => new SalesByTimeDto
                    {
                        Date = g.Key,
                        TotalSales = g.Sum(x => x.TotalAmount)
                    })
                    .OrderBy(x => x.Date)
                    .ToList();
                break;
        }

        return result;
    }

    public async Task<List<SalesByTimeDto>> GetSalesByTimeForDistributorAsync(string distributorId, DateTime? from = null, DateTime? to = null, string period = "day")
    {
        // Step 1: Get all downline user IDs including self
        var allUsers = await _context.Users.ToListAsync();
        var userIds = new List<string>();

        void Traverse(string parentId)
        {
            var children = allUsers.Where(u => u.ParentId == parentId).ToList();
            foreach (var child in children)
            {
                userIds.Add(child.Id);
                Traverse(child.Id);
            }
        }

        userIds.Add(distributorId); // include self
        Traverse(distributorId);

        // Step 2: Filter orders by those user IDs
        var query = _context.Orders.Where(o => userIds.Contains(o.UserId));

        if (from != null)
            query = query.Where(o => o.OrderDate >= from.Value);
        if (to != null)
            query = query.Where(o => o.OrderDate <= to.Value);

        var data = await query
            .Select(o => new { o.OrderDate, o.TotalAmount })
            .ToListAsync();

        List<SalesByTimeDto> result;

        switch (period.ToLower())
        {
            case "year":
                result = data
                    .GroupBy(o => o.OrderDate.Year)
                    .Select(g => new SalesByTimeDto
                    {
                        Date = new DateTime(g.Key, 1, 1),
                        TotalSales = g.Sum(x => x.TotalAmount)
                    })
                    .OrderBy(x => x.Date)
                    .ToList();
                break;

            case "quarter":
                result = data
                    .GroupBy(o => new { o.OrderDate.Year, Quarter = (o.OrderDate.Month - 1) / 3 + 1 })
                    .Select(g => new SalesByTimeDto
                    {
                        Date = new DateTime(g.Key.Year, (g.Key.Quarter - 1) * 3 + 1, 1),
                        TotalSales = g.Sum(x => x.TotalAmount)
                    })
                    .OrderBy(x => x.Date)
                    .ToList();
                break;

            case "month":
                result = data
                    .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                    .Select(g => new SalesByTimeDto
                    {
                        Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                        TotalSales = g.Sum(x => x.TotalAmount)
                    })
                    .OrderBy(x => x.Date)
                    .ToList();
                break;

            case "day":
            default:
                result = data
                    .GroupBy(o => o.OrderDate.Date)
                    .Select(g => new SalesByTimeDto
                    {
                        Date = g.Key,
                        TotalSales = g.Sum(x => x.TotalAmount)
                    })
                    .OrderBy(x => x.Date)
                    .ToList();
                break;
        }

        return result;
    }

    public async Task<List<DistributorAccountBalanceDto>> GetDistributorAccountBalancesAsync()
    {
        var distributors = await _context.Users
            .Where(u => u.TotalWallet > 0 || u.CommisionAmmount > 0)
            .Select(u => new DistributorAccountBalanceDto
            {
                DistributorId = u.Id,
                DistributorName = (u.FirstName + " " + (u.MiddleName ?? "") + " " + u.LastName).Trim(),
                DistributorEmail = u.Email!,
                TotalWallet = u.TotalWallet,
                CommissionAmount = u.CommisionAmmount
            })
            .ToListAsync();

        return distributors;
    }

    public async Task<List<ProductStockReportDto>> GetProductStockReportAsync(int reorderLevel = 10)
    {
        return await _context.Products
            .Select(p => new ProductStockReportDto
            {
                ProductId = p.Id,
                ProductTitle = p.Title,
                StockQuantity = p.StockQuantity,
                ReorderLevel = reorderLevel,
                NeedsReorder = p.StockQuantity <= reorderLevel
            })
            .OrderBy(p => p.StockQuantity)
            .ToListAsync();
    }

    public async Task<PersonalEarningsDto?> GetPersonalEarningsAsync(string distributorId, DateTime? from = null, DateTime? to = null)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == distributorId);
        if (user == null) return null;

        var orders = _context.Orders
            .Where(o => o.UserId == distributorId);

        if (from != null) orders = orders.Where(o => o.OrderDate >= from.Value);
        if (to != null) orders = orders.Where(o => o.OrderDate <= to.Value);

        var totalSales = await orders.SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

        return new PersonalEarningsDto
        {
            DistributorId = user.Id,
            DistributorName = BuildFullName(user.FirstName, user.MiddleName, user.LastName),
            DistributorEmail = user.Email!,
            TotalPersonalSales = totalSales,
            TotalCommission = user.CommisionAmmount,
            TotalWallet = user.TotalWallet
        };
    }

    public async Task<List<DistributorPerformanceDto>> GetDistributorPerformanceReportAsync(DateTime? from = null, DateTime? to = null)
    {
        var users = await _context.Users.ToListAsync();

        var report = new List<DistributorPerformanceDto>();

        foreach (var user in users)
        {
            // Direct recruits
            var directRecruits = users.Count(u => u.ReferalId == user.Id);

            // Total downline (recursive)
            int GetDownlineCount(string userId)
            {
                var direct = users.Where(u => u.ParentId == userId).ToList();
                return direct.Count + direct.Sum(d => GetDownlineCount(d.Id));
            }

            var totalDownline = GetDownlineCount(user.Id);

            // Personal sales
            var personalSales = await _context.Orders
                .Where(o => o.UserId == user.Id)
                .Where(o => !from.HasValue || o.OrderDate >= from)
                .Where(o => !to.HasValue || o.OrderDate <= to)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            // Team sales: sales from all downline (direct and indirect)
            var downlineIds = new List<string>();
            void CollectDownlineIds(string id)
            {
                var children = users.Where(u => u.ParentId == id).ToList();
                foreach (var child in children)
                {
                    downlineIds.Add(child.Id);
                    CollectDownlineIds(child.Id);
                }
            }
            CollectDownlineIds(user.Id);

            var teamSales = await _context.Orders
                .Where(o => downlineIds.Contains(o.UserId))
                .Where(o => !from.HasValue || o.OrderDate >= from)
                .Where(o => !to.HasValue || o.OrderDate <= to)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            report.Add(new DistributorPerformanceDto
            {
                DistributorId = user.Id,
                DistributorName = BuildFullName(user.FirstName, user.MiddleName, user.LastName),
                DistributorEmail = user.Email!,
                DirectRecruits = directRecruits,
                TotalDownline = totalDownline,
                PersonalSales = personalSales,
                TeamSales = teamSales,
                TotalCommission = user.CommisionAmmount
            });
        }

        return report;
    }

    public async Task<List<UserGrowthDto>> GetUserGrowthReportAsync(string period = "month", DateTime? from = null, DateTime? to = null)
    {
        var query = _context.Users.AsQueryable();

        if (from.HasValue)
            query = query.Where(u => u.CreatedAt >= from.Value);
        if (to.HasValue)
            query = query.Where(u => u.CreatedAt <= to.Value);

        var users = await query.ToListAsync();

        // Fetch roles for all users
        var userWithRoles = new List<(User User, IList<string> Roles)>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userWithRoles.Add((user, roles));
        }

        // Now group in-memory
        Func<User, DateTime> groupKeySelector = period switch
        {
            "day" => u => u.CreatedAt.Date,
            "year" => u => new DateTime(u.CreatedAt.Year, 1, 1),
            _ => u => new DateTime(u.CreatedAt.Year, u.CreatedAt.Month, 1),
        };

        var distributorRoleName = "Distributor";

        var report = userWithRoles
            .GroupBy(x => groupKeySelector(x.User))
            .Select(g => new UserGrowthDto
            {
                Period = g.Key,
                TotalNewUsers = g.Count(),
                TotalNewDistributors = g.Count(x => x.Roles.Contains(distributorRoleName))
            })
            .OrderBy(x => x.Period)
            .ToList();

        return report;
    }

    public async Task<List<WithdrawalRequestDto>> GetWithdrawalRequestsAsync(DateTime? from = null, DateTime? to = null, string? status = null)
    {
        var query = _context.WithdrawalRequests
            .Include(w => w.User)
            .AsQueryable();

        if (from != null)
            query = query.Where(w => w.RequestDate >= from.Value);
        if (to != null)
            query = query.Where(w => w.RequestDate <= to.Value);
        if (!string.IsNullOrEmpty(status))
            query = query.Where(w => w.Status == status);

        var list = await query
            .OrderByDescending(w => w.RequestDate)
            .Select(w => new WithdrawalRequestDto
            {
                Id = w.Id,
                DistributorId = w.UserId,
                DistributorName = (w.User.FirstName + " " + (w.User.MiddleName ?? "") + " " + w.User.LastName).Trim(),
                DistributorEmail = w.User.Email!,
                Amount = w.Amount,
                RequestDate = w.RequestDate,
                Status = w.Status,
                ProcessedDate = w.ProcessedDate,
                Remarks = w.Remarks
            })
            .ToListAsync();

        return list;
    }

    public async Task<List<CommissionPayoutDto>> GetCommissionPayoutsAsync(DateTime? from = null, DateTime? to = null, string? status = null)
    {
        var query = _context.CommissionPayouts
            .Include(p => p.User)
            .AsQueryable();

        if (from != null)
            query = query.Where(p => p.PayoutDate >= from.Value);
        if (to != null)
            query = query.Where(p => p.PayoutDate <= to.Value);
        if (!string.IsNullOrEmpty(status))
            query = query.Where(p => p.Status == status);

        var list = await query
            .OrderByDescending(p => p.PayoutDate)
            .Select(p => new CommissionPayoutDto
            {
                Id = p.Id,
                DistributorId = p.UserId,
                DistributorName = (p.User.FirstName + " " + (p.User.MiddleName ?? "") + " " + p.User.LastName).Trim(),
                DistributorEmail = p.User.Email!,
                Amount = p.Amount,
                PayoutDate = p.PayoutDate,
                Status = p.Status,
                Remarks = p.Remarks
            })
            .ToListAsync();

        return list;
    }

    public async Task<List<CommissionPayoutSummaryDto>> GetCommissionPayoutSummaryAsync(DateTime? from = null, DateTime? to = null)
    {
        var payouts = _context.CommissionPayouts
            .Include(p => p.User)
            .AsQueryable();

        if (from.HasValue)
            payouts = payouts.Where(p => p.PayoutDate >= from.Value);
        if (to.HasValue)
            payouts = payouts.Where(p => p.PayoutDate <= to.Value);

        // Group by UserId + name parts to avoid grouping by a computed string
        var summary = await payouts
            .GroupBy(p => new
            {
                p.UserId,
                p.User.FirstName,
                p.User.MiddleName,
                p.User.LastName,
                p.User.Email
            })
            .Select(g => new
            {
                g.Key.UserId,
                g.Key.FirstName,
                g.Key.MiddleName,
                g.Key.LastName,
                g.Key.Email,
                TotalPaid = g.Where(p => p.Status == "Paid").Sum(p => (decimal?)p.Amount) ?? 0,
                TotalPending = g.Where(p => p.Status == "Pending").Sum(p => (decimal?)p.Amount) ?? 0,
                TotalFailed = g.Where(p => p.Status == "Failed").Sum(p => (decimal?)p.Amount) ?? 0,
                TotalPayouts = g.Count()
            })
            .OrderByDescending(s => s.TotalPaid)
            .ToListAsync();

        // Compose name in memory
        return summary.Select(s => new CommissionPayoutSummaryDto
        {
            DistributorId = s.UserId,
            DistributorName = BuildFullName(s.FirstName, s.MiddleName, s.LastName),
            DistributorEmail = s.Email!,
            TotalPaid = s.TotalPaid,
            TotalPending = s.TotalPending,
            TotalFailed = s.TotalFailed,
            TotalPayouts = s.TotalPayouts
        }).ToList();
    }

    public async Task<List<WithdrawalTransactionDto>> GetWithdrawalTransactionsAsync(DateTime? from = null, DateTime? to = null)
    {
        var query = _context.WithdrawalRequests
            .Include(w => w.User)
            .AsQueryable();

        if (from != null)
            query = query.Where(w => w.RequestDate >= from.Value);
        if (to != null)
            query = query.Where(w => w.RequestDate <= to.Value);

        return await query
            .OrderByDescending(w => w.RequestDate)
            .Select(w => new WithdrawalTransactionDto
            {
                Id = w.Id,
                DistributorId = w.UserId,
                DistributorName = (w.User.FirstName + " " + (w.User.MiddleName ?? "") + " " + w.User.LastName).Trim(),
                DistributorEmail = w.User.Email!,
                Amount = w.Amount,
                RequestDate = w.RequestDate,
                Status = w.Status,
                ProcessedDate = w.ProcessedDate,
                Remarks = w.Remarks
            })
            .ToListAsync();
    }
    public async Task<List<FullTransactionDto>> GetFullTransactionalReportAsync(
        string? userId = null, DateTime? from = null, DateTime? to = null)
    {
        // Orders
        var orderQuery = _context.Orders.AsQueryable();
        if (!string.IsNullOrEmpty(userId)) orderQuery = orderQuery.Where(o => o.UserId == userId);
        if (from.HasValue) orderQuery = orderQuery.Where(o => o.OrderDate >= from.Value);
        if (to.HasValue) orderQuery = orderQuery.Where(o => o.OrderDate <= to.Value);

        var orders = await
            (from o in orderQuery
             join u in _context.Users on o.UserId equals u.Id
             select new FullTransactionDto
             {
                 TransactionType = "Sale",
                 TransactionId = o.Id,
                 UserId = o.UserId,
                 UserName = (u.FirstName + " " + (u.MiddleName ?? "") + " " + u.LastName).Trim(),
                 UserEmail = u.Email ?? "",
                 Amount = o.TotalAmount,
                 Date = o.OrderDate,
                 Status = "Completed",
                 Remarks = null
             })
            .ToListAsync();

        // Commission Payouts
        var payoutQuery = _context.CommissionPayouts.AsQueryable();
        if (!string.IsNullOrEmpty(userId)) payoutQuery = payoutQuery.Where(p => p.UserId == userId);
        if (from.HasValue) payoutQuery = payoutQuery.Where(p => p.PayoutDate >= from.Value);
        if (to.HasValue) payoutQuery = payoutQuery.Where(p => p.PayoutDate <= to.Value);

        var payouts = await
            (from p in payoutQuery
             join u in _context.Users on p.UserId equals u.Id
             select new FullTransactionDto
             {
                 TransactionType = "Commission Payout",
                 TransactionId = p.Id,
                 UserId = p.UserId,
                 UserName = (u.FirstName + " " + (u.MiddleName ?? "") + " " + u.LastName).Trim(),
                 UserEmail = u.Email ?? "",
                 Amount = p.Amount,
                 Date = p.PayoutDate,
                 Status = p.Status,
                 Remarks = p.Remarks
             })
            .ToListAsync();

        // Withdrawals
        var withdrawalQuery = _context.WithdrawalRequests.AsQueryable();
        if (!string.IsNullOrEmpty(userId)) withdrawalQuery = withdrawalQuery.Where(w => w.UserId == userId);
        if (from.HasValue) withdrawalQuery = withdrawalQuery.Where(w => w.RequestDate >= from.Value);
        if (to.HasValue) withdrawalQuery = withdrawalQuery.Where(w => w.RequestDate <= to.Value);

        var withdrawals = await
            (from w in withdrawalQuery
             join u in _context.Users on w.UserId equals u.Id
             select new FullTransactionDto
             {
                 TransactionType = "Withdrawal",
                 TransactionId = w.Id,
                 UserId = w.UserId,
                 UserName = (u.FirstName + " " + (u.MiddleName ?? "") + " " + u.LastName).Trim(),
                 UserEmail = u.Email ?? "",
                 Amount = w.Amount,
                 Date = w.RequestDate,
                 Status = w.Status,
                 Remarks = w.Remarks
             })
            .ToListAsync();

        // Balance Transfers
        var transferQuery = _context.BalanceTransfers
            .Include(t => t.Sender)
            .Include(t => t.Receiver)
            .AsQueryable();

        if (!string.IsNullOrEmpty(userId))
            transferQuery = transferQuery.Where(t => t.SenderId == userId || t.ReceiverId == userId);

        if (from.HasValue) transferQuery = transferQuery.Where(t => t.TransferDate >= from.Value);
        if (to.HasValue) transferQuery = transferQuery.Where(t => t.TransferDate <= to.Value);

        var transfers = await transferQuery
            .Select(t => new FullTransactionDto
            {
                TransactionType = "Balance Transfer",
                TransactionId = t.Id,
                UserId = t.SenderId,
                UserName = (t.Sender.FirstName + " " + (t.Sender.MiddleName ?? "") + " " + t.Sender.LastName).Trim(),
                UserEmail = t.Sender.Email ?? "",
                Amount = t.Amount,
                Date = t.TransferDate,
                Status = "Completed",
                Remarks =
                    "Sent from " +
                    (t.Sender.FirstName + " " + (t.Sender.MiddleName ?? "") + " " + t.Sender.LastName).Trim() +
                    " to " +
                    (t.Receiver.FirstName + " " + (t.Receiver.MiddleName ?? "") + " " + t.Receiver.LastName).Trim() +
                    ", remarks: " + t.Remarks
            })
            .ToListAsync();

        // Combine All
        return orders
            .Concat(payouts)
            .Concat(withdrawals)
            .Concat(transfers)
            .OrderByDescending(t => t.Date)
            .ToList();
    }




    public async Task<IEnumerable<CommissionPayoutDto>> GetCommissionPayoutsByDistributorAsync(string userId, DateTime? from, DateTime? to, string? status)
    {
        var query = _context.CommissionPayouts
            .Where(p => p.UserId == userId);

        if (from.HasValue) query = query.Where(p => p.PayoutDate >= from);
        if (to.HasValue) query = query.Where(p => p.PayoutDate <= to);
        if (!string.IsNullOrEmpty(status)) query = query.Where(p => p.Status == status);

        return await query.Select(p => new CommissionPayoutDto
        {
            Id = p.Id,
            DistributorId = p.Id == 0 ? p.UserId : p.UserId, // unchanged, explicit
            DistributorName = (p.User.FirstName + " " + (p.User.MiddleName ?? "") + " " + p.User.LastName).Trim(),
            DistributorEmail = p.User.Email!,
            Amount = p.Amount,
            PayoutDate = p.PayoutDate,
            Status = p.Status,
            Remarks = p.Remarks
        }).ToListAsync();
    }

    public async Task<CommissionPayoutSummaryDto> GetCommissionPayoutSummaryByDistributorAsync(string userId, DateTime? from, DateTime? to)
    {
        var query = _context.CommissionPayouts
            .Include(u => u.User)
            .Where(p => p.UserId == userId);

        if (from.HasValue) query = query.Where(p => p.PayoutDate >= from);
        if (to.HasValue) query = query.Where(p => p.PayoutDate <= to);

        var payouts = await query.ToListAsync();

        return new CommissionPayoutSummaryDto
        {
            DistributorId = userId,
            DistributorName = BuildFullName(payouts.FirstOrDefault()?.User.FirstName, payouts.FirstOrDefault()?.User.MiddleName, payouts.FirstOrDefault()?.User.LastName),
            DistributorEmail = payouts.FirstOrDefault()?.User.Email!,
            TotalPaid = payouts.Where(p => p.Status == "Paid").Sum(p => p.Amount),
            TotalPending = payouts.Where(p => p.Status == "Pending").Sum(p => p.Amount),
            TotalFailed = payouts.Where(p => p.Status == "Failed").Sum(p => p.Amount),
            TotalPayouts = payouts.Count
        };
    }

    public async Task<IEnumerable<WithdrawalRequestDto>> GetWithdrawalRequestsByDistributorAsync(string userId, DateTime? from, DateTime? to, string? status)
    {
        var query = _context.WithdrawalRequests.Where(w => w.UserId == userId);

        if (from.HasValue) query = query.Where(w => w.RequestDate >= from);
        if (to.HasValue) query = query.Where(w => w.RequestDate <= to);
        if (!string.IsNullOrEmpty(status)) query = query.Where(w => w.Status == status);

        return await query.Select(w => new WithdrawalRequestDto
        {
            Id = w.Id,
            DistributorId = w.UserId,
            DistributorName = (w.User.FirstName + " " + (w.User.MiddleName ?? "") + " " + w.User.LastName).Trim(),
            DistributorEmail = w.User.Email!,
            Amount = w.Amount,
            RequestDate = w.RequestDate,
            Status = w.Status,
            ProcessedDate = w.ProcessedDate,
            Remarks = w.Remarks
        }).ToListAsync();
    }

    public async Task<IEnumerable<WithdrawalTransactionDto>> GetWithdrawalTransactionsByDistributorAsync(string userId, DateTime? from, DateTime? to)
    {
        var query = _context.WithdrawalRequests.Where(w => w.UserId == userId);

        if (from.HasValue) query = query.Where(w => w.RequestDate >= from);
        if (to.HasValue) query = query.Where(w => w.RequestDate <= to);

        return await query.Select(w => new WithdrawalTransactionDto
        {
            Id = w.Id,
            DistributorId = w.UserId,
            DistributorName = (w.User.FirstName + " " + (w.User.MiddleName ?? "") + " " + w.User.LastName).Trim(),
            DistributorEmail = w.User.Email!,
            Amount = w.Amount,
            RequestDate = w.RequestDate,
            Status = w.Status,
            ProcessedDate = w.ProcessedDate,
            Remarks = w.Remarks
        }).ToListAsync();
    }

    public async Task<TotalSalesDto?> GetTotalSalesByDistributorAsync(string distributorId, DateTime? from = null, DateTime? to = null)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == distributorId);
        if (user == null) return null;

        var allUsers = await _context.Users.ToListAsync();

        // Step 1: Collect downline user IDs (recursively)
        var downlineIds = new List<string>();
        void CollectDownlines(string id)
        {
            var children = allUsers.Where(u => u.ParentId == id).ToList();
            foreach (var child in children)
            {
                downlineIds.Add(child.Id);
                CollectDownlines(child.Id);
            }
        }
        CollectDownlines(distributorId);

        // Step 2: Personal sales
        var personalSalesQuery = _context.Orders.Where(o => o.UserId == distributorId);
        if (from.HasValue) personalSalesQuery = personalSalesQuery.Where(o => o.OrderDate >= from.Value);
        if (to.HasValue) personalSalesQuery = personalSalesQuery.Where(o => o.OrderDate <= to.Value);
        var personalSales = await personalSalesQuery.SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

        // Step 3: Team sales
        var teamSalesQuery = _context.Orders.Where(o => downlineIds.Contains(o.UserId));
        if (from.HasValue) teamSalesQuery = teamSalesQuery.Where(o => o.OrderDate >= from.Value);
        if (to.HasValue) teamSalesQuery = teamSalesQuery.Where(o => o.OrderDate <= to.Value);
        var teamSales = await teamSalesQuery.SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

        // Step 4: Return combined DTO
        return new TotalSalesDto
        {
            DistributorId = distributorId,
            DistributorName = BuildFullName(user.FirstName, user.MiddleName, user.LastName),
            DistributorEmail = user.Email!,
            PersonalSales = personalSales,
            TeamSales = teamSales
        };
    }
}
