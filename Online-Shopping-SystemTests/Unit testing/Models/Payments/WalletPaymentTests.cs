using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

using Online_Shopping_System.Models.Payment;

namespace Online_Shopping_System.Tests.Unit_testing.Models.Payments
{
    public class WalletPaymentTests
    {
        [Fact]
        public void CheckEnteredDataForWalletPayment_EnterCorectWalletNumberAndWalletName_ReturnsTrue()
        {
            WalletPayment walletPayment = new WalletPayment
            {
                WalletNumber = "01245678901",
                WalletProviderName = "Vodafone"
            };

            Assert.True(walletPayment.CollectMoney());
        }

        [Fact]
        public void CheckEnteredDataForWalletPayment_IncorrectWalletNumberCorrectWalletName_ReturnsFalse()
        {
            WalletPayment walletPayment = new WalletPayment
            {
                WalletNumber = "0123567890",
                WalletProviderName = "Vodafone"
            };

            Assert.False(walletPayment.CollectMoney());
        }

        [Fact]
        public void CheckEnteredDataForWalletPayment_CorrectWalletNumberIncorrectWalletName_ReturnsFalse()
        {
            WalletPayment walletPayment = new WalletPayment
            {
                WalletNumber = "012345607890",
                WalletProviderName = "aa"
            };

         
            Assert.False(walletPayment.CollectMoney());
        }

        [Fact]
        public void CheckEnteredDataForWalletPayment_IncorrectWalletNumberAndWalletName_ReturnsFalse()
        {
            WalletPayment walletPayment = new WalletPayment
            {
                WalletNumber = "01234560789",
                WalletProviderName = "aa"
            };

            Assert.False(walletPayment.CollectMoney());
        }

        [Fact]
        public void CheckEnteredDataForWalletPayment_nullWalletNumberAndWalletName_ReturnsFalse()
        {
            WalletPayment walletPayment = new WalletPayment
            {
                WalletNumber = null,
                WalletProviderName = null
            };

            Assert.False(walletPayment.CollectMoney());
        }

        [Fact]
        public void CheckEnteredDataForWalletPayment_nullWalletNumberAndCorrectWalletName_ReturnsFalse()
        {
            WalletPayment walletPayment = new WalletPayment
            {
                WalletNumber = null,
                WalletProviderName = "Vodafone"
            };

            Assert.False(walletPayment.CollectMoney());
        }
    }
}
