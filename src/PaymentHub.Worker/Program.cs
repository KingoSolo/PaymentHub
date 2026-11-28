using Microsoft.EntityFrameworkCore;
using PaymentHub.Shared.Data;
using PaymentHub.Worker;
using PaymentHub.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("PaymentHub.Shared")));
builder.Services.AddHostedService<OutboxPublisher>();

var host = builder.Build();
host.Run();
