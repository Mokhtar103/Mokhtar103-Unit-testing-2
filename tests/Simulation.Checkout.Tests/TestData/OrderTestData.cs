using AutoFixture;
using Simulation.Checkout.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Checkout.Tests.TestData
{
    public static class OrderTestData
    {
        public static Order CreateValidOrder(int itemCount = 3, string? customerName = null,
    string? customerEmail = null)
        {
            var fixture = new Fixture();

            fixture.Customize<OrderItem>(composer => composer
                .With(item => item.Quantity, 1)
                .With(item => item.UnitPrice, 10m));

            var items = fixture.CreateMany<OrderItem>(itemCount).ToList();

            return fixture.Build<Order>()
                .With(order => order.Items, items)
                .With(order => order.CustomerName, customerName ?? "Test Customer")
                .With(order => order.CustomerEmail, customerEmail ?? "test@example.com")
                .Create();
        }
    }
}
