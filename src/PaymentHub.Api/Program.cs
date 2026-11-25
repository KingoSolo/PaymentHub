using Microsoft.EntityFrameworkCore;
using PaymentHub.Shared.Data;
using PaymentHub.Shared.Entities;
using PaymentHub.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("PaymentHub.Shared")));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.MapBatchEndpoints();
app.Run();

