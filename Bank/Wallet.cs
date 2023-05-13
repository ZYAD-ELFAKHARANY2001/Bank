using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank
{
    internal class Wallet
    {
        public int WalletID {get; set; }
        public string WalletHolder { get; set; }

        public decimal WalletBalance { get; set; }

        public override string ToString()
        {
            return $"Wallet id: {WalletID}\n Wallet Holder: {WalletHolder}\n Wallet Balance: {WalletBalance}";
        }
    }
}
