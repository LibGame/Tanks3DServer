using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tanks3DServer.MoiraiScripts
{
    internal class Transaction
    {
        public string Address { get; set; }
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public string Txid { get; set; }

        public override bool Equals(object? obj)
        {
            Transaction transaction = obj as Transaction;

            if(transaction == null)
            {
                return false;
            }

            if(transaction.Txid == Txid)
                return true;

            return false;
        }
    }
}
