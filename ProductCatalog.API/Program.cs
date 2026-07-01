using ProductCatalog.Application.Services;
using ProductCatalog.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "ProductCatalog API v1");
    });

}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();