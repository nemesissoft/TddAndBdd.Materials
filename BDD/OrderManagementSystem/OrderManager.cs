﻿using System.Collections.Concurrent;
using Common;

namespace OrderManagementSystem;

public class OrderManager : IOrderManager
{
    private readonly ConcurrentDictionary<string, OmsOrderBase> _orders = new();

    private int OrderId;

    public event NewOrderInsertedEventHandler NewOrderInserted;
    public event OrderChangedEventHandler OrderChanged;

    public bool Acknowledge(string id)
    {
        if (!_orders.TryGetValue(id, out var order)) return false;
        order.Acknowledge();

        OrderChanged?.Invoke(OrderChangeType.Acknowledge, order);
        return true;
    }

    public bool Cancel(string id)
    {
        if (!_orders.TryGetValue(id, out var order)) return false;

        if (!order.Cancel())
            return false;

        OrderChanged?.Invoke(OrderChangeType.Cancellation, order);
        return true;
    }

    public bool Fill(string id, int quantity)
    {
        if (!_orders.TryGetValue(id, out var order)) return false;

        if (!order.Fill(quantity))
            return false;

        OrderChanged?.Invoke(OrderChangeType.Fill, order);
        return true;
    }

    public IEnumerable<IOrder> GetAll() => _orders.Select(order => order.Value).ToArray();

    public IEnumerable<IOrder> GetAllChilds(string id)
    {
        _orders.TryGetValue(id, out var parent);
        return (parent as ParentOmsOrder)?.GetAllChilds();
    }

    public IOrder GetById(string id) => !_orders.TryGetValue(id, out var order) ? null : (IOrder)order;

    public string Insert(
        string Symbol,
        Side Side,
        decimal Price,
        int Quantity,
        TimeInForce TimeInForce,
        string ParentOrderId,
        StrategyTypes Strategy)
    {
        if (string.IsNullOrWhiteSpace(ParentOrderId))
        {
            var parentId = GenerateId();
            var order = new ParentOmsOrder
            {
                Id = parentId,
                Symbol = Symbol,
                ParentOrderId = ParentOrderId,
                Price = Price,
                Quantity = Quantity,
                Side = Side,
                State = State.UNACK,
                TimeInForce = TimeInForce,
                Timestamp = DateTime.Now,
                Strategy = Strategy
            };
            _orders.TryAdd(parentId, order);

            NewOrderInserted?.Invoke(order);

            return parentId;
        }


        if (!_orders.ContainsKey(ParentOrderId))
            return null;
        var id = GenerateId();

        var child = new OmsOrder
        {
            Id = id,
            Symbol = Symbol,
            ParentOrderId = ParentOrderId,
            Price = Price,
            Quantity = Quantity,
            Side = Side,
            State = State.UNACK,
            TimeInForce = TimeInForce,
            Timestamp = DateTime.Now
        };
        _orders.TryAdd(id, child);
        (_orders[ParentOrderId] as ParentOmsOrder).AddChild(child);

        NewOrderInserted?.Invoke(child);
        return id;
    }

    public bool Reject(string id)
    {
        if (!_orders.TryGetValue(id, out var order)) return false;

        if (!order.Reject()) return false;

        OrderChanged?.Invoke(OrderChangeType.Rejection, order);

        return true;
    }

    public bool Update(string id, decimal newPrice, int newQuantity)
    {
        if (!_orders.TryGetValue(id, out var order)) return false;
        if (!order.Update(newPrice, newQuantity))
            return false;

        OrderChanged?.Invoke(OrderChangeType.Modification, order);
        return true;
    }

    private string GenerateId() => (++OrderId).ToString();
}