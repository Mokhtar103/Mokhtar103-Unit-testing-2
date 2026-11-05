using Moq;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Checkout.Tests
{
    public class ReceiptSnapshotTests
    {
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
