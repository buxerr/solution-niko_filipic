using Microsoft.AspNetCore.Authentication;
using ProductCatalog.API.Authentication;
using ProductCatalog.Application.Services;
using ProductCatalog.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();

builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddAuthentication(DummyJsonAuthenticationDefaults.AuthenticationScheme)
    .AddScheme<AuthenticationSchemeOptions, DummyJsonAuthenticationHandler>(
        DummyJsonAuthenticationDefaults.AuthenticationScheme,
        options => { });

builder.Services.AddAuthorization();

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();