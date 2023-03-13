using Biscotti.Online.Orders.API.Models;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Biscotti.Online.Orders.API.Controllers;
[ApiController]
[Route("[controller]")]
public class OrdersController : ControllerBase
{
    /// <summary>
    /// Method for submitting a new order.
    /// </summary>
    /// <param name="order">Order info.</param>
    /// <param name="daprClient">State client to interact with Dapr runtime.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPost]
    public async Task<ActionResult<Guid>> SubmitOrder(Order order, [FromServices] DaprClient daprClient)
    {
        Console.WriteLine("Enter submit order");

        order.Id = Guid.NewGuid();

        foreach (var item in order.Items)
        {
            /* a dynamic type is passed to sample.microservice.reservation and not
            the Order in scope of sample.microservice.order, you could use DTO instead */
            var data = new { sku = item.ProductCode, quantity = item.Quantity };
            var result = await daprClient.InvokeMethodAsync<object, dynamic>(HttpMethod.Post, "reservations-api", "reservations", data);
            Console.WriteLine($"sku: {result.GetProperty("value").GetProperty("sku")} === new quantity: {result.GetProperty("value").GetProperty("quantity")}");
        }

        Console.WriteLine($"Submitted order {order.Id}");

        return order.Id;
    }
}
