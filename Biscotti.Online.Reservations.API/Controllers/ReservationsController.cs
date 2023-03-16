using Biscotti.Online.Reservations.Abstract;
using Biscotti.Online.Reservations.API.State;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Biscotti.Online.Reservations.API.Controllers;
[ApiController]
[Route("[controller]")]
public class ReservationsController : ControllerBase
{
    private const string StoreName = "reservations-sqlstore";

    private readonly ILogger<ReservationsController> _logger;

    public ReservationsController(ILogger<ReservationsController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<Item>> Post([FromBody] Item item, [FromServices] DaprClient client)
    {
        _logger.LogInformation("Enter Reservation");

        var state = await client.GetStateEntryAsync<ItemState>(StoreName, item.SKU);
        state.Value ??= new ItemState
        {
            SKU = item.SKU,
            Changes = new List<ItemReservation>()
        };

        // Update balance
        state.Value.BalanceQuantity -= item.Quantity;

        // Record change
        var change = new ItemReservation
        {
            SKU = item.SKU,
            Quantity = item.Quantity,
            ReservedOn = DateTime.UtcNow
        };
        state.Value.Changes ??= new List<ItemReservation>();
        state.Value.Changes.Add(change);
        if (state.Value.Changes.Count > 10)
        {
            state.Value.Changes.RemoveAt(0);
        }

        // TrySaveAsync leverages ETag while SaveAsync does not
        _logger.LogInformation("ETag {ETag}", state.ETag);
        var saved = await state.TrySaveAsync();
        if (!saved)
        {
            _logger.LogError("Failed to save state");
            return StatusCode(500);
        }

        // Return current balance
        var result = new Item
        {
            SKU = state.Value.SKU,
            Quantity = state.Value.BalanceQuantity
        };
        _logger.LogInformation($"Reservation of {result.SKU} is now {result.Quantity}");
        return Ok(result);
    }

    [HttpGet("{state}")]
    public ActionResult<Item> Get([FromState(StoreName)] StateEntry<ItemState> state)
    {
        _logger.LogInformation("Enter item retrieval");
        if (state.Value is null)
        {
            return NotFound();
        }
        var result = new Item
        {
            SKU = state.Value.SKU,
            Quantity = state.Value.BalanceQuantity
        };
        _logger.LogInformation("Retrieved {SKU} is {Quantity}", result.SKU, result.Quantity);
        return Ok(result);
    }
}
