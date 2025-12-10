using WashTrade.Analyzer;
using WashTrade.Domain;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

// Configure Database
builder.Services.AddDbContext<WashTradeDbContext>(options =>
    options.UseNpgsql("Host=localhost;Database=washtrade;Username=admin;Password=password"));

builder.Services.AddScoped<IDetective, Detective>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
