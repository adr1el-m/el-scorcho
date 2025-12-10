using WashTrade.Domain;
using WashTrade.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace WashTrade.Analyzer;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Analyzer running at: {time}", DateTimeOffset.Now);
            }

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<WashTradeDbContext>();
                    var detective = scope.ServiceProvider.GetRequiredService<IDetective>();
                    
                    await detective.DetectCircularTrading(dbContext, stoppingToken);
                    await detective.DetectSelfFunding(dbContext, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during analysis");
            }

            await Task.Delay(10000, stoppingToken); // Run every 10 seconds
        }
    }
}
