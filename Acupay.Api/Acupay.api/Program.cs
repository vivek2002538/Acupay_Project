using Acupay.Data;
using Acupay.Data.Repositories;
var builder = WebApplication.CreateBuilder(args);
// var app=builder.Build();
// Database connection
var connectionString = builder.Configuration.GetConnectionString("AcuPayDb")
    ?? throw new InvalidOperationException("AcuPayDb connection string not found.");

builder.Services.AddSingleton(new DbConnectionFactory(connectionString));

// Register repository
builder.Services.AddScoped<InvoiceRepository>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<DbConnectionFactory>();

    using var connection = factory.CreateConnection();
    connection.Open();

    Console.WriteLine("SQL Server connection successful!");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
