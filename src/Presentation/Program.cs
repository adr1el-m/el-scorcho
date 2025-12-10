using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using WHRID.Application.Interfaces;
using WHRID.Application.Services;
using WHRID.Infrastructure.DbContexts;
using WHRID.Infrastructure.Repositories;
using WHRID.Presentation;
using WHRID.Presentation.Components;

using WHRID.Infrastructure.ArgusIntegration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers(); // Add API controllers

// WHRID Database (App Data)
var whridConnString = builder.Configuration.GetConnectionString("WHRIDConnection") ?? "Host=localhost;Database=whrid;Username=postgres;Password=password;Port=5433";
builder.Services.AddDbContextFactory<WHRIDDbContext>(options =>
    options.UseNpgsql(whridConnString));

// Argus Database (Blockchain Data)
var argusConnString = builder.Configuration.GetConnectionString("ArgusConnection") ?? "Host=localhost;Database=argus;Username=postgres;Password=password;Port=5433";
builder.Services.AddDbContextFactory<ArgusDbContext>(options =>
    options.UseNpgsql(argusConnString));

// Application Services
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<IArgusRepository, ArgusRepository>();
builder.Services.AddScoped<RiskScoringService>();
builder.Services.AddScoped<WalletAnalysisService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapControllers(); // Map API controllers

// Ensure DB created
using (var scope = app.Services.CreateScope())
{
    var whridDb = scope.ServiceProvider.GetRequiredService<WHRIDDbContext>();
    whridDb.Database.EnsureCreated();

    // Create Argus DB if not exists
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ArgusDbContext>>();
    using var argusDb = factory.CreateDbContext();
    argusDb.Database.EnsureCreated();
}

app.Run();
