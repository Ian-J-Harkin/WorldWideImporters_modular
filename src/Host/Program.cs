using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using WWI_ModularKit.BuildingBlocks.Abstractions;
using WWI_ModularKit.Host.Infrastructure;
using WWI_ModularKit.Modules.Sales.Features.Orders.CreateOrder;
using WWI_ModularKit.Modules.Sales.Features.Orders.GetOrders;
using WWI_ModularKit.Modules.Sales.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "WWI Modular Kit API", Version = "v1" });
    
    // Define the X-Tenant-Id header security scheme
    c.AddSecurityDefinition("TenantId", new OpenApiSecurityScheme
    {
        Description = "Enter your Tenant ID (Guid). Example: 8db1620a-8640-410a-8651-f0945934188b",
        Name = "X-Tenant-Id",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKey"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "TenantId"
                }
            },
            Array.Empty<string>()
        }
    });
});
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

app.UseCors("DevPolicy");

app.UseHttpsRedirection();

// Map Endpoints (In a real app, this would be auto-discovered via Carter or similar)
app.MapPost("/api/sales/orders", async (CreateOrderCommand command, IMediator mediator) =>
{
    var orderId = await mediator.Send(command);
    return Results.Created($"/api/sales/orders/{orderId}", orderId);
})
.WithOpenApi(operation => 
{
    operation.Summary = "Create a new sales order";
    operation.Description = "Requires X-Tenant-Id header.";
    return operation;
});

app.MapGet("/api/sales/orders", async (IMediator mediator) =>
{
    var result = await mediator.Send(new GetOrdersQuery());
    return Results.Ok(result);
})
.WithOpenApi(operation => 
{
    operation.Summary = "Get all orders for the current tenant";
    operation.Description = "Returns orders filtered by the X-Tenant-Id header. Only orders belonging to the specified tenant will be returned.";
    return operation;
});

app.Run();

public partial class Program { }
