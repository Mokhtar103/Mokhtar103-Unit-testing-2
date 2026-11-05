## Answers for Unit Testing Assignment

1- Total Tests : 21
Passed : 21
Failed : 0

2- Line coverage: 90.5%

3- Moq Used in Task 2 to mock interfaces and verify depedency interactions
AutoFixture Used in Task 3 to automatically generate valid Order and OrderItem test data
Bogus Used in Task 3 to generate realistic, deterministic customer names and email addresses for testing

4- No, it's not fair, a customer paying 999.99 pays 999.99, while a customer spending 1,000 pays only 900 after the 100 discount

5- Pricing Bug: applying a 10% discount immediately at 1,000 creates a pricing gap, where a customer spending more can pay less than one spending less