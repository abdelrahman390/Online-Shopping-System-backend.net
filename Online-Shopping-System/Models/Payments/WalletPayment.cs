namespace Online_Shopping_System.Models.Payment
{
    public class WalletPayment : Payment
    {
        public string WalletNumber { get; set; }
        public string WalletProviderName { get; set; }

        public override bool CollectMoney()
        {
            // Generate a unique transaction ID for the payment for testing.
            TransactionId = Guid.NewGuid().ToString();

            bool ValidNum = WalletNumber.Length == 12;
            bool ValidWalletName = WalletProviderName.Length > 3;
            return ValidNum && ValidWalletName;
        }
    }
}
