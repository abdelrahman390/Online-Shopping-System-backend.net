namespace Online_Shopping_System.Models.Payment
{
    public class CreditCardPayment : Payment
    {
        public string CardNumber { get; set; }

        public override bool CollectMoney()
        {
            if(CardNumber == null) return false;

            // Generate a unique transaction ID for the payment for testing.
            TransactionId = Guid.NewGuid().ToString();

            return CardNumber.Length == 16;
        }
    }
}
