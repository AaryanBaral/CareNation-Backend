
using backend.Dto;

namespace backend.Interface.Service
{
    public interface IOrderService
    {
        Task<int> PlaceOrderAsync(string userId);
    }
}