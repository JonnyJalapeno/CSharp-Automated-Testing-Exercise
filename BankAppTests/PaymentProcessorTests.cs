using BankApp;
using Moq;
using NUnit.Framework;
using System;

namespace BankAppTests
{
    [TestFixture]
    public class PaymentProcessorTests
    {
        private Mock<IVendorDatabase> _vendorDbMock;
        private Mock<ISslService> _sslMock;
        private Mock<IUserConsentService> _consentMock;
        private PaymentValidator _validator;
        private PaymentProcessor _processor;

        [SetUp]
        public void SetUp()
        {
            _vendorDbMock = new Mock<IVendorDatabase>();
            _sslMock = new Mock<ISslService>();
            _consentMock = new Mock<IUserConsentService>();
            _validator = new PaymentValidator();

            _processor = new PaymentProcessor(
                _vendorDbMock.Object,
                _sslMock.Object,
                _consentMock.Object,
                _validator
            );
        }

        [Test]
        public void ProcessPayment_Calls_All_Services_When_Valid()
        {
            // Arrange mocks to "succeed" (do nothing)
            _vendorDbMock.Setup(v => v.EnsureVendorExists("Vendor123"))
             .Returns(PaymentResult.Ok());

            _sslMock.Setup(s => s.PerformHandshake("Vendor123"))
                    .Returns(PaymentResult.Ok());

            _consentMock.Setup(c => c.VerifyUserConsent("User1", 100))
                        .Returns(PaymentResult.Ok());

            // Act + Assert
            var result = _processor.ProcessPayment("User1", "Vendor123", 100);
            Assert.That(result.Success, Is.EqualTo(true));

            // Verify mocks were called correctly
            _vendorDbMock.Verify(v => v.EnsureVendorExists("Vendor123"), Times.Once);
            _sslMock.Verify(s => s.PerformHandshake("Vendor123"), Times.Once);
            _consentMock.Verify(c => c.VerifyUserConsent("User1", 100), Times.Once);
        }

        [Test]
        public void ProcessPayment_Fails_If_VendorService_Fails()
        {
            _vendorDbMock.Setup(v => v.EnsureVendorExists("Vendor123"))
             .Returns(PaymentResult.Fail("Vendor not found"));

            var result = _processor.ProcessPayment("User1", "Vendor123", 100);
            Assert.IsFalse(result.Success);
            Assert.That(result.ErrorMessage, Is.EqualTo("Vendor not found"));
        }

        [Test]
        public void ProcessPayment_Fails_If_SslService_Fails()
        {
            _vendorDbMock.Setup(v => v.EnsureVendorExists("Vendor123"))
             .Returns(PaymentResult.Ok());
            _sslMock.Setup(s => s.PerformHandshake("Vendor123"))
                .Returns(PaymentResult.Fail("SSL handshake failure"));

            var result = _processor.ProcessPayment("User1", "Vendor123", 100);
            Assert.IsFalse(result.Success);
            Assert.That(result.ErrorMessage, Is.EqualTo("SSL handshake failure"));
        }

        [Test]
        public void ProcessPayment_Fails_If_UserConsent_Fails()
        {
            _vendorDbMock.Setup(v => v.EnsureVendorExists("Vendor123"))
            .Returns(PaymentResult.Ok());
            _sslMock.Setup(s => s.PerformHandshake("Vendor123"))
                .Returns(PaymentResult.Ok());
            _consentMock.Setup(s => s.VerifyUserConsent("User1",100))
                .Returns(PaymentResult.Fail("User empty or amount is wrong"));

            var result = _processor.ProcessPayment("User1", "Vendor123", 100);
            Assert.IsFalse(result.Success);
            Assert.That(result.ErrorMessage, Is.EqualTo("User empty or amount is wrong"));
        }
    }
}
