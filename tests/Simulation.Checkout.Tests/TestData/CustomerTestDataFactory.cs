using Bogus;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Checkout.Tests.TestData
{
    public sealed class CustomerTestData
    {
        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
    }

    public static class CustomerTestDataFactory
    {
        public static CustomerTestData Create(int seed = 12345)
        {
            return new Faker<CustomerTestData>()
                .UseSeed(seed)
                .RuleFor(customer => customer.Name, faker => faker.Name.FullName())
                .RuleFor(customer => customer.Email, faker => faker.Internet.Email())
                .Generate();
        }
    }
}
