using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace binance_dotnet.objects.api_responses
{
    public class Response_MyTrades : APIResponse
    {
        public trade[] trades { get; set; }
        public class trade
        {
            public int id { get; set; }
            public string price { get; set; }
            public string qty { get; set; }
            public string commission { get; set; }
            public string commissionAsset { get; set; }
            public long time { get; set; }
            public bool isBuyer { get; set; }
            public bool isMaker { get; set; }
            public bool isBestMatch { get; set; }
        }
    }
}
