﻿using Common;
using NUnit.Framework;

namespace Behavioral.Tools.ScenarioHelpers;

public class ScenarioHelper
{
    public string OrderId { get; set; }
    public IEnumerable<IOrder> ChildOrders { get;  set; } = new List<IOrder>();
    public async Task<IOrder> GetLastChild()
    {
        var child = (await Hooks.Hooks.SimulationContext.OrderManagementSystemSimulator.GetAllChilds(OrderId)).Where(c=>(c.State != State.CANCELLED || c.State != State.FILLED || c.State != State.CLOSED || c.State != State.REJECTED)).OrderByDescending(o=>o.Id).FirstOrDefault();
        Assert.IsNotNull(child);
        return child;
    }

    public async Task StoreCurrentChildList() => 
        ChildOrders = await Hooks.Hooks.SimulationContext.OrderManagementSystemSimulator.GetAllChilds(OrderId);
}