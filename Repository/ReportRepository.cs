using Microsoft.EntityFrameworkCore;
using backend.Dto;
using backend.Data;
using backend.Interface.Repository;
using Microsoft.AspNetCore.Identity;
using backend.Models;

public class ReportRepository : IReportRepository
{
    private readonly UserManager<User> _userManager;
    private readonly AppDbContext _context;
    public ReportRepository(AppDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
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

        if (period == "month")
        {
            return await query
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new SalesByTimeDto
                {
                    Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                    TotalSales = g.Sum(x => x.TotalAmount)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();
        }
        else // day
        {
            return await query
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new SalesByTimeDto
                {
                    Date = g.Key,
                    TotalSales = g.Sum(x => x.TotalAmount)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();
        }

    }
    public async Task<List<DistributorAccountBalanceDto>> GetDistributorAccountBalancesAsync()
    {
        var distributors = await _context.Users
            .Where(u => u.TotalWallet > 0 || u.CommisionAmmount > 0)
            .Select(u => new DistributorAccountBalanceDto
            {
                DistributorId = u.Id,
                DistributorName = u.FullName,
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

        var totalSales = await orders.SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

        return new PersonalEarningsDto
        {
            DistributorId = user.Id,
            DistributorName = user.FullName,
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
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

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
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

            report.Add(new DistributorPerformanceDto
            {
                DistributorId = user.Id,
                DistributorName = user.FullName,
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
    public async Task<List<UserGrowthDto>> GetUserGrowthReportAsync(
    string period = "month", DateTime? from = null, DateTime? to = null)
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
        Func<User, DateTime> groupKeySelector;
        switch (period)
        {
            case "day":
                groupKeySelector = u => u.CreatedAt.Date;
                break;
            case "year":
                groupKeySelector = u => new DateTime(u.CreatedAt.Year, 1, 1);
                break;
            default:
                groupKeySelector = u => new DateTime(u.CreatedAt.Year, u.CreatedAt.Month, 1);
                break;
        }

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
                DistributorName = w.User.FullName,
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
                DistributorName = p.User.FullName,
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

        var summary = await payouts
            .GroupBy(p => new { p.UserId, p.User.FullName, p.User.Email })
            .Select(g => new CommissionPayoutSummaryDto
            {
                DistributorId = g.Key.UserId,
                DistributorName = g.Key.FullName,
                DistributorEmail = g.Key.Email!,
                TotalPaid = g.Where(p => p.Status == "Paid").Sum(p => (decimal?)p.Amount) ?? 0,
                TotalPending = g.Where(p => p.Status == "Pending").Sum(p => (decimal?)p.Amount) ?? 0,
                TotalFailed = g.Where(p => p.Status == "Failed").Sum(p => (decimal?)p.Amount) ?? 0,
                TotalPayouts = g.Count()
            })
            .OrderByDescending(s => s.TotalPaid)
            .ToListAsync();

        return summary;
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
                DistributorName = w.User.FullName,
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
        // Sales/Orders
        var orderQuery = _context.Orders.AsQueryable();
        if (userId != null) orderQuery = orderQuery.Where(o => o.UserId == userId);
        if (from != null) orderQuery = orderQuery.Where(o => o.OrderDate >= from);
        if (to != null) orderQuery = orderQuery.Where(o => o.OrderDate <= to);

        var orders = await (from o in orderQuery
                            join u in _context.Users on o.UserId equals u.Id
                            select new FullTransactionDto
                            {
                                TransactionType = "Sale",
                                TransactionId = o.Id,
                                UserId = o.UserId,
                                UserName = u.FullName,
                                UserEmail = u.Email!,
                                Amount = o.TotalAmount,
                                Date = o.OrderDate,
                                Status = "Completed",
                                Remarks = null
                            }).ToListAsync();

        // Commission Payouts
        var payoutQuery = _context.CommissionPayouts.AsQueryable();
        if (userId != null) payoutQuery = payoutQuery.Where(p => p.UserId == userId);
        if (from != null) payoutQuery = payoutQuery.Where(p => p.PayoutDate >= from);
        if (to != null) payoutQuery = payoutQuery.Where(p => p.PayoutDate <= to);

        var payouts = await (from p in payoutQuery
                             join u in _context.Users on p.UserId equals u.Id
                             select new FullTransactionDto
                             {
                                 TransactionType = "CommissionPayout",
                                 TransactionId = p.Id,
                                 UserId = p.UserId,
                                 UserName = u.FullName,
                                 UserEmail = u.Email!,
                                 Amount = p.Amount,
                                 Date = p.PayoutDate,
                                 Status = p.Status,
                                 Remarks = p.Remarks
                             }).ToListAsync();

        // Withdrawals
        var withdrawalQuery = _context.WithdrawalRequests.AsQueryable();
        if (userId != null) withdrawalQuery = withdrawalQuery.Where(w => w.UserId == userId);
        if (from != null) withdrawalQuery = withdrawalQuery.Where(w => w.RequestDate >= from);
        if (to != null) withdrawalQuery = withdrawalQuery.Where(w => w.RequestDate <= to);

        var withdrawals = await (from w in withdrawalQuery
                                 join u in _context.Users on w.UserId equals u.Id
                                 select new FullTransactionDto
                                 {
                                     TransactionType = "Withdrawal",
                                     TransactionId = w.Id,
                                     UserId = w.UserId,
                                     UserName = u.FullName,
                                     UserEmail = u.Email!,
                                     Amount = w.Amount,
                                     Date = w.RequestDate,
                                     Status = w.Status,
                                     Remarks = w.Remarks
                                 }).ToListAsync();

        // Combine all
        var allTransactions = orders
            .Concat(payouts)
            .Concat(withdrawals)
            .OrderByDescending(t => t.Date)
            .ToList();

        return allTransactions;
    }










}
