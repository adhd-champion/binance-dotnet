using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace binance_dotnet.objects
{
    public class Response_AggTrades : APIResponse
    {
        public AggTrade[] aggtrades { get; set; }
        public class AggTrade
        {
            public int a { get; set; }
            public string p { get; set; }
            public string q { get; set; }
            public int f { get; set; }
            public int l { get; set; }
            public long T { get; set; }
            public bool m { get; set; }
            public bool M { get; set; }
        }
    }

    

}
