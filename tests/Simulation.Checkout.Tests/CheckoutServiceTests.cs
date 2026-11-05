using AutoFixture;
using Bogus;
using FluentAssertions;
using Moq;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using Simulation.Checkout.Domain.Exceptions;
using Simulation.Checkout.Tests.TestData;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Checkout.Tests
{
    public class CheckoutServiceTests
    {
        // Task 1

        // 1.1 
        [Fact]
        public async Task CheckoutAsync_WhenOrderHasNoItems_ShouldThrowEmptyOrderException()
        {
            // Arrange
            var order = new Order
            {
                CustomerName = "Ahmed",
                CustomerEmail = "Ahmed.Mokhtar@gmail.com",
                Items = [],

            };

            var service = new CheckoutService(
                Mock.Of<IOrderRepository>(),
                Mock.Of<IInventoryService>(),
                Mock.Of<INotificationService>());


            // Act
            var act = () => service.CheckoutAsync(order);

            // Assert
            await act.Should().ThrowAsync<EmptyOrderException>();
        }

        // 1.2
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task CheckoutoutAsync_WhenItemQuantityIsInvalid_ShouldThrowInvalidQuantityException(int quantity)
        {
            // Arrange
            const string productName = "Laptop";

            var order = new Order
            {

                CustomerName = "Ahmed",
                CustomerEmail = "Ahmed.Mokhtar@gmail.com",
                Items = [new OrderItem {
                    ProductId = Guid.NewGuid(),
                    ProductName = productName,
                    UnitPrice = 12000m,
                    Quantity = quantity
                }]

            };

            var service = new CheckoutService(
                Mock.Of<IOrderRepository>(),
                Mock.Of<IInventoryService>(),
                Mock.Of<INotificationService>());

            // Act
            var act = () => service.CheckoutAsync(order);


            // Assert
            var exception = await act.Should().ThrowAsync<InvalidQuantityException>();

            exception.Which.Message.Should().Contain(productName);
        }

        // 1.3
        [Fact]
        public async Task CheckoutoutAsync_WhenItemPriceIsInvalid_ShouldThrowInvalidPriceException()
        {
            // Arrange
            var order = new Order
            {

                CustomerName = "Ahmed",
                CustomerEmail = "Ahmed.Mokhtar@gmail.com",
                Items = [new OrderItem {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Laptop",
                    UnitPrice = -0.01m,
                    Quantity = 3
                }]

            };

            var service = new CheckoutService(
                Mock.Of<IOrderRepository>(),
                Mock.Of<IInventoryService>(),
                Mock.Of<INotificationService>());

            // Act
            var act = () => service.CheckoutAsync(order);


            // Assert
            var exception = await act.Should().ThrowAsync<InvalidPriceException>();

        }

        // 1.4
        [Fact]
        public async Task CheckoutoutAsync_WhenItemPriceIsZero_ShouldSucceed()
        {
            //Arrange
            var order = new Order
            {

                CustomerName = "Ahmed",
                CustomerEmail = "Ahmed.Mokhtar@gmail.com",
                Items = [new OrderItem {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Charger",
                    UnitPrice = 0m,
                    Quantity = 3
                }]

            };

            var service = new CheckoutService(
                Mock.Of<IOrderRepository>(),
                Mock.Of<IInventoryService>(inventory =>
            inventory.IsAvailableAsync(
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()) == Task.FromResult(true)),
                Mock.Of<INotificationService>());

            // Act
            var act = () => service.CheckoutAsync(order);


            // Assert
            await act.Should().NotThrowAsync();

        }

        // 1.5
        public static TheoryData<decimal, decimal> DiscountTestData => new()
                    {
                        { 999.99m, 0m },
                        { 1000m, 100m },
                        { 1500m, 100m }
                    };

        [Theory]
        [MemberData(nameof(DiscountTestData))]
        public void CalculateDiscount_WhenSubtotalIsGiven_ShouldReturnExpectedDiscount(decimal subtotal, decimal expectedDiscount)
        {

            // Act
            var discount = CheckoutService.CalculateDiscount(subtotal);

            // Assert
            discount.Should().Be(expectedDiscount);
        }

        // 1.6

        // Example A
        [Fact]
        public async Task CheckoutAsync_WhenSubtotalIsBelowDiscountThreshold_ShouldReturnCorrectReceiptAmounts()
        {
            // Arrange
            var order = new Order
            {
                CustomerName = "Ahmed",
                CustomerEmail = "Ahmed.Mokhtar@gmail.com",
                Items =
                [
                    new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Product A",
                UnitPrice = 150m,
                Quantity = 2
            },
            new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Product B",
                UnitPrice = 400m,
                Quantity = 1
            }
                ]
            };

            var inventoryService = new Mock<IInventoryService>();

            inventoryService
                .Setup(x => x.IsAvailableAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var service = new CheckoutService(
                Mock.Of<IOrderRepository>(),
                inventoryService.Object,
                Mock.Of<INotificationService>());

            // Act
            var receipt = await service.CheckoutAsync(order);

            // Assert
            receipt.Subtotal.Should().Be(700m);
            receipt.Discount.Should().Be(0m);
            receipt.Total.Should().Be(700m);
        }

        // Example C
        [Fact]
        public async Task CheckoutAsync_WhenSubtotalReachDiscountThreshold_ShouldApplyMaximumDiscount()
        {
            // Arrange
            var order = new Order
            {
                CustomerName = "Ahmed",
                CustomerEmail = "Ahmed.Mokhtar@gmail.com",
                Items =
                [
                    new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Product C",
                UnitPrice = 500m,
                Quantity = 3
            }
                ]
            };

            var inventoryService = new Mock<IInventoryService>();

            inventoryService
                .Setup(x => x.IsAvailableAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var service = new CheckoutService(
                Mock.Of<IOrderRepository>(),
                inventoryService.Object,
                Mock.Of<INotificationService>());

            // Act
            var receipt = await service.CheckoutAsync(order);

            // Assert
            receipt.Subtotal.Should().Be(1500m);
            receipt.Discount.Should().Be(100m);
            receipt.Total.Should().Be(1400m);
        }

        // Task 2 

        // 2.1

        [Fact]
        public async Task CheckoutAsync_WhenOrderIsValidAndAllItemsAreInStock_ShouldSaveOrderAndSendConfirmationOnce()
        {
            // Arrange
            var order = new Order
            {
                CustomerName = "Ahmed",
                CustomerEmail = "Ahmed.Mokhtar@gmail.com",
                Items =
                [
                    new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Charger",
                UnitPrice = 100m,
                Quantity = 2
            }
                ]
            };

            var orderRepository = new Mock<IOrderRepository>();

            var inventoryService = new Mock<IInventoryService>();

            inventoryService
                .Setup(x => x.IsAvailableAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var notificationService = new Mock<INotificationService>();

            var service = new CheckoutService(
                orderRepository.Object,
                inventoryService.Object,
                notificationService.Object);

            // Act
            await service.CheckoutAsync(order);

            // Assert
            orderRepository.Verify(
                x => x.SaveAsync(order, It.IsAny<CancellationToken>()),
                Times.Once);

            notificationService.Verify(
                x => x.SendOrderConfirmationAsync(
                    It.IsAny<CheckoutReceipt>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // 2.2
        [Fact]
        public async Task CheckoutAsync_WhenItemIsOutOfStock_ShouldThrowExceptionAndNotSaveOrSend()
        {
            // Arrange
            var order = new Order
            {
                CustomerName = "Ahmed",
                CustomerEmail = "Ahmed.Mokhtar@gmail.com",
                Items =
                [
                    new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Charger",
                UnitPrice = 100m,
                Quantity = 2
            }
                ]
            };

            var orderRepository = new Mock<IOrderRepository>();

            var inventoryService = new Mock<IInventoryService>();

            inventoryService
                .Setup(x => x.IsAvailableAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var notificationService = new Mock<INotificationService>();

            var service = new CheckoutService(
                orderRepository.Object,
                inventoryService.Object,
                notificationService.Object);

            // Act
            var act = () => service.CheckoutAsync(order);

            // Assert
            await act.Should().ThrowAsync<InsufficientInventoryException>();

            orderRepository.Verify(
                x => x.SaveAsync(
                    It.IsAny<Order>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            notificationService.Verify(
                x => x.SendOrderConfirmationAsync(
                    It.IsAny<CheckoutReceipt>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        // 2.3
        [Fact]
        public async Task CheckoutAsync_WhenOrderHasInvalidQuantity_ShouldThrowExceptionWithoutCallingServices()
        {
            // Arrange
            var order = new Order
            {
                CustomerName = "Ahmed",
                CustomerEmail = "Ahmed.Mokhtar@gmail.com",
                Items =
                [
                    new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Charger",
                UnitPrice = 100m,
                Quantity = 0
            }
                ]
            };

            var orderRepository = new Mock<IOrderRepository>();
            var inventoryService = new Mock<IInventoryService>();
            var notificationService = new Mock<INotificationService>();

            var service = new CheckoutService(
                orderRepository.Object,
                inventoryService.Object,
                notificationService.Object);

            // Act
            var act = () => service.CheckoutAsync(order);

            // Assert
            await act.Should().ThrowAsync<InvalidQuantityException>();

            inventoryService.Verify(
                x => x.IsAvailableAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            orderRepository.Verify(
                x => x.SaveAsync(
                    It.IsAny<Order>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            notificationService.Verify(
                x => x.SendOrderConfirmationAsync(
                    It.IsAny<CheckoutReceipt>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        // Task 3 

        // 3.1
        [Fact]
        public async Task CheckoutAsync_WhenOrderIsGeneratedByAutoFixture_ShouldSucceed()
        {
            // Arrange
            var order = OrderTestData.CreateValidOrder();

            var inventoryService = new Mock<IInventoryService>();
            inventoryService
                .Setup(x => x.IsAvailableAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var service = new CheckoutService(
                Mock.Of<IOrderRepository>(),
                inventoryService.Object,
                Mock.Of<INotificationService>());

            // Act
            var receipt = await service.CheckoutAsync(order);

            // Assert
            receipt.Should().NotBeNull();
        }

        // 3.2 
        [Fact]
        public async Task CheckoutAsync_WhenCustomerDataIsGeneratedByBogus_ShouldPreserveCustomerInformation()
        {
            // Arrange
            var customer = CustomerTestDataFactory.Create();

            var order = OrderTestData.CreateValidOrder(customerName: customer.Name, customerEmail: customer.Email);

            var inventoryService = new Mock<IInventoryService>();
            inventoryService
                .Setup(x => x.IsAvailableAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var service = new CheckoutService(
                Mock.Of<IOrderRepository>(),
                inventoryService.Object,
                Mock.Of<INotificationService>());

            // Act
            var receipt = await service.CheckoutAsync(order);

            // Assert
            receipt.CustomerName.Should().Be(customer.Name);
            receipt.CustomerEmail.Should().Be(customer.Email);
        }

        // Task 4 

        [Fact]
        public async Task CheckoutAsync_WhenExampleCIsCheckedOut_ShouldMatchReceiptSnapshot()
        {
            // Arrange
            var order = new Order
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                CustomerName = "Test Customer",
                CustomerEmail = "test@example.com",
                Items =
                [
                    new OrderItem
            {
                ProductId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                ProductName = "Product C",
                UnitPrice = 500m,
                Quantity = 3
            }
                ]
            };

            var inventoryService = new Mock<IInventoryService>();
            inventoryService
                .Setup(x => x.IsAvailableAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var service = new CheckoutService(
                Mock.Of<IOrderRepository>(),
                inventoryService.Object,
                Mock.Of<INotificationService>());

            var settings = new VerifySettings();
            settings.IgnoreMember("CreatedAt");

            // Act
            var receipt = await service.CheckoutAsync(order);

            // Assert
            await Verify(receipt, settings);
        }
    }
}
