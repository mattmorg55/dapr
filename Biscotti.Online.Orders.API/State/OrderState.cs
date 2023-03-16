using Biscotti.Online.Orders.Abstract;
using System;

namespace Biscotti.Online.Orders.API.State;

public class OrderState
{
    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public Order? Order { get; set; }
}
