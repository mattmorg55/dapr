using Biscotti.Online.Orders.Abstract;
using Biscotti.Online.Orders.API.State;
using Biscotti.Online.Reservations.Abstract;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Biscotti.Online.Orders.API.Controllers;
[ApiController]
[Route("[controller]")]
public class OrdersController : ControllerBase
{
    private const string StoreName = "orders-sqlstore";
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(ILogger<OrdersController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Method for submitting a new order.
    /// </summary>
    /// <param name="order">Order info.</param>
    /// <param name="daprClient">State client to interact with Dapr runtime.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPost]
    public async Task<ActionResult<Guid>> Post([FromBody] Order order, [FromServices] DaprClient daprClient)
    {
        _logger.LogInformation("Enter submit order");

        order.Id = Guid.NewGuid();

        var state = await daprClient.GetStateEntryAsync<OrderState>(StoreName, order.Id.ToString());
        state.Value ??= new OrderState
        {
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
            Order = order
        };

        foreach (var item in order.Items)
        {
            var data = new Item
            {
                SKU = item.ProductCode,
                Quantity = item.Quantity
            };
            var result = await daprClient.InvokeMethodAsync<Item, Item>(HttpMethod.Post, "reservations-api", "reservations", data);
            _logger.LogInformation("sku: {SKU} === new quantity: {Quantity}", result.SKU, result.Quantity);
        }

        await state.SaveAsync();

        _logger.LogInformation("Submitted order {Id}", order.Id);

        return Ok(order.Id);
    }

    [HttpGet("{state}")]
    public ActionResult<Order> Get([FromState(StoreName)] StateEntry<OrderState> state)
    {
        _logger.LogInformation("Enter order retrieval");
        if (state.Value is null)
        {
            return NotFound();
        }
        var result = state.Value.Order;
        _logger.LogInformation("Retrieved order {Id}", result.Id);
        return Ok(result);
    }
}
