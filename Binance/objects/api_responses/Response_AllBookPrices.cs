using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binance.objects.api_responses
{
    public class Response_AllBookPrices : APIResponse
    {
        public BookPrice[] BookPrices { get; set; }
        public class BookPrice
        {
            public string symbol { get; set; }
            public string bidPrice { get; set; }
            public string bidQty { get; set; }
            public string askPrice { get; set; }
            public string askQty { get; set; }
        }
    }



}
