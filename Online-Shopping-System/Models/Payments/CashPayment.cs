namespace Online_Shopping_System.Models.Payment
{
    public class CashPayment : Payment
    {
        //public int PaymentTypeId { get; set; }
        public override bool CollectMoney()
        {
            // Generate a unique transaction ID for the payment for testing.
            TransactionId = Guid.NewGuid().ToString();

            return true;
        }
    }
}
