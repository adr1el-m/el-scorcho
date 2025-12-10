using System.Collections.Generic;
using System.Threading.Tasks;
using WHRID.Domain.Entities;

namespace WHRID.Application.Interfaces
{
    public interface IArgusRepository
    {
        Task<List<Transaction>> GetTransactionsByAddressAsync(string address, int limit = 50);
        Task<decimal> GetBalanceAsync(string address);
        Task<List<Transaction>> GetRecentTransactionsAsync(int limit = 10);
    }
}
