using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WashTrade.Domain;
using Argus.Indexer;

var builder = Host.CreateApplicationBuilder(args);

// Configure Database
builder.Services.AddDbContext<WashTradeDbContext>(options =>
    options.UseNpgsql("Host=localhost;Database=washtrade;Username=admin;Password=password"));

// Register the Indexer Service (Simulating Argus)
builder.Services.AddHostedService<IndexerWorker>();

var host = builder.Build();

// Apply migrations at startup
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WashTradeDbContext>();
    // db.Database.EnsureCreated(); // Or Migrate()
    // We will use migrations
}

host.Run();
