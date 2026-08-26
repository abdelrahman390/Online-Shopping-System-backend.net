using Online_Shopping_System.Models.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Online_Shopping_SystemTests.Unit_testing.Models.Payments
{
    public class CreditCardPaymentTests
    {
        [Fact]
        public void CheckEnteredDataForCreditCardPayment_EnterCorrectCardNumber_ReturnsTrue()
        {
            CreditCardPayment creditCardPayment = new CreditCardPayment
            {
                CardNumber = "1234567890123456"
            };

            Assert.True(creditCardPayment.CollectMoney());
        }

        [Fact]
        public void CheckEnteredDataForCreditCardPayment_EnterCardNumberLessThan16Digits_ReturnsFalse()
        {
            CreditCardPayment creditCardPayment = new CreditCardPayment
            {
                CardNumber = "123456789012345"
            };

            Assert.False(creditCardPayment.CollectMoney());
        }

        [Fact]
        public void CheckEnteredDataForCreditCardPayment_EnterCardNumberMoreThan16Digits_ReturnsFalse()
        {
            CreditCardPayment creditCardPayment = new CreditCardPayment
            {
                CardNumber = "12345678901234567"
            };

            Assert.False(creditCardPayment.CollectMoney());
        }

        [Fact]
        public void CheckEnteredDataForCreditCardPayment_EnterEmptyCardNumber_ReturnsFalse()
        {
            CreditCardPayment creditCardPayment = new CreditCardPayment
            {
                CardNumber = ""
            };

            Assert.False(creditCardPayment.CollectMoney());
        }

        [Fact]
        public void CheckEnteredDataForCreditCardPayment_EnterNullCardNumber_ReturnsFalse()
        {
            CreditCardPayment creditCardPayment = new CreditCardPayment
            {
                CardNumber = null
            };

            Assert.False(creditCardPayment.CollectMoney());
        }
    }
}
