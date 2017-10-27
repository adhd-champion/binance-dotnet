using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.objects.api_responses
{
    public class Response_Account : APIResponse
    {
        public int makerCommission { get; set; }
        public int takerCommission { get; set; }
        public int buyerCommission { get; set; }
        public int sellerCommission { get; set; }
        public bool canTrade { get; set; }
        public bool canWithdraw { get; set; }
        public bool canDeposit { get; set; }
        public Balance[] balances { get; set; }
        public class Balance
        {
            public string asset { get; set; }
            public string free { get; set; }
            public string locked { get; set; }
        }
    }

    

}
