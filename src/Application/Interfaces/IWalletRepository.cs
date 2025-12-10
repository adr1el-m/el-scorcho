using System.Threading.Tasks;
using WHRID.Domain.Entities;

namespace WHRID.Application.Interfaces
{
    public interface IWalletRepository
    {
        Task<Wallet?> GetByAddressAsync(string address);
        Task SaveAsync(Wallet wallet);
    }
}
