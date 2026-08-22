namespace Online_Shopping_System.Models.Payment
{
    public class CashPayment : Payment
    {
        public override bool CollectMoney()
        {
            // Generate a unique transaction ID for the payment for testing.
            TransactionId = Guid.NewGuid().ToString();

            return true;
        }
    }
}
