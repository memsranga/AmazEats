﻿using AmazEats.Entities;
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

    private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
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
        await semaphore.WaitAsync();
        try
        {
            var allOrderCount = await _context.Orders.CountAsync();
            var newOrder = new OrderEntity
            {
                Id = Guid.NewGuid().ToString(),
                Number = allOrderCount + 1,
                CreatedAt = DateTimeOffset.UtcNow,
            };
            var createdOrder = await _context.Orders.AddAsync(newOrder);
            await _context.SaveChangesAsync();
            return createdOrder.Entity;
        }
        finally
        {
            semaphore.Release();
        }
    }
}

