using System;
using System.Linq;
using System.Threading.Tasks;
using WHRID.Application.Interfaces;
using WHRID.Domain.Entities;

namespace WHRID.Application.Services
{
    public class WalletAnalysisService
    {
        private readonly IWalletRepository _walletRepo;
        private readonly RiskScoringService _riskService;
        private readonly IArgusRepository _argusRepo;

        public WalletAnalysisService(IWalletRepository walletRepo, RiskScoringService riskService, IArgusRepository argusRepo)
        {
            _walletRepo = walletRepo;
            _riskService = riskService;
            _argusRepo = argusRepo;
        }

        public async Task<Wallet> AnalyzeWalletAsync(string address)
        {
            var wallet = await _walletRepo.GetByAddressAsync(address);
            if (wallet == null)
            {
                wallet = new Wallet
                {
                    Id = Guid.NewGuid(),
                    Address = address,
                    FirstSeen = DateTime.UtcNow,
                    LastActive = DateTime.UtcNow
                };
                await _walletRepo.SaveAsync(wallet);
            }

            try
            {
                var transactions = await _argusRepo.GetTransactionsByAddressAsync(address);
                if (transactions != null && transactions.Count > 0)
                {
                    var existing = new HashSet<string>(wallet.Transactions.Select(t => t.Hash));
                    foreach (var t in transactions)
                    {
                        if (!existing.Contains(t.Hash))
                        {
                            t.WalletId = wallet.Id;
                            wallet.Transactions.Add(t);
                        }
                    }
                    wallet.FirstSeen = wallet.Transactions.Min(t => t.Timestamp);
                    wallet.LastActive = wallet.Transactions.Max(t => t.Timestamp);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching Argus data: {ex.Message}");
                throw;
            }

            wallet.RiskProfile = _riskService.CalculateRisk(wallet);
            await _walletRepo.SaveAsync(wallet);

            return wallet;
        }
    }
}
