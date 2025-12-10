using System.Threading;
using System.Threading.Tasks;
using WashTrade.Domain;

namespace WashTrade.Analyzer
{
    public interface IDetective
    {
        Task DetectCircularTrading(WashTradeDbContext db, CancellationToken ct);
        Task DetectSelfFunding(WashTradeDbContext db, CancellationToken ct);
    }
}
