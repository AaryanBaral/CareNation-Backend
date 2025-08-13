using System.Linq;
using backend.Models;

public static class WithdrawalRequestMapper
{
    private static string BuildFullName(User u)
    {
        var parts = new[] { u.FirstName, u.MiddleName, u.LastName }
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s!.Trim());
        return string.Join(" ", parts);
    }

    public static WithdrawalRequestDto ToWithdrawalRequestDto(this WithdrawalRequest model)
    {
        return new WithdrawalRequestDto
        {
            Id = model.Id,
            DistributorId = model.UserId,
            DistributorName = BuildFullName(model.User),
            DistributorEmail = model.User.Email ?? string.Empty,
            Amount = model.Amount,
            RequestDate = model.RequestDate,
            Status = model.Status,
            ProcessedDate = model.ProcessedDate,
            Remarks = model.Remarks
        };
    }

    public static WithdrawalTransactionDto ToWithdrawalTransactionDto(this WithdrawalRequest model)
    {
        return new WithdrawalTransactionDto
        {
            Id = model.Id,
            DistributorId = model.UserId,
            DistributorName = BuildFullName(model.User),
            DistributorEmail = model.User.Email ?? string.Empty,
            Amount = model.Amount,
            RequestDate = model.RequestDate,
            Status = model.Status,
            ProcessedDate = model.ProcessedDate,
            Remarks = model.Remarks
        };
    }
}
