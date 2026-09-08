namespace Online_Shopping_System.Models.Payment
{
    public class WalletPayment : Payment
    {
        public string WalletNumber { get; set; }
        public string WalletProviderName { get; set; }

        public override bool CollectMoney()
        {
            if (WalletNumber == null || WalletProviderName == null) return false;
            // Generate a unique transaction ID for the payment for testing.
            TransactionId = Guid.NewGuid().ToString();

            bool ValidNum = WalletNumber.Length == 11;
            bool ValidWalletName = WalletProviderName.Length > 3;
            return ValidNum && ValidWalletName;
        }
    }
}
