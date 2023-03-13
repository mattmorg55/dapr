﻿using System;
using System.Collections.Generic;

namespace Biscotti.Online.Orders.API.Models;

public class Order
{
    public DateTime Date { get; set; }

    public Guid Id { get; set; }

    public string? CustomerCode { get; set; }

    public List<OrderItem> Items { get; set; }
}

public class OrderItem
{
    public string ProductCode { get; set; }
    public int Quantity { get; set; }
}
