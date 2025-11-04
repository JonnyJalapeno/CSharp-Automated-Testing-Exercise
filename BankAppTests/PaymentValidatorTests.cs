using BankApp;
using NUnit.Framework;
using System;

namespace BankAppTests
{
    [TestFixture]
    public class PaymentValidatorTests
    {
        private PaymentValidator _validator;

        [SetUp]
        public void SetUp()
        {
            _validator = new PaymentValidator();
        }

        [Test]
        public void ValidatePaymentInput_Fails_When_UserId_Is_NullOrEmpty()
        {
            Assert.IsFalse(_validator.ValidatePaymentInput(null, "Vendor123", 100).Success);
            Assert.IsFalse(_validator.ValidatePaymentInput("", "Vendor123", 100).Success);

        }

        [Test]
        public void ValidatePaymentInput_Fails_When_VendorId_Is_NullOrEmpty()
        {
            Assert.IsFalse(_validator.ValidatePaymentInput("User1", null, 100).Success);
            Assert.IsFalse(_validator.ValidatePaymentInput("User1", "", 100).Success);
        }

        [Test]
        public void ValidatePaymentInput_Fails_When_Amount_Is_NonPositive()
        {
            Assert.IsFalse(_validator.ValidatePaymentInput("User1", "Vendor123", 0).Success);
            Assert.IsFalse(_validator.ValidatePaymentInput("User1", "Vendor123", -50).Success);
        }

        [Test]
        public void ValidatePaymentInput_Passes_With_ValidInput()
        {
            Assert.That(_validator.ValidatePaymentInput("User1","Vendor123",100).Success, Is.EqualTo(true));
        }
    }
}
