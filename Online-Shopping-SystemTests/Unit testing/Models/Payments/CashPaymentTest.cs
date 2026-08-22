using Xunit;

using Online_Shopping_System.Models.Payment;

namespace Online_Shopping_System.Tests.Unit_testing.Models.Payments
{
    public class CashPaymentTest
    {
        [Fact]
        public void CheckinteredDataForCashPayment_ReturnsTrue()
        {
            CashPayment cashPayment = new CashPayment();
            
            Assert.True(cashPayment.CollectMoney());
        }

    }
}

