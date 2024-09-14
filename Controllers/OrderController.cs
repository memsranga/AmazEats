using AmazEats.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;

namespace AmazEats.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<OrderController> _logger;
    private readonly AmazEatsDbContext _context;
    private readonly Container _container;

    public OrderController(ILogger<OrderController> logger, AmazEatsDbContext context, CosmosClient client)
    {
        _logger = logger;
        _context = context;
        _container = client.GetContainer("AmazEats", "Orders");
    }

    [HttpGet]
    public async Task<IEnumerable<OrderEntity>> GetAsync()
    {
        return await _context.Orders.ToListAsync();
    }

    [HttpPost]
    public async Task<OrderEntity> CreateAsync()
    {
        var newOrder = new OrderEntity
        {
            Id = Guid.NewGuid().ToString(),
            CafeId = "Cafe 1",
            CreatedAt = DateTimeOffset.UtcNow,
        };
        var createdOrder = await _container.Scripts.ExecuteStoredProcedureAsync<OrderEntity>("insertOrder", new PartitionKey(newOrder.CafeId), new dynamic[] { newOrder });
        return createdOrder.Resource;
    }
}

