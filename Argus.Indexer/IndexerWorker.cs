using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Argus.Indexer
{
    public class IndexerWorker : BackgroundService
    {
        private readonly ILogger<IndexerWorker> _logger;

        public IndexerWorker(ILogger<IndexerWorker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Argus Indexer Service is starting.");

            // TODO: Initialize Argus here.
            // Argus usually connects to a Cardano Node (via Ouroboros or similar).
            // Example:
            // var argus = new ArgusClient(...);
            // argus.OnTransaction += HandleTransaction;
            // await argus.StartAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Argus Indexer running... listening for blocks.");
                
                // Simulate processing
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
