using System.Threading.Tasks;
using abc.Models;

namespace abc.Services
{
    public interface IQueueService
    {
        Task EnqueueOrderAsync(OrderMessage order);
        Task EnqueueInventoryAsync(InventoryMessage msg);
    }
}
