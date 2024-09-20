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

    [HttpPost("/new-order")]
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

    [HttpPost]
    public async Task<OrderEntity> CreateUsingEtagsAsync()
    {
        var cafeId = "Cafe 1";
        var today = DateTimeOffset.UtcNow.Date;
        var nextCount = await GetNextOrderNumberAsync(cafeId, today);


        var newOrder = new OrderEntity
        {
            Id = Guid.NewGuid().ToString(),
            CafeId = "Cafe 1",
            CreatedAt = DateTimeOffset.UtcNow,
            Number = nextCount,
        };
        var createdOrder = await _container.CreateItemAsync(newOrder, new PartitionKey(newOrder.CafeId));
        return createdOrder.Resource;
    }

    /// <summary>
    /// Retrieves the next order number for a given cafe on a specific date. 
    /// If no order number exists for the given date, it initializes the count to 1.
    /// </summary>
    /// <param name="cafeId">The unique identifier of the cafe.</param>
    /// <param name="date">The date for which the order number is being generated.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the next order number as a long integer.
    /// </returns>
    /// <remarks>
    /// This method attempts to retrieve the current order number for the specified cafe and date.
    /// If the item does not exist, it initializes the order number to 1. If the item exists, it increments the current number by 1.
    /// In the event of a conflict due to concurrent updates (ETag conflict), the method retries until successful.
    /// </remarks>
    /// <exception cref="CosmosException">
    /// Thrown when there is an error accessing the Cosmos DB container. Handles NotFound exception to initialize the order number.
    /// </exception>
    private async Task<long> GetNextOrderNumberAsync(string cafeId, DateTime date)
    {
        while (true)
        {
            try
            {
                // Attempt to read the current order number entity for the cafe and date.
                var currentCountItem = await _container.ReadItemAsync<OrderNumberEntity>(cafeId + date.ToString("yyyy-MM-dd"), new PartitionKey(cafeId));

                // Increment the current order number and update the entity in the database.
                var newCount = await _container.UpsertItemAsync(new OrderNumberEntity
                {
                    Id = currentCountItem.Resource.Id,
                    CafeId = cafeId,
                    Date = date,
                    CurrentNumber = currentCountItem.Resource.CurrentNumber + 1,
                }, new PartitionKey(cafeId), new ItemRequestOptions
                {
                    IfMatchEtag = currentCountItem.ETag
                });

                // Return the new incremented order number.
                return newCount.Resource.CurrentNumber;
            }
            catch (CosmosException cosmosException)
            {
                // If the item does not exist, initialize the order number to 1.
                if (cosmosException.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await _container.CreateItemAsync(new OrderNumberEntity
                    {
                        Id = cafeId + date.ToString("yyyy-MM-dd"),
                        CafeId = cafeId,
                        Date = date,
                        CurrentNumber = 1,
                    }, new PartitionKey(cafeId));

                    return 1;
                }
            }
            catch (Exception exception)
            {
                // Log any other exceptions.
                Console.WriteLine(exception.Message);
            }
        }
    }
}

