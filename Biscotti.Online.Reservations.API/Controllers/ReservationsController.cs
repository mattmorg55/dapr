using Biscotti.Online.Reservations.API.Models;
using Dapr.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Biscotti.Online.Reservations.API.Controllers;
[ApiController]
[Route("[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly ILogger<ReservationsController> _logger;

    public ReservationsController(ILogger<ReservationsController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public IResult SubmitReservation([FromServices] DaprClient client, [FromBody] Item item)
    {
        _logger.LogInformation("Enter Reservation");

        /* a specific type is used in sample.microservice.reservation and not
        reused the class in sample.microservice.order with the same signature: 
        this is just to not introduce DTO and to suggest that it might be a good idea
        having each service separating the type for persisting store */
        Item storedItem;
        storedItem = new Item();
        storedItem.SKU = item.SKU;
        storedItem.Quantity -= item.Quantity;

        _logger.LogInformation($"Reservation of {storedItem.SKU} is now {storedItem.Quantity}");

        return Results.Ok(storedItem);
    }
}
