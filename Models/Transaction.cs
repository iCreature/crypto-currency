using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Neptune_Wallet.Models
{
    public class Transaction
    {
        // add transaction components(properties)
        public decimal Amount { get; set; }
        public string Recipent { get; set; }
        public string Sender { get; set; }
        public string Signature { get; set; }
        public decimal Fees { get; set; }

        public override string ToString()
        {
            return Amount.ToString("0.000000") + Recipent+ Sender;
        }


    }
}
