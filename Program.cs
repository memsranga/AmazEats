using AmazEats;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var cosmosConnection = "<<connection-string>>";
var cosmosDatabaseName = "AmazEats";


builder.Services.AddControllers();
builder.Services.AddDbContext<AmazEatsDbContext>(options =>
{
    options.UseCosmos(cosmosConnection, cosmosDatabaseName);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AmazEatsDbContext>();
    //context.Database.EnsureCreated();  // This will ensure the containers are created
    await context.Database.EnsureDeletedAsync();
    await context.Database.EnsureCreatedAsync();
}

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();

