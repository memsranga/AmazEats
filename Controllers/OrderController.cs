using AmazEats.Entities;
using Microsoft.AspNetCore.Mvc;
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

    public OrderController(ILogger<OrderController> logger, AmazEatsDbContext context)
    {
        _logger = logger;
        _context = context;
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
            Number = 1,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        var createdOrder = await _context.Orders.AddAsync(newOrder);
        await _context.SaveChangesAsync();
        return createdOrder.Entity;
    }
}

