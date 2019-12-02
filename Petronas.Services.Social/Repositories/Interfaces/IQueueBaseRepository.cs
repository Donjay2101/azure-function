using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Petronas.Services.Social.Repositories.Interfaces
{
    public interface IQueueBaseRepository
    {
        Task<CloudQueueMessage> AddMessage(string messageContent);
        Task<CloudQueueMessage> ReadMessage();
        Task<IEnumerable<CloudQueueMessage>> ReadMessages(int messageCount);
        Task<bool> DeleteMessage(CloudQueueMessage message);
    }
}
