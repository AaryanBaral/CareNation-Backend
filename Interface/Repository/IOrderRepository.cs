
using backend.Models;

namespace backend.Interface.Repository
{
    public interface IOrderRepository
    {
    Task CreateOrderAsync(Order order);
    }
}