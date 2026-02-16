using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using WWI_ModularKit.BuildingBlocks.Abstractions;
using WWI_ModularKit.Host.Infrastructure;
using WWI_ModularKit.Modules.Sales.Features.Orders.CreateOrder;
using WWI_ModularKit.Modules.Sales.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

// Register Multi-Tenancy
builder.Services.AddScoped<ITenantProvider, HttpTenantProvider>();

// Register MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
    typeof(CreateOrderHandler).Assembly
));
builder.Services.AddValidatorsFromAssembly(typeof(CreateOrderHandler).Assembly);

// Register Database Contexts
builder.Services.AddDbContext<SalesDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register MassTransit
builder.Services.AddMassTransit(x =>
{
    // Configure Transactional Outbox for Sales Module
    x.AddEntityFrameworkOutbox<SalesDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Map Endpoints (In a real app, this would be auto-discovered via Carter or similar)
app.MapPost("/api/sales/orders", async (CreateOrderCommand command, IMediator mediator) =>
{
    var orderId = await mediator.Send(command);
    return Results.Created($"/api/sales/orders/{orderId}", orderId);
})
.WithOpenApi();

app.Run();

public partial class Program { }
