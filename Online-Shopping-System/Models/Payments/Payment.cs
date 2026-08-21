using Online_Shopping_System.Models.Carts;
using Online_Shopping_System.Models.Orders;

namespace Online_Shopping_System.Models.Payment
{
    public abstract class Payment
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public float Amount { get; set; }
        public string TransactionId { get; set; }
        public DateTimeOffset? Date { get; set; }

        // PaymentTypeNames: Cash - CreditCard - Wallet
        //public string PaymentTypeName { get; set; } = "Cash";

        // Navigation property
        public Order Order { get; set; }

        //private static readonly List<string> PaymentTypes =
        //    ["Cash", "CreditCard", "Wallet"];

        public abstract bool CollectMoney();
        //{
        //    bool validPaymentType = PaymentTypes.Contains(PaymentTypeName);

        //    return validPaymentType;
        //}

    }
}
