using NetArchTest.Rules;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using FluentAssertions;

namespace Simulation.Checkout.Tests
{
    // Task 5
    public class ArchitectureTests
    {
        private static readonly Assembly[] ProductionAssemblies =
   [
        typeof(Order).Assembly,
        typeof(CheckoutService).Assembly,
        Assembly.Load("Simulation.Checkout.Infrastructure")
   ];

        [Fact]
        public void Domain_WhenCheckingDependencies_ShouldNotDependOnApplicationOrInfrastructure()
        {
            // Arrange
            var domainAssembly = typeof(Order).Assembly;

            // Act
            var result = Types.InAssembly(domainAssembly)
                .That()
                .ResideInNamespace("Simulation.Checkout.Domain")
                .ShouldNot()
                .HaveDependencyOn("Simulation.Checkout.Application")
                .And()
                .NotHaveDependencyOn("Simulation.Checkout.Infrastructure")
                .GetResult();

            // Assert
            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Application_WhenCheckingDependencies_ShouldNotDependOnInfrastructure()
        {
            // Arrange
            var applicationAssembly = typeof(CheckoutService).Assembly;

            // Act
            var result = Types.InAssembly(applicationAssembly)
                .That()
                .ResideInNamespace("Simulation.Checkout.Application")
                .ShouldNot()
                .HaveDependencyOn("Simulation.Checkout.Infrastructure")
                .GetResult();

            // Assert
            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Interfaces_WhenCheckingNames_ShouldStartWithI()
        {

            // Act
            var results = ProductionAssemblies
                .Select(assembly => Types.InAssembly(assembly)
                    .That()
                    .AreInterfaces()
                    .Should()
                    .HaveNameStartingWith("I")
                    .GetResult());

            // Assert
            results.Should().OnlyContain(result => result.IsSuccessful);
        }

        [Fact]
        public void IOrderRepositoryImplementations_WhenCheckingNames_ShouldEndWithRepository()
        {

            // Act
            var results = ProductionAssemblies
                .Select(assembly => Types.InAssembly(assembly)
                    .That()
                    .ImplementInterface(typeof(IOrderRepository))
                    .Should()
                    .HaveNameEndingWith("Repository")
                    .GetResult());

            // Assert
            results.Should().OnlyContain(result => result.IsSuccessful);
        }
    }
}
