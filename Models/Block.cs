using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Neptune_Wallet.Models
{
    public class Block
    {
        public string PreviousHash { get; set; }
        public int nonce { get; set; }
        public DateTime TimeStamp { get; set; }
        public int Index { get; set; }
        public List<Transaction> Transactions { get; set; }

        public override string ToString()
        {
            return $"{Index}[{TimeStamp.ToString("yyyy-mm-dd HH:mm:ss")}] Proof:{nonce} | PrevHash : {PreviousHash}".ToString();
        }
    }
}
