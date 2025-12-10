using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WashTrade.Analyzer;
using WashTrade.Domain;
using WashTrade.Domain.Entities;
using Xunit;

namespace WashTrade.Tests
{
    public class DetectiveTests
    {
        private WashTradeDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<WashTradeDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new WashTradeDbContext(options);
        }

        private ILogger<Detective> GetLogger()
        {
            return new LoggerFactory().CreateLogger<Detective>();
        }

        [Fact]
        public async Task DetectSelfFunding_ShouldCreateAlert_WhenPatternExists()
        {
            // Arrange
            using var db = GetInMemoryDbContext();
            var detective = new Detective(GetLogger());

            // 1. Create Wallets
            var walletA = new Wallet { Address = "addr_A" };
            var walletB = new Wallet { Address = "addr_B" };
            db.Wallets.AddRange(walletA, walletB);
            await db.SaveChangesAsync();

            // 2. Create Asset
            var asset = new Asset { PolicyId = "policy1", AssetName = "NFT1" };
            db.Assets.Add(asset);
            await db.SaveChangesAsync();

            // 3. Create Transactions
            // Tx1: A funds B (ADA only) at T-2 hours
            var fundingTx = new Transaction
            {
                TxHash = "tx_fund",
                SenderWalletId = walletA.Id,
                ReceiverWalletId = walletB.Id,
                AssetId = null, // ADA
                Amount = 100,
                Timestamp = DateTime.UtcNow.AddHours(-2)
            };

            // Tx2: B buys Asset from A at T-1 hour
            var saleTx = new Transaction
            {
                TxHash = "tx_sale",
                SenderWalletId = walletB.Id,
                ReceiverWalletId = walletA.Id,
                AssetId = asset.Id, // NFT
                Amount = 100,
                Timestamp = DateTime.UtcNow.AddHours(-1)
            };

            db.Transactions.AddRange(fundingTx, saleTx);
            await db.SaveChangesAsync();

            // Act
            await detective.DetectSelfFunding(db, CancellationToken.None);

            // Assert
            var alert = await db.WashTradeAlerts.FirstOrDefaultAsync();
            Assert.NotNull(alert);
            Assert.Equal(WashTradeType.SelfFunding, alert.Type);
            Assert.Equal(walletA.Id, alert.WalletId); // Wallet A is the suspicious beneficiary
            Assert.Equal(asset.Id, alert.AssetId);
        }

        [Fact]
        public async Task DetectCircularTrading_ShouldCreateAlert_WhenPatternExists()
        {
            // Arrange
            using var db = GetInMemoryDbContext();
            var detective = new Detective(GetLogger());

            var walletA = new Wallet { Address = "addr_A" };
            var walletB = new Wallet { Address = "addr_B" };
            db.Wallets.AddRange(walletA, walletB);
            
            var asset = new Asset { PolicyId = "policy1", AssetName = "NFT1" };
            db.Assets.Add(asset);
            await db.SaveChangesAsync();

            // Tx1: A -> B
            var tx1 = new Transaction
            {
                TxHash = "tx_1",
                SenderWalletId = walletA.Id,
                ReceiverWalletId = walletB.Id,
                AssetId = asset.Id,
                Timestamp = DateTime.UtcNow.AddHours(-5)
            };

            // Tx2: B -> A (Back to Sender)
            var tx2 = new Transaction
            {
                TxHash = "tx_2",
                SenderWalletId = walletB.Id,
                ReceiverWalletId = walletA.Id,
                AssetId = asset.Id,
                Timestamp = DateTime.UtcNow.AddHours(-1)
            };

            db.Transactions.AddRange(tx1, tx2);
            await db.SaveChangesAsync();

            // Act
            await detective.DetectCircularTrading(db, CancellationToken.None);

            // Assert
            var alert = await db.WashTradeAlerts.FirstOrDefaultAsync();
            Assert.NotNull(alert);
            Assert.Equal(WashTradeType.CircularPattern, alert.Type);
            Assert.Equal(walletA.Id, alert.WalletId);
        }
    }
}
